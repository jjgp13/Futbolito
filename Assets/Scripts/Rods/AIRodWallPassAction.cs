using UnityEngine;

/// <summary>
/// AI Rod Wall Pass Action - Manages wall pass execution for AI-controlled rods
/// 
/// REFACTORED ARCHITECTURE: CONDITION-BASED EVALUATION
/// 
/// PHILOSOPHY:
/// Wall Pass is NOT a state - it's an action triggered when conditions are met.
/// 
/// WALL PASS CONDITIONS (No Probability, Pure Logic):
/// 1. Ball is in wall pass range (inside figure's trigger collider)
/// 2. AIGoalEvaluator found NO clear shot path (shots are blocked)
/// 3. Rod is active
/// 
/// PURPOSE:
/// - Create confusion and unpredictable ball movement when shots are blocked
/// - Bounce ball off wall to create new opportunities
/// - Force opponent to react and potentially open shooting lanes
/// 
/// EVALUATED BY:
/// AIGoalEvaluator.EvaluateWallPassOpportunity() - Called by PositioningState
/// 
/// KEY DIFFERENCES FROM OLD SYSTEM:
/// - ? NO probability checks
/// - ? NO reaction delays
/// - ? NO periodic evaluation (called by PositioningState instead)
/// - ? Condition-based: wall pass if AIGoalEvaluator recommends it
/// - ? Immediate response to conditions
/// - ? Works alongside positioning
/// 
/// SIMILAR TO:
/// PlayerRodWallPassAction - But with automatic evaluation instead of input-based
/// </summary>
[RequireComponent(typeof(AIRodMovementAction))]
public class AIRodWallPassAction : MonoBehaviour
{
    #region Configuration

    [Header("Wall Pass Configuration")]
    [Tooltip("Force applied to ball during wall pass")]
    [SerializeField] private float wallPassForce = 10f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    #endregion

    #region References

    private AIRodMovementAction rodMovement;
    private AIRodStateMachine stateMachine;
    private AIGoalEvaluator goalEvaluator;
    private FoosballFigureAnimationController[] figures;
    private FoosballFigureWallPassAction[] wallPassActions;
    private GameObject ball;

    #endregion

    #region State

    private bool wallPassExecutedRecently = false;
    private float wallPassCooldownTimer = 0f;
    private const float WALL_PASS_COOLDOWN = 1.0f; // Prevent spam

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        rodMovement = GetComponent<AIRodMovementAction>();
        stateMachine = GetComponent<AIRodStateMachine>();
        goalEvaluator = GetComponent<AIGoalEvaluator>();

        CollectFigures();
    }

    private void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Ball");
        ConfigureFigureWallPass();
    }

    private void Update()
    {
        if (ball == null)
        {
            ball = GameObject.FindGameObjectWithTag("Ball");
        }

        // Update cooldown timer
        if (wallPassExecutedRecently)
        {
            wallPassCooldownTimer += Time.deltaTime;
            if (wallPassCooldownTimer >= WALL_PASS_COOLDOWN)
            {
                wallPassExecutedRecently = false;
                wallPassCooldownTimer = 0f;
            }
        }
    }

    #endregion

    #region Initialization

    private void CollectFigures()
    {
        int childCount = transform.childCount;
        figures = new FoosballFigureAnimationController[childCount];
        wallPassActions = new FoosballFigureWallPassAction[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            figures[i] = child.GetComponent<FoosballFigureAnimationController>();
            wallPassActions[i] = child.GetComponent<FoosballFigureWallPassAction>();

            // Add FoosballFigureWallPassAction component if it doesn't exist
            if (wallPassActions[i] == null && figures[i] != null)
            {
                wallPassActions[i] = child.gameObject.AddComponent<FoosballFigureWallPassAction>();
            }

            // Configure wall pass force
            if (wallPassActions[i] != null)
            {
                wallPassActions[i].wallPassForce = this.wallPassForce;
            }
        }
    }

    private void ConfigureFigureWallPass()
    {
        foreach (var wallPassAction in wallPassActions)
        {
            if (wallPassAction != null)
            {
                wallPassAction.wallPassForce = this.wallPassForce;
            }
        }
    }

    #endregion

    #region Wall Pass Evaluation (CONDITION-BASED, NO PROBABILITY)

    /// <summary>
    /// Evaluates if wall pass should be executed
    /// Called by PositioningState when AIGoalEvaluator recommends wall pass
    /// 
    /// RETURNS: True if wall pass was executed, false otherwise
    /// </summary>
    public bool EvaluateAndExecuteWallPass()
    {
        if (!rodMovement.isActive || ball == null)
        {
            AIDebugLogger.LogWallPass(gameObject.name, false, "Rod inactive or no ball");
            return false;
        }

        // Check cooldown (prevent spam)
        if (wallPassExecutedRecently)
        {
            AIDebugLogger.LogWallPass(gameObject.name, false, $"On cooldown ({wallPassCooldownTimer:F1}s / {WALL_PASS_COOLDOWN}s)");
            if (showDebugInfo)
            {
                Debug.Log($"[AIRodWallPassAction] {gameObject.name}: Wall pass on cooldown");
            }
            return false;
        }

        // Find figure that can perform wall pass
        int figureIndex = FindFigureForWallPass();

        if (figureIndex < 0)
        {
            return false;
        }

        // Execute wall pass
        ExecuteWallPass(figureIndex);
        return true;
    }

    /// <summary>
    /// Finds the figure that can perform wall pass
    /// 
    /// CONDITIONS:
    /// 1. Figure's wall pass action has ball in trigger collider
    /// 2. Wall pass is physically possible (CanPerformWallPass())
    /// 
    /// Returns -1 if no figure can perform wall pass
    /// </summary>
    private int FindFigureForWallPass()
    {
        for (int i = 0; i < wallPassActions.Length; i++)
        {
            if (wallPassActions[i] != null && wallPassActions[i].CanPerformWallPass())
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[AIRodWallPassAction] {gameObject.name}: Figure {i} can perform wall pass");
                }
                return i;
            }
        }

        return -1;
    }

    #endregion

    #region Wall Pass Execution

    /// <summary>
    /// Executes wall pass on specific figure
    /// IMMEDIATE - no delays
    /// </summary>
    private void ExecuteWallPass(int figureIndex)
    {
        if (figureIndex < 0 || figureIndex >= wallPassActions.Length) return;

        FoosballFigureWallPassAction wallPassAction = wallPassActions[figureIndex];
        if (wallPassAction == null || !wallPassAction.CanPerformWallPass())
            return;

        // Perform wall pass
        wallPassAction.PerformWallPass();

        // Set cooldown
        wallPassExecutedRecently = true;
        wallPassCooldownTimer = 0f;

        AIDebugLogger.LogWallPass(gameObject.name, true, $"Executed on figure {figureIndex}");

        if (showDebugInfo)
        {
            Debug.Log($"[AIRodWallPassAction] {gameObject.name}: Wall pass executed on figure {figureIndex}");
        }

        // Optional: Trigger FSM cooldown state
        // This prevents other actions immediately after wall pass
        if (stateMachine != null)
        {
            stateMachine.ChangeState<CooldownState>();
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Sets wall pass force (called by AITeamRodsController for difficulty)
    /// </summary>
    public void SetWallPassForce(float force)
    {
        wallPassForce = force;
        ConfigureFigureWallPass();
    }

    /// <summary>
    /// Gets whether wall pass is on cooldown
    /// </summary>
    public bool IsOnCooldown()
    {
        return wallPassExecutedRecently;
    }

    /// <summary>
    /// Checks if any figure can perform wall pass right now
    /// </summary>
    public bool CanPerformWallPass()
    {
        if (wallPassExecutedRecently) return false;

        for (int i = 0; i < wallPassActions.Length; i++)
        {
            if (wallPassActions[i] != null && wallPassActions[i].CanPerformWallPass())
            {
                return true;
            }
        }

        return false;
    }

    #endregion
}

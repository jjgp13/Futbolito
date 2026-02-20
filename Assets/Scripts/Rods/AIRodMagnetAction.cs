using UnityEngine;

/// <summary>
/// AI Rod Magnet Action - Manages magnet activation for AI-controlled rods
/// 
/// REFACTORED ARCHITECTURE: CONTINUOUS CONDITION-BASED EVALUATION
/// 
/// PHILOSOPHY:
/// Magnet is NOT a state - it's an action that runs continuously in parallel with positioning.
/// The AI evaluates magnet conditions every frame and activates/deactivates based on CONDITIONS, not probability.
/// 
/// ACTIVATION CONDITIONS (No Probability, Pure Logic):
/// 1. Rod is active
/// 2. Ball is inside the magnet trigger collider (PointEffector2D range)
/// 3. Ball velocity is LOW (below threshold) - fast balls should not be attracted
/// 4. Ball is NOT already in shootable position (if ball is ready to shoot, don't attract)
/// 5. Ball is NOT too close (prevent over-attraction)
/// 
/// PURPOSE:
/// - Attract ball closer to figure when ball is nearby but not in shooting position
/// - Position ball in front of figure for optimal shooting
/// - Works in PARALLEL with rod movement
/// 
/// KEY DIFFERENCES FROM OLD SYSTEM:
/// - ❌ NO probability checks
/// - ❌ NO reaction delays
/// - ❌ NO state machine integration
/// - ✅ Continuous evaluation every frame
/// - ✅ Immediate response to conditions
/// - ✅ Works alongside PositioningState
/// 
/// SIMILAR TO:
/// PlayerRodMagnetAction - But with automatic evaluation instead of input-based
/// </summary>
[RequireComponent(typeof(AIRodMovementAction))]
public class AIRodMagnetAction : MonoBehaviour
{
    #region Configuration

    [Header("Magnet Configuration")]
    [Tooltip("Maximum ball velocity to activate magnet (if ball is moving fast, don't attract)")]
    [SerializeField] private float maxBallVelocityForMagnet = 6f;

    [Tooltip("Attraction force applied by magnet (negative = attraction)")]
    [SerializeField] private float attractionForce = -10f;

    [Tooltip("Minimum distance to keep magnet on (prevents over-attraction)")]
    [SerializeField] private float minimumMagnetDistance = 0.3f;

    [Tooltip("Distance threshold to consider ball in 'shootable position'")]
    [SerializeField] private float shootableDistanceThreshold = 2.0f;

    [Header("Magnet Timing")]
    [Tooltip("Maximum duration magnet can stay active without contact (seconds)")]
    [SerializeField] private float maxMagnetDuration = 999f;

    [Tooltip("Minimum time magnet stays active once activated (seconds). Prevents instant release.")]
    [SerializeField] private float minimumMagnetHoldTime = 0.3f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    #endregion

    #region References

    private AIRodMovementAction rodMovement;
    private AIRodStateMachine stateMachine;
    private AIRodShootAction shootAction;
    private FoosballFigureAnimationController[] figures;
    private FoosballFigureMagnetAction[] magnetActions;
    private GameObject ball;
    private Rigidbody2D ballRigidbody;

    #endregion

    #region State

    private bool isMagnetActive = false;
    private float magnetActiveTime = 0f;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        rodMovement = GetComponent<AIRodMovementAction>();
        stateMachine = GetComponent<AIRodStateMachine>();
        shootAction = GetComponent<AIRodShootAction>();

        CollectFigures();
    }

    private void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Ball");
        if (ball != null)
        {
            ballRigidbody = ball.GetComponent<Rigidbody2D>();
        }

        ConfigureFigureMagnets();
    }

    private void Update()
    {
        // Only evaluate if rod is active
        if (!rodMovement.isActive)
        {
            DeactivateMagnet();
            return;
        }

        // Find ball if lost
        if (ball == null)
        {
            ball = GameObject.FindGameObjectWithTag("Ball");
            if (ball != null)
            {
                ballRigidbody = ball.GetComponent<Rigidbody2D>();
            }
            else
            {
                DeactivateMagnet();
                return;
            }
        }

        // Evaluate magnet conditions EVERY FRAME (continuous evaluation)
        EvaluateAndUpdateMagnet();
    }

    private void OnDisable()
    {
        DeactivateMagnet();
    }

    #endregion

    #region Initialization

    private void CollectFigures()
    {
        int childCount = transform.childCount;
        figures = new FoosballFigureAnimationController[childCount];
        magnetActions = new FoosballFigureMagnetAction[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            figures[i] = child.GetComponent<FoosballFigureAnimationController>();
            magnetActions[i] = child.GetComponentInChildren<FoosballFigureMagnetAction>();
        }
    }

    private void ConfigureFigureMagnets()
    {
        foreach (var magnetAction in magnetActions)
        {
            if (magnetAction != null)
            {
                magnetAction.attractionForce = attractionForce;
            }
        }
    }

    #endregion

    #region Magnet Evaluation (CONDITION-BASED, NO PROBABILITY)

    /// <summary>
    /// Continuously evaluates whether magnet should be active
    /// Called every frame when rod is active
    /// 
    /// NO PROBABILITY - Pure condition-based logic
    /// TACTICAL: Only activate magnet when there's a clear attract→position→shoot intent
    /// </summary>
    private void EvaluateAndUpdateMagnet()
    {
        // Check timeout if magnet is active
        if (isMagnetActive)
        {
            magnetActiveTime += Time.deltaTime;
            if (magnetActiveTime > maxMagnetDuration)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[AIRodMagnetAction] {gameObject.name} - Magnet timed out after {maxMagnetDuration}s");
                }
                DeactivateMagnet();
                return;
            }
        }

        // Don't activate magnet while rod is charging a shot — effector fights the kick
        if (shootAction != null && shootAction.IsCharging())
        {
            DeactivateMagnet();
            return;
        }

        // Tactical check: don't use magnet if opponent has possession
        // But respect minimum hold time if magnet is already active
        AITeamRodsController teamController = GetComponentInParent<AITeamRodsController>();
        if (teamController != null && teamController.CurrentPossession == AITeamRodsController.BallPossession.Opponent)
        {
            if (!isMagnetActive || magnetActiveTime >= minimumMagnetHoldTime)
            {
                if (isMagnetActive)
                    AIDebugLogger.LogMagnet(gameObject.name, false, "Opponent has possession");
                DeactivateMagnet();
                return;
            }
        }

        // Find which figure (if any) should have magnet active
        int figureIndex = FindFigureForMagnet();

        if (figureIndex >= 0)
        {
            // Activate magnet on ALL figures (matching player rod behavior)
            if (!isMagnetActive)
            {
                ActivateMagnet();
            }
        }
        else if (!isMagnetActive || magnetActiveTime >= minimumMagnetHoldTime)
        {
            // Only deactivate if minimum hold time has passed (or magnet wasn't active)
            DeactivateMagnet();
        }
    }

    /// <summary>
    /// Finds the best figure to activate magnet on
    /// Returns -1 if no figure should have magnet active
    /// 
    /// CONDITIONS CHECKED:
    /// 1. Ball velocity is low
    /// 2. Ball is in magnet range
    /// 3. Ball is NOT in shootable position
    /// 4. Ball is NOT too close
    /// </summary>
    private int FindFigureForMagnet()
    {
        if (ball == null || ballRigidbody == null) return -1;

        Vector2 ballPosition = ball.transform.position;
        Vector2 ballVelocity = ballRigidbody.linearVelocity;
        float ballSpeed = ballVelocity.magnitude;

        // Determine if ball is behind any reachable figure — if so, use relaxed speed limit
        // because the magnet's PURPOSE is to capture and redirect the ball to the front
        bool ballBehindAnyFigure = IsBallBehindAnyFigure(ballPosition);
        float effectiveSpeedLimit = ballBehindAnyFigure ? maxBallVelocityForMagnet * 4f : maxBallVelocityForMagnet;

        // CONDITION 1: Check if ball is moving too fast
        if (ballSpeed > effectiveSpeedLimit)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[AIRodMagnetAction] {gameObject.name} - Ball moving too fast ({ballSpeed:F2} > {effectiveSpeedLimit:F2})");
            }
            return -1;
        }

        // Find figure closest to ball that meets all conditions
        int bestFigureIndex = -1;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < magnetActions.Length; i++)
        {
            if (magnetActions[i] == null || figures[i] == null)
                continue;

            Vector2 figurePosition = figures[i].transform.position;
            float distanceToBall = Vector2.Distance(figurePosition, ballPosition);

            // CONDITION 2: Check if ball is within magnet range
            CircleCollider2D magnetCollider = magnetActions[i].GetComponent<CircleCollider2D>();
            float magnetRange = magnetCollider != null ? magnetCollider.radius : 1.5f;

            if (distanceToBall > magnetRange)
                continue;

            // CONDITION 3: Check if ball is in "shootable position" - if yes, don't use magnet
            if (IsBallInShootablePosition(figures[i].transform, ballPosition))
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[AIRodMagnetAction] {gameObject.name} - Ball already in shootable position for figure {i}");
                }
                continue;
            }

            // CONDITION 4: Check if ball is too close (prevent over-attraction)
            if (distanceToBall < minimumMagnetDistance)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[AIRodMagnetAction] {gameObject.name} - Ball too close to figure {i} ({distanceToBall:F2} < {minimumMagnetDistance})");
                }
                continue;
            }

            // This figure is a valid candidate
            if (distanceToBall < closestDistance)
            {
                closestDistance = distanceToBall;
                bestFigureIndex = i;
            }
        }

        return bestFigureIndex;
    }

    /// <summary>
    /// Checks if ball is behind any figure on this rod (between figure and own goal).
    /// Used to relax speed threshold — magnet should capture balls coming from behind.
    /// </summary>
    private bool IsBallBehindAnyFigure(Vector2 ballPosition)
    {
        if (stateMachine == null) return false;
        TeamSide teamSide = stateMachine.TeamSide;

        for (int i = 0; i < figures.Length; i++)
        {
            if (figures[i] == null) continue;
            Vector2 figPos = figures[i].transform.position;

            if (teamSide == TeamSide.LeftTeam)
            {
                if (ballPosition.x < figPos.x) return true; // Ball to the left = behind for left team
            }
            else
            {
                if (ballPosition.x > figPos.x) return true; // Ball to the right = behind for right team
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if ball is already in shootable position relative to figure
    /// 
    /// SHOOTABLE POSITION CRITERIA:
    /// 1. Ball is CLOSE (distance < shootableDistanceThreshold)
    /// 2. Ball is IN FRONT of figure (based on team attack direction)
    ///    - Left team attacks RIGHT: ball.x >= figure.x
    ///    - Right team attacks LEFT: ball.x <= figure.x
    /// 
    /// If ball is in shootable position, NO NEED to use magnet
    /// </summary>
    private bool IsBallInShootablePosition(Transform figureTransform, Vector2 ballPosition)
    {
        if (stateMachine == null) return false;

        Vector2 figurePos = figureTransform.position;
        float distanceToBall = Vector2.Distance(figurePos, ballPosition);

        // Check distance
        if (distanceToBall > shootableDistanceThreshold)
            return false;

        // Check if ball is "in front" based on team side
        TeamSide teamSide = stateMachine.TeamSide;
        bool ballInFront = false;

        if (teamSide == TeamSide.LeftTeam)
        {
            // AI attacking RIGHT, ball should be to the right of or equal to figure
            ballInFront = ballPosition.x >= figurePos.x;
        }
        else // RightTeam
        {
            // AI attacking LEFT, ball should be to the left of or equal to figure
            ballInFront = ballPosition.x <= figurePos.x;
        }

        return ballInFront;
    }

    #endregion

    #region Magnet Control

    /// <summary>
    /// Activates magnet on all figures
    /// IMMEDIATE - no delays
    /// </summary>
    private void ActivateMagnet()
    {
        if (isMagnetActive) return;

        isMagnetActive = true;
        magnetActiveTime = 0f;

        AIDebugLogger.LogMagnet(gameObject.name, true, "Ball slow + in range + not shootable");

        // Activate magnet on all figures (matching player rod behavior)
        foreach (var figure in figures)
        {
            if (figure != null)
            {
                figure.SetMagnetState(true);
            }
        }

        if (showDebugInfo)
        {
            Debug.Log($"[AIRodMagnetAction] {gameObject.name} - Magnet activated on all figures");
        }
    }

    /// <summary>
    /// Deactivates magnet on all figures
    /// </summary>
    public void DeactivateMagnet()
    {
        if (!isMagnetActive) return;

        AIDebugLogger.LogMagnet(gameObject.name, false, $"Active for {magnetActiveTime:F2}s");

        isMagnetActive = false;
        magnetActiveTime = 0f;
        foreach (var figure in figures)
        {
            if (figure != null)
            {
                figure.SetMagnetState(false);
            }
        }

        if (showDebugInfo)
        {
            Debug.Log($"[AIRodMagnetAction] {gameObject.name} - Magnet deactivated");
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Sets attraction force (called by AITeamRodsController for difficulty)
    /// </summary>
    public void SetAttractionForce(float force)
    {
        attractionForce = force;
        ConfigureFigureMagnets();
    }

    /// <summary>
    /// Sets shootable distance threshold (called by AITeamRodsController)
    /// </summary>
    public void SetShootableDistanceThreshold(float threshold)
    {
        shootableDistanceThreshold = threshold;
    }

    public void SetMinimumMagnetHoldTime(float holdTime)
    {
        minimumMagnetHoldTime = holdTime;
    }

    public void SetMaxBallVelocityForMagnet(float velocity)
    {
        maxBallVelocityForMagnet = velocity;
    }

    /// <summary>
    /// Gets whether magnet is currently active
    /// </summary>
    public bool IsMagnetActive()
    {
        return isMagnetActive;
    }

    /// <summary>
    /// Sets the maximum magnet duration before timeout (called by AITeamRodsController for difficulty)
    /// </summary>
    public void SetMaxMagnetDuration(float duration)
    {
        maxMagnetDuration = Mathf.Max(0.1f, duration);
    }

    #endregion
}
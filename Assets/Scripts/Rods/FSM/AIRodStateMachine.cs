using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Priority levels for AI actions — higher priority can interrupt lower priority
/// </summary>
public enum AIActionPriority
{
    Idle = 0,
    Positioning = 1,
    Magnet = 2,
    WallPass = 3,
    Shoot = 4,
    DefensiveIntercept = 5
}

/// <summary>
/// Main Finite State Machine controller for AI rod behavior
/// 
/// REFACTORED ARCHITECTURE: HYBRID FSM + INDEPENDENT ACTIONS
/// 
/// NEW PHILOSOPHY (Ball-Centric, Role-Agnostic):
/// ================================================================
/// 1. ALL RODS ARE EQUAL - No special behavior for Goalkeeper vs Attacker
/// 2. BALL-CENTRIC DECISIONS - Everything based on where ball is and where it's going
/// 3. ACTIONS ARE TOOLS - Shoot/Magnet/WallPass are just means to score or defend
/// 4. UNIFIED STRATEGY - Defend when threatened, attack when opportunity exists
/// 
/// SIMPLIFIED FSM STATES (Strategic Decision Making):
/// ================================================================
/// - IdleState: Rod is inactive, waiting for activation
/// - PositioningState: Rod actively positioning based on ball context (UNIFIED)
/// - ShootingState: Executing the shot
/// - CooldownState: Recovery period after action
/// 
/// WHAT CHANGED FROM OLD ARCHITECTURE:
/// ================================================================
/// REMOVED:
/// - TrackingState (merged into PositioningState)
/// - DefendingState (merged into PositioningState)
/// - Role-specific behavior (all rods use same logic)
/// 
/// KEPT:
/// - Action states that need timing (ChargingShot, WallPass)
/// - Cooldown for rate limiting
/// - Goal-oriented decision making in EvaluatingActionState
/// 
/// NEW UNIFIED APPROACH:
/// ================================================================
/// PositioningState analyzes ball and chooses movement mode:
/// - Ball threatening own goal? ? Defensive positioning
/// - Ball in contested area? ? Track ball
/// - Ball moving toward opponent goal? ? Attacking positioning
/// 
/// The SAME STATE handles all scenarios. No separate Defending/Attacking states.
/// Movement modes (DefensiveBlocking, AttackingPosition) are just strategies,
/// not separate states.
/// 
/// BENEFITS OF NEW ARCHITECTURE:
/// ================================================================
/// ? Simpler mental model (fewer states)
/// ? All rods behave consistently
/// ? Ball-centric logic is clearer
/// ? Easier to extend and maintain
/// ? More realistic foosball behavior
/// ? Actions can potentially be combined in future (magnet + movement)
/// 
/// PROGRAMMING CONCEPTS USED:
/// - Finite State Machine Pattern: Manages strategic states
/// - State Pattern: Encapsulates behavior in state objects
/// - Composition: FSM composed of multiple state objects
/// - Delegation: Delegates behavior to current active state
/// - Centralized Control: Single point of control for all AI actions
/// - Strategy Pattern: Different movement strategies via MovementMode
/// </summary>
[RequireComponent(typeof(AIRodMovementAction))]
[RequireComponent(typeof(AIGoalEvaluator))]
public class AIRodStateMachine : MonoBehaviour
{
    #region Configuration

    [Header("FSM Configuration")]
    [Tooltip("Enable to use FSM, disable to use legacy individual action scripts")]
    public bool useFSM = true;

    [Header("Goal-Oriented Decision Making")]
    [Tooltip("Minimum shooting score (0-1) to attempt a shot")]
    [SerializeField] private float minimumShootScore = 0.4f;

    [Tooltip("Minimum pass advantage to prefer passing over shooting")]
    [SerializeField] private float minimumPassAdvantage = 0.2f;

    [Header("Detection Configuration")]
    [SerializeField] private float detectionDistance = 2f;
    [SerializeField] private float detectionAngle = 60f;

    [Header("Action Configuration")]
    [SerializeField] private float maxChargeTime = 3f;
    [SerializeField] private float lightShotThreshold = 1.0f;
    [SerializeField] private float mediumShotThreshold = 2.0f;
    [SerializeField] private float wallPassForce = 10f;
    [SerializeField] private float attractionForce = -5f;

    [Header("AI Decision Making")]
    [SerializeField] private float decisionInterval = 0.3f;
    [SerializeField] private float cooldownDuration = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [Tooltip("Current state name (read-only)")]
    [SerializeField] private string currentStateName = "None";

    #endregion

    #region References

    private AIRodMovementAction rodMovement;
    private GameObject ball;
    private AIGoalEvaluator goalEvaluator;

    // Figure components
    private FoosballFigureAnimationController[] figures;
    private FoosballFigureShootAction[] shootActions;
    private FoosballFigureWallPassAction[] wallPassActions;
    private FoosballFigureMagnetAction[] magnetActions;

    // Action components (NEW - Parallel execution)
    private AIRodMagnetAction rodMagnetAction;
    private AIRodShootAction rodShootAction;
    private AIRodWallPassAction rodWallPassAction;

    #endregion

    #region State Management

    // All possible states
    private Dictionary<System.Type, AIRodState> states;
    private AIRodState currentState;

    // State instances (SIMPLIFIED - Action states removed)
    private IdleState idleState;
    private PositioningState positioningState; // UNIFIED positioning state
    private ShootingState shootingState; // Only for animation lock
    private CooldownState cooldownState; // Rate limiting after actions

    // Removed states (actions now run in parallel):
    // - EvaluatingActionState (logic moved to PositioningState)
    // - ChargingShotState (converted to AIRodShootAction component)
    // - WallPassState (converted to AIRodWallPassAction component)
    // - MagnetState (converted to AIRodMagnetAction component)
    // - DefendingState (merged into PositioningState)

    #endregion

    #region Priority Override System

    /// <summary>
    /// Current action priority level
    /// </summary>
    public AIActionPriority CurrentActionPriority { get; private set; } = AIActionPriority.Idle;

    /// <summary>
    /// Whether the priority override system is enabled (set per difficulty)
    /// </summary>
    [Header("Priority Override")]
    [SerializeField] private bool priorityOverrideEnabled = false;

    /// <summary>
    /// Attempts to interrupt the current action with a higher-priority one.
    /// Returns true if the interrupt succeeded.
    /// </summary>
    public bool TryInterruptWithPriority(AIActionPriority newPriority)
    {
        if (!priorityOverrideEnabled) return false;
        if (newPriority <= CurrentActionPriority) return false;

        // Cancel current action using existing cleanup paths
        switch (CurrentActionPriority)
        {
            case AIActionPriority.Shoot:
                if (rodShootAction != null)
                    rodShootAction.StopCharging();
                break;
            case AIActionPriority.Magnet:
                if (rodMagnetAction != null)
                    rodMagnetAction.DeactivateMagnet();
                break;
        }

        CurrentActionPriority = newPriority;

        if (showDebugInfo)
        {
            AIDebugLogger.Log(gameObject.name, "FSM", $"Priority override to {newPriority}");
        }

        return true;
    }

    /// <summary>
    /// Sets the current action priority (called by action components)
    /// </summary>
    public void SetActionPriority(AIActionPriority priority)
    {
        CurrentActionPriority = priority;
    }

    #endregion

    #region Difficulty Parameters

    [Header("Difficulty Settings (auto-configured from AITeamRodsController)")]
    [Tooltip("These values are set by parent AITeamRodsController - Edit there for changes")]
    [SerializeField] private int aiDifficultyLevel = 1; // 0=Easy, 1=Medium, 2=Hard

    // These are set by AITeamRodsController
    public float ShootProbability { get; private set; } = 0.6f;
    public float WallPassProbability { get; private set; } = 0.5f;
    public float MagnetProbability { get; private set; } = 0.5f;
    public float ReactionDelay { get; private set; } = 0.2f;
    public float ChargeTimeMultiplier { get; private set; } = 0.75f;
    private float attractionForceValue = -10f; // Changed from -0.5f to match default
    private float wallPassForceValue = 10f;

    #endregion

    #region Properties (Public getters for states to access)

    public AIRodMovementAction RodMovement => rodMovement;
    public GameObject Ball => ball;
    public AIGoalEvaluator GoalEvaluator => goalEvaluator;
    public FoosballFigureAnimationController[] Figures => figures;
    public FoosballFigureShootAction[] ShootActions => shootActions;
    public FoosballFigureWallPassAction[] WallPassActions => wallPassActions;
    public FoosballFigureMagnetAction[] MagnetActions => magnetActions;

    public float DetectionDistance => detectionDistance;
    public float DetectionAngle => detectionAngle;
    public float MaxChargeTime => maxChargeTime;
    public float LightShotThreshold => lightShotThreshold;
    public float MediumShotThreshold => mediumShotThreshold;
    public float WallPassForce => wallPassForce;
    public float AttractionForce => attractionForce;
    public float DecisionInterval => decisionInterval;
    public float CooldownDuration => cooldownDuration;

    public float MinimumShootScore => minimumShootScore;
    public float MinimumPassAdvantage => minimumPassAdvantage;

    public TeamSide TeamSide { get; private set; }
    public bool ShowDebugInfo => showDebugInfo;
    public bool IsGoalkeeper { get; private set; }

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        rodMovement = GetComponent<AIRodMovementAction>();
        goalEvaluator = GetComponent<AIGoalEvaluator>();

        if (goalEvaluator == null)
        {
            goalEvaluator = gameObject.AddComponent<AIGoalEvaluator>();
        }

        // Get or add action components
        rodMagnetAction = GetComponent<AIRodMagnetAction>();
        if (rodMagnetAction == null)
        {
            rodMagnetAction = gameObject.AddComponent<AIRodMagnetAction>();
        }

        rodShootAction = GetComponent<AIRodShootAction>();
        if (rodShootAction == null)
        {
            rodShootAction = gameObject.AddComponent<AIRodShootAction>();
        }

        rodWallPassAction = GetComponent<AIRodWallPassAction>();
        if (rodWallPassAction == null)
        {
            rodWallPassAction = gameObject.AddComponent<AIRodWallPassAction>();
        }

        CollectFigures();
        InitializeStates();
    }

    private void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Ball");

        // Get team side
        var teamController = GetComponentInParent<AITeamRodsController>();
        if (teamController != null)
        {
            TeamSide = teamController.teamSide;
        }

        // Detect if this rod is the goalkeeper
        IsGoalkeeper = gameObject.name == "GoalKepperRod";

        // Start in idle state
        ChangeState<IdleState>();
    }

    private void Update()
    {
        if (!useFSM) return;

        // Find ball if lost
        if (ball == null)
        {
            ball = GameObject.FindGameObjectWithTag("Ball");
        }

        // Update current state
        currentState?.Update();

        // Update debug info
        if (showDebugInfo)
        {
            currentStateName = currentState?.GetStateName() ?? "None";
        }
    }

    private void FixedUpdate()
    {
        if (!useFSM) return;

        currentState?.FixedUpdate();
    }

    private void OnDisable()
    {
        // Clean up current state
        currentState?.Exit();
        currentState = null;
    }

    #endregion

    #region Initialization

    private void CollectFigures()
    {
        int childCount = transform.childCount;
        figures = new FoosballFigureAnimationController[childCount];
        shootActions = new FoosballFigureShootAction[childCount];
        wallPassActions = new FoosballFigureWallPassAction[childCount];
        magnetActions = new FoosballFigureMagnetAction[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            figures[i] = child.GetComponent<FoosballFigureAnimationController>();

            // Get or add action components
            shootActions[i] = child.GetComponent<FoosballFigureShootAction>();
            if (shootActions[i] == null && figures[i] != null)
            {
                shootActions[i] = child.gameObject.AddComponent<FoosballFigureShootAction>();
            }

            wallPassActions[i] = child.GetComponent<FoosballFigureWallPassAction>();
            if (wallPassActions[i] == null && figures[i] != null)
            {
                wallPassActions[i] = child.gameObject.AddComponent<FoosballFigureWallPassAction>();
                wallPassActions[i].wallPassForce = wallPassForce;
            }

            magnetActions[i] = child.GetComponentInChildren<FoosballFigureMagnetAction>();
            if (magnetActions[i] != null)
            {
                magnetActions[i].attractionForce = attractionForce;
            }
        }
    }

    private void InitializeStates()
    {
        // Create state instances
        states = new Dictionary<System.Type, AIRodState>();

        // Create simplified state set (actions are now components, not states)
        idleState = new IdleState(this);
        positioningState = new PositioningState(this); // UNIFIED positioning state
        shootingState = new ShootingState(this); // Only for animation lock
        cooldownState = new CooldownState(this); // Rate limiting

        // Register states
        states[typeof(IdleState)] = idleState;
        states[typeof(PositioningState)] = positioningState;
        states[typeof(ShootingState)] = shootingState;
        states[typeof(CooldownState)] = cooldownState;

        if (showDebugInfo)
        {
            AIDebugLogger.Log(gameObject.name, "FSM", "Initialized with REFACTORED ARCHITECTURE: Idle, Positioning, Shooting, Cooldown");
        }
    }

    #endregion

    #region State Management Methods

    /// <summary>
    /// Changes to a new state
    /// PROGRAMMING CONCEPT: State Pattern - Encapsulates state-specific behavior
    /// </summary>
    public void ChangeState<T>() where T : AIRodState
    {
        if (!useFSM) return;

        // Exit current state
        currentState?.Exit();

        // Get new state
        System.Type stateType = typeof(T);
        if (states.ContainsKey(stateType))
        {
            currentState = states[stateType];
            currentState.Enter();

            if (showDebugInfo)
            {
                AIDebugLogger.Log(gameObject.name, "FSM", $"State changed to: {currentState.GetStateName()}");
            }
        }
        else
        {
            Debug.LogWarning($"State not found: {stateType}", gameObject);
        }
    }

    /// <summary>
    /// Called by states to request a transition to another state
    /// PROGRAMMING CONCEPT: Delegation - Delegates state transitions to the manager
    /// </summary>
    public void RequestTransition(AIRodState nextState)
    {
        if (!useFSM) return;

        // Exit current state
        currentState?.Exit();

        // Transition to next state
        currentState = nextState;
        currentState.Enter();

        if (showDebugInfo)
        {
            AIDebugLogger.Log(gameObject.name, "FSM", $"State transitioned to: {currentState.GetStateName()}");
        }
    }

    /// <summary>
    /// Forcefully set the current state (debug only)
    /// PROGRAMMING CONCEPT: Composition - FSM composed of states, can replace parts at runtime
    /// </summary>
    public void DebugSetState(AIRodState newState)
    {
        if (!useFSM) return;

        // Exit current state
        currentState?.Exit();

        // Set new state
        currentState = newState;
        currentState.Enter();

        if (showDebugInfo)
        {
            AIDebugLogger.Log(gameObject.name, "FSM", $"State set to: {currentState.GetStateName()}");
        }
    }

    /// <summary>
    /// Gets the current state (useful for debugging)
    /// </summary>
    public AIRodState GetCurrentState()
    {
        return currentState;
    }

    /// <summary>
    /// Checks if currently in a specific state
    /// </summary>
    public bool IsInState<T>() where T : AIRodState
    {
        return currentState != null && currentState.GetType() == typeof(T);
    }

    #endregion

    #region Difficulty Configuration API (called by AITeamRodsController)

    /// <summary>
    /// Sets action probabilities from parent controller
    /// </summary>
    public void SetShootProbability(float value) => ShootProbability = Mathf.Clamp01(value);
    public void SetWallPassProbability(float value) => WallPassProbability = Mathf.Clamp01(value);
    public void SetMagnetProbability(float value) => MagnetProbability = Mathf.Clamp01(value);

    /// <summary>
    /// Sets timing parameters from parent controller
    /// </summary>
    public void SetReactionDelay(float value) => ReactionDelay = Mathf.Max(0f, value);
    public void SetChargeTimeMultiplier(float value)
    {
        ChargeTimeMultiplier = Mathf.Clamp(value, 0.1f, 2f);

        // Update shoot action component
        if (rodShootAction != null)
        {
            rodShootAction.SetChargeTimeMultiplier(ChargeTimeMultiplier);
        }
    }
    public void SetDecisionInterval(float value) => decisionInterval = Mathf.Max(0.1f, value);

    /// <summary>
    /// Sets goal evaluation thresholds from parent controller
    /// </summary>
    public void SetMinimumShootScore(float value) => minimumShootScore = Mathf.Clamp01(value);
    public void SetMinimumPassAdvantage(float value) => minimumPassAdvantage = Mathf.Clamp01(value);

    /// <summary>
    /// Sets physical parameters from parent controller
    /// </summary>
    public void SetAttractionForce(float value)
    {
        attractionForceValue = value;
        attractionForce = value;

        // Update all magnet actions (figures)
        foreach (var magnetAction in magnetActions)
        {
            if (magnetAction != null)
            {
                magnetAction.attractionForce = value;
            }
        }

        // Update rod magnet action component
        if (rodMagnetAction != null)
        {
            rodMagnetAction.SetAttractionForce(value);
        }
    }

    public void SetWallPassForce(float value)
    {
        wallPassForceValue = value;
        wallPassForce = value;

        // Update all wall pass actions (figures)
        foreach (var wallPassAction in wallPassActions)
        {
            if (wallPassAction != null)
            {
                wallPassAction.wallPassForce = value;
            }
        }

        // Update rod wall pass action component
        if (rodWallPassAction != null)
        {
            rodWallPassAction.SetWallPassForce(value);
        }
    }

    /// <summary>
    /// Sets shootable distance threshold for magnet and shoot actions
    /// </summary>
    public void SetShootableDistanceThreshold(float value)
    {
        if (rodMagnetAction != null)
        {
            rodMagnetAction.SetShootableDistanceThreshold(value);
        }

        if (rodShootAction != null)
        {
            rodShootAction.SetShootableDistanceThreshold(value);
        }
    }

    /// <summary>
    /// Sets whether the priority override system is enabled
    /// </summary>
    public void SetPriorityOverrideEnabled(bool enabled)
    {
        priorityOverrideEnabled = enabled;
    }

    /// <summary>
    /// Sets interrupt settings on the shoot action component
    /// </summary>
    public void SetInterruptEnabled(bool enabled)
    {
        if (rodShootAction != null)
            rodShootAction.SetInterruptEnabled(enabled);
    }

    public void SetInterruptDistanceMultiplier(float multiplier)
    {
        if (rodShootAction != null)
            rodShootAction.SetInterruptDistanceMultiplier(multiplier);
    }

    public void SetInterruptBallSpeedThreshold(float threshold)
    {
        if (rodShootAction != null)
            rodShootAction.SetInterruptBallSpeedThreshold(threshold);
    }

    public void SetChargeAdaptationEnabled(bool enabled)
    {
        if (rodShootAction != null)
            rodShootAction.SetChargeAdaptationEnabled(enabled);
    }

    public void SetMaxMagnetDuration(float duration)
    {
        if (rodMagnetAction != null)
            rodMagnetAction.SetMaxMagnetDuration(duration);
    }

    /// <summary>
    /// Gets current difficulty level (for display/debugging)
    /// </summary>
    public int GetDifficultyLevel() => aiDifficultyLevel;

    #endregion
}

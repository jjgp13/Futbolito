using UnityEngine;

/// <summary>
/// AI Team Rods Controller - Manages which AI rods are active based on ball reachability
/// 
/// ENHANCED TWO-HAND CONTROL SYSTEM:
/// Simulates realistic foosball play where AI controls 2 rods simultaneously:
/// - Defensive Configuration: Goalkeeper + Defense (when defending)
/// - Balanced Configuration: Defense + Midfield (ball in center)
/// - Attacking Configuration: Midfield + Attacker (when attacking)
/// 
/// INTELLIGENT ROD SELECTION:
/// - Analyzes ball position relative to field zones
/// - Switches configurations dynamically based on game situation
/// - Coordinates both active rods for defensive coverage or attacking pressure
/// 
/// ACTIVATION STRATEGY (UPDATED):
/// Instead of using fixed X-position thresholds, this controller now uses a reach-based system:
/// - Calculates if ball is within reach considering:
///   * Rod's X position
/// * Rod's Y movement range (rodMovementLimit)
///   * Figure collider radius (magnet detection range)
/// - Keeps rods active even if ball passes behind, as long as ball remains reachable
/// - Only deactivates when ball is completely out of reach
/// 
/// INTEGRATION WITH FSM:
/// - Activates/deactivates rods based on ball reachability
/// - FSM states respond to rod activation status
/// - When rod.isActive = true, FSM transitions from Idle to Tracking/Defending
/// - When rod.isActive = false, FSM transitions to Idle
/// 
/// This controller focuses on WHICH rods are active
/// The FSM focuses on WHAT those active rods do
/// </summary>
public class AITeamRodsController : MonoBehaviour
{
    [Header("Team Configuration")]
    [Tooltip("Which side this AI team is on")]
    public TeamSide teamSide;

    [Header("Rod References")]
    [Tooltip("Reference to rods gameobjects [0]=GK, [1]=Defense, [2]=Midfield, [3]=Attack")]
    public GameObject[] rods = new GameObject[4];

    [Header("Visual Indicators")]
    [Tooltip("UI sprites to show which line is active")]
    public SpriteRenderer[] rodsIndicators = new SpriteRenderer[4];
    public Sprite inactiveRodUISprite;
    public Sprite activeRodUISprite;

    [Header("Reach-Based Activation Settings")]
    [Tooltip("Additional buffer distance added to reach calculation for smoother transitions")]
    [SerializeField] private float reachBuffer = 0.5f;

    [Header("Two-Hand Control Configuration")]
    [Tooltip("Defensive zone threshold (ball X position relative to own goal)")]
    [SerializeField] private float defensiveZoneThreshold = 0.3f; // 30% of field from own goal

    [Tooltip("Attacking zone threshold (ball X position relative to opponent goal)")]
    [SerializeField] private float attackingZoneThreshold = 0.3f; // 30% of field from opponent goal

    [Header("=== CENTRALIZED AI DIFFICULTY CONFIGURATION ===")]
    [Header("Difficulty Level")]
    [Tooltip("AI Difficulty: 1=Easy, 2=Medium, 3=Hard (auto-detected from MatchInfo)")]
    [SerializeField] private int aiDifficultyLevel = 2; // Default to medium

    [Header("Action Probabilities (0-1)")]
    [Tooltip("Probability of attempting a shot when opportunity exists")]
    [Range(0f, 1f)]
    [SerializeField] private float shootProbability = 0.6f;

    [Tooltip("Probability of attempting a wall pass")]
    [Range(0f, 1f)]
    [SerializeField] private float wallPassProbability = 0.5f;

    [Tooltip("Probability of using magnet to control ball")]
    [Range(0f, 1f)]
    [SerializeField] private float magnetProbability = 0.5f;

    [Header("Reaction & Timing")]
    [Tooltip("Delay before AI reacts to opportunities (seconds)")]
    [SerializeField] private float reactionDelay = 0.2f;

    [Tooltip("Multiplier for shot charge time (higher = stronger shots)")]
    [Range(0.1f, 2f)]
    [SerializeField] private float chargeTimeMultiplier = 0.75f;

    [Tooltip("How often AI evaluates actions (seconds)")]
    [SerializeField] private float decisionInterval = 0.3f;

    [Header("Goal Evaluation")]
    [Tooltip("Minimum shooting score to attempt shot (0-1)")]
    [Range(0f, 1f)]
    [SerializeField] private float minimumShootScore = 0.4f;

    [Tooltip("Minimum advantage required to pass instead of shoot")]
    [Range(0f, 1f)]
    [SerializeField] private float minimumPassAdvantage = 0.2f;

    [Header("Physical Parameters")]
    [Tooltip("Magnet attraction force (negative = attraction)")]
    [SerializeField] private float attractionForce = -10f;

    [Tooltip("Wall pass force strength")]
    [SerializeField] private float wallPassForce = 10f;

    [Tooltip("Max distance from figure to ball to consider shootable (must match actual figure spacing ~1.6+)")]
    [SerializeField] private float shootableDistanceThreshold = 2.0f;

    [Header("Defensive Strategy Weights")]
    [Tooltip("Zone Defense weight (covers areas)")]
    [Range(0f, 1f)]
    [SerializeField] private float zoneDefenseWeight = 0.4f;

    [Tooltip("Man Marking weight (tracks opponent)")]
    [Range(0f, 1f)]
    [SerializeField] private float manMarkingWeight = 0.4f;

    [Tooltip("Anticipation weight (intercepts)")]
    [Range(0f, 1f)]
    [SerializeField] private float anticipationWeight = 0.2f;

    [Header("Interrupt & Adaptation Settings")]
    [Tooltip("Enable charge interruption when ball moves away")]
    [SerializeField] private bool interruptEnabled = true;

    [Tooltip("Distance multiplier beyond shootable threshold to trigger interrupt")]
    [SerializeField] private float interruptDistanceMultiplier = 1.5f;

    [Tooltip("Ball speed threshold to trigger interrupt")]
    [SerializeField] private float interruptBallSpeedThreshold = 8f;

    [Tooltip("Enable re-evaluating best figure during charge")]
    [SerializeField] private bool chargeAdaptationEnabled = false;

    [Tooltip("Enable priority override system")]
    [SerializeField] private bool priorityOverrideEnabled = false;

    [Tooltip("Maximum magnet duration before timeout (seconds)")]
    [SerializeField] private float maxMagnetDuration = 999f;

    [Header("Behavior Quality (Difficulty-Scaled)")]
    [Tooltip("Chance of whiffing a shot (0=never, 1=always)")]
    [Range(0f, 1f)]
    [SerializeField] private float whiffChance = 0.05f;

    [Tooltip("Positioning accuracy (1=perfect, lower=overshoot)")]
    [Range(0.3f, 1f)]
    [SerializeField] private float positioningAccuracy = 0.85f;

    [Tooltip("Rod movement speed multiplier (1=full speed)")]
    [Range(0.3f, 1.5f)]
    [SerializeField] private float movementSpeedMultiplier = 0.85f;

    [Header("Shot Aggressiveness (Difficulty-Scaled)")]
    [Tooltip("Target charge fraction of lightShotThreshold for non-GK shots (lower=faster shots)")]
    [Range(0.2f, 1.0f)]
    [SerializeField] private float shotChargeTarget = 0.6f;

    [Tooltip("Cooldown after max charge restarts before allowing shots again (seconds)")]
    [Range(0.3f, 3.0f)]
    [SerializeField] private float chargeCooldownDuration = 1.5f;

    [Header("Magnet Behavior (Difficulty-Scaled)")]
    [Tooltip("Minimum time magnet stays active once turned on (seconds)")]
    [Range(0.1f, 2.0f)]
    [SerializeField] private float minimumMagnetHoldTime = 0.3f;

    [Tooltip("Maximum ball velocity to activate magnet")]
    [Range(2.0f, 10.0f)]
    [SerializeField] private float maxBallVelocityForMagnet = 6.0f;

    [Header("Difficulty Presets (Read-Only)")]
    [Tooltip("Click buttons in Inspector to load preset difficulty configurations")]
    [SerializeField] private bool showPresetButtons = true;

    // Ball reference
    private GameObject ball;

    // Cached rod x positions (fixed, won't change during gameplay)
    private float[] rodXPositions = new float[4];

    // Cached rod configurations for reach calculations
    private global::RodConfiguration[] rodConfigurations = new global::RodConfiguration[4];
    private float[] figureColliderRadii = new float[4];

    // Cached references to FSM components
    private AIRodStateMachine[] rodStateMachines = new AIRodStateMachine[4];

    // Field dimensions for zone calculation
    private float fieldMinX;
    private float fieldMaxX;
    private float fieldWidth;

    // Current rod configuration
    private RodConfiguration currentRodConfig = RodConfiguration.Defensive;

    /// <summary>
    /// Two-hand rod configurations (simulates human player using 2 hands)
    /// </summary>
    public enum RodConfiguration
    {
        Defensive,      // GK + Defense (ball in defensive zone)
        Balanced,       // Defense + Midfield (ball in center)
        Attacking,      // Midfield + Attack (ball in attacking zone)
        GoalkeeperOnly  // GK only (GK has possession, defense would self-block)
    }

    /// <summary>
    /// Ball possession state
    /// </summary>
    public enum BallPossession
    {
        AI,         // AI team has ball control
        Opponent,   // Opponent has ball control
        Free        // Ball is free (moving fast or not near any figure)
    }

    /// <summary>
    /// Current ball possession (updated every FixedUpdate)
    /// </summary>
    public BallPossession CurrentPossession { get; private set; } = BallPossession.Free;

    [Header("Ball Possession Settings")]
    [Tooltip("Max ball speed to consider it 'controlled' by a figure")]
    [SerializeField] private float possessionMaxBallSpeed = 2.5f;

    [Tooltip("Max distance from figure to consider ball 'possessed'")]
    [SerializeField] private float possessionDistance = 2.5f;

    private void Awake()
    {
        // Cache FSM references
        CacheFSMReferences();

        // Cache rod configurations
        CacheRodConfigurations();

        // Calculate field dimensions
        CalculateFieldDimensions();
    }

    private void Start()
    {
        // Get the active ball with Rigidbody2D component
        FindBall();

        // Cache rod x positions (they don't move during gameplay)
        CacheRodPositions();

        // Configure rods for AI control with FSM
        ConfigureRodsForAI();

        // Load difficulty from MatchInfo
        LoadDifficultyFromMatchInfo();

        // Apply difficulty settings to all child rods
        ApplyDifficultySettingsToRods();
    }

    private void FixedUpdate()
    {
        // If there's a ball in the field, activate appropriate rods
        if (ball != null)
        {
            // Update ball possession state
            UpdateBallPossession();

            // Update rods based on intelligent two-hand selection
            UpdateRodsWithTwoHandControl();
        }
        else
        {
            // If ball reference is lost, try to find it again
            FindBall();
        }
    }

    #region Initialization

    /// <summary>
    /// Finds and caches the ball reference
    /// </summary>
    private void FindBall()
    {
        GameObject[] allBalls = GameObject.FindGameObjectsWithTag("Ball");
        foreach (GameObject ballObj in allBalls)
        {
            if (ballObj.GetComponent<Rigidbody2D>() != null)
            {
                ball = ballObj;
                break;
            }
        }
    }

    #region Ball Possession Detection

    /// <summary>
    /// Updates ball possession state by checking proximity to AI and opponent figures.
    /// Ball is "possessed" if it's slow and near a figure.
    /// </summary>
    private void UpdateBallPossession()
    {
        if (ball == null) { CurrentPossession = BallPossession.Free; return; }

        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        if (ballRb == null) { CurrentPossession = BallPossession.Free; return; }

        // Fast-moving ball = nobody has it
        if (ballRb.linearVelocity.magnitude > possessionMaxBallSpeed)
        {
            CurrentPossession = BallPossession.Free;
            return;
        }

        Vector2 ballPos = ball.transform.position;
        float closestAIDist = float.MaxValue;
        float closestOpponentDist = float.MaxValue;

        // Check AI figures
        for (int i = 0; i < rods.Length; i++)
        {
            if (rods[i] == null) continue;
            for (int j = 0; j < rods[i].transform.childCount; j++)
            {
                float dist = Vector2.Distance(ballPos, rods[i].transform.GetChild(j).position);
                if (dist < closestAIDist) closestAIDist = dist;
            }
        }

        // Check opponent figures
        TeamRodsController opponentTeam = FindOpponentTeamController();
        if (opponentTeam != null)
        {
            for (int i = 0; i < opponentTeam.lines.Length; i++)
            {
                if (opponentTeam.lines[i] == null) continue;
                for (int j = 0; j < opponentTeam.lines[i].transform.childCount; j++)
                {
                    float dist = Vector2.Distance(ballPos, opponentTeam.lines[i].transform.GetChild(j).position);
                    if (dist < closestOpponentDist) closestOpponentDist = dist;
                }
            }
        }

        // Determine possession
        BallPossession previousPossession = CurrentPossession;

        if (closestAIDist < possessionDistance && closestAIDist <= closestOpponentDist)
            CurrentPossession = BallPossession.AI;
        else if (closestOpponentDist < possessionDistance)
            CurrentPossession = BallPossession.Opponent;
        else
            CurrentPossession = BallPossession.Free;

        // Log possession changes
        if (CurrentPossession != previousPossession)
        {
            AIDebugLogger.LogPossession(previousPossession.ToString(), CurrentPossession.ToString());
        }
    }

    /// <summary>
    /// Finds the opponent's TeamRodsController
    /// </summary>
    private TeamRodsController _cachedOpponentTeam;
    private TeamRodsController FindOpponentTeamController()
    {
        if (_cachedOpponentTeam != null) return _cachedOpponentTeam;

        TeamRodsController[] allTeams = FindObjectsOfType<TeamRodsController>();
        foreach (var team in allTeams)
        {
            if (team.teamSide != teamSide)
            {
                _cachedOpponentTeam = team;
                return team;
            }
        }
        return null;
    }

    #endregion
    /// </summary>
    private void CacheRodPositions()
    {
        for (int i = 0; i < rods.Length; i++)
        {
            if (rods[i] != null)
            {
                rodXPositions[i] = rods[i].transform.position.x;
            }
        }
    }

    /// <summary>
    /// Caches rod configurations and figure collider radii for reach calculations
    /// These values are set in RodConfiguration based on number of figures per rod
    /// </summary>
    private void CacheRodConfigurations()
    {
        for (int i = 0; i < rods.Length; i++)
        {
            if (rods[i] != null)
            {
                rodConfigurations[i] = rods[i].GetComponent<global::RodConfiguration>();

                // Get figure collider radius from first child's CircleCollider2D
                if (rodConfigurations[i] != null && rods[i].transform.childCount > 0)
                {
                    CircleCollider2D figureCollider = rods[i].transform.GetChild(0).GetComponentInChildren<CircleCollider2D>();
                    if (figureCollider != null)
                    {
                        figureColliderRadii[i] = figureCollider.radius;
                    }
                    else
                    {
                        // Default fallback based on rod configuration (from RodConfiguration.cs SetupRodFigures)
                        int figureCount = rodConfigurations[i].rodFoosballFigureCount;
                        figureColliderRadii[i] = GetDefaultColliderRadius(figureCount);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Returns default collider radius based on number of figures in rod
    /// Values from RodConfiguration.cs SetupRodFigures method
    /// </summary>
    private float GetDefaultColliderRadius(int figureCount)
    {
        switch (figureCount)
        {
            case 1: return 2.5f;
            case 2: return 2.25f;
            case 3: return 2f;
            case 4: return 1.75f;
            case 5: return 1.5f;
            default: return 2f;
        }
    }

    /// <summary>
    /// Caches FSM component references for efficient access
    /// </summary>
    private void CacheFSMReferences()
    {
        for (int i = 0; i < rods.Length; i++)
        {
            if (rods[i] != null)
            {
                rodStateMachines[i] = rods[i].GetComponent<AIRodStateMachine>();
                if (rodStateMachines[i] == null)
                {
                    Debug.LogWarning($"[AITeamRodsController] Rod '{rods[i].name}' is missing AIRodStateMachine component! FSM will not work for this rod.");
                }
            }
        }
    }

    /// <summary>
    /// Calculates field dimensions for zone-based decisions
    /// </summary>
    private void CalculateFieldDimensions()
    {
        // Find field boundaries based on rods
        if (rods[0] != null && rods[3] != null)
        {
            // Use goalkeeper and attacker positions as field boundaries
            fieldMinX = Mathf.Min(rods[0].transform.position.x, rods[3].transform.position.x);
            fieldMaxX = Mathf.Max(rods[0].transform.position.x, rods[3].transform.position.x);
            fieldWidth = fieldMaxX - fieldMinX;
        }
        else
        {
            // Fallback estimates
            fieldMinX = -15f;
            fieldMaxX = 15f;
            fieldWidth = 30f;
        }
    }

    /// <summary>
    /// Loads difficulty level from MatchInfo and applies preset
    /// </summary>
    private void LoadDifficultyFromMatchInfo()
    {
        // Get difficulty level from MatchInfo
        if (MatchInfo.instance != null)
        {
            aiDifficultyLevel = Mathf.Clamp(MatchInfo.instance.matchLevel, 1, 3);
            Debug.Log($"[AITeamRodsController] Loaded difficulty level {aiDifficultyLevel} from MatchInfo");
        }
        else
        {
            // No match info available (e.g. testing in editor) — default to Medium
            aiDifficultyLevel = 2;
            Debug.LogWarning($"[AITeamRodsController] MatchInfo not found, defaulting to Medium (level {aiDifficultyLevel})");
        }

        // ALWAYS apply preset based on difficulty level
        // This ensures code values override any old serialized Inspector values
        ApplyDifficultyPreset(aiDifficultyLevel);
    }

    /// <summary>
    /// Applies difficulty preset configuration
    /// Called automatically when loading from MatchInfo or manually from Inspector buttons
    /// </summary>
    public void ApplyDifficultyPreset(int level)
    {
        aiDifficultyLevel = Mathf.Clamp(level, 1, 3);

        switch (aiDifficultyLevel)
        {
            case 1: // Easy
                ApplyEasyPreset();
                break;
            case 2: // Medium
                ApplyMediumPreset();
                break;
            case 3: // Hard
                ApplyHardPreset();
                break;
        }

        Debug.Log($"[AITeamRodsController] Applied difficulty preset: Level {aiDifficultyLevel}");

        // Apply to rods if already initialized
        if (Application.isPlaying && rods[0] != null)
        {
            ApplyDifficultySettingsToRods();
        }
    }

    /// <summary>
    /// EASY PRESET - Forgiving AI for beginners
    /// </summary>
    private void ApplyEasyPreset()
    {
        // Action Probabilities - Lower chances
        shootProbability = 0.4f;
        wallPassProbability = 0.3f;
        magnetProbability = 0.3f;

        // Reaction & Timing - Slower
        reactionDelay = 0.3f;
        chargeTimeMultiplier = 1.0f;
        decisionInterval = 0.3f;

        // Goal Evaluation - Less aggressive
        minimumShootScore = 0.5f; // Higher threshold = pickier about shots
        minimumPassAdvantage = 0.3f;

        // Physical Parameters - Weaker
        attractionForce = -5f;
        wallPassForce = 8f;
        shootableDistanceThreshold = 2.5f;
        possessionDistance = 2.5f;

        // Defensive Strategy - Predictable
        zoneDefenseWeight = 0.7f;
        manMarkingWeight = 0.2f;
        anticipationWeight = 0.1f;

        // Interrupt System - Disabled (keeps "human-like" mistakes)
        interruptEnabled = false;
        chargeAdaptationEnabled = false;
        priorityOverrideEnabled = false;
        maxMagnetDuration = 999f;

        // Behavior Quality - Sloppy (visible mistakes)
        whiffChance = 0.20f;
        positioningAccuracy = 0.60f;
        movementSpeedMultiplier = 0.70f;

        // Shot Aggressiveness - Slow and telegraphed
        shotChargeTarget = 0.8f; // Charges to 0.8s (slow, easy to read)
        chargeCooldownDuration = 2.0f; // Long cooldown between shot attempts

        // Magnet Behavior - Brief hold, low velocity capture
        minimumMagnetHoldTime = 0.2f;
        maxBallVelocityForMagnet = 4.0f;
    }

    /// <summary>
    /// MEDIUM PRESET - Balanced AI for average players
    /// </summary>
    private void ApplyMediumPreset()
    {
        // Action Probabilities - Balanced
        shootProbability = 0.6f;
        wallPassProbability = 0.5f;
        magnetProbability = 0.5f;

        // Reaction & Timing - Average
        reactionDelay = 0.2f;
        chargeTimeMultiplier = 0.75f;
        decisionInterval = 0.3f;

        // Goal Evaluation - Moderate
        minimumShootScore = 0.3f;
        minimumPassAdvantage = 0.2f;

        // Physical Parameters - Standard
        attractionForce = -10f;
        wallPassForce = 10f;
        shootableDistanceThreshold = 2.0f;
        possessionDistance = 2.5f;

        // Defensive Strategy - Mixed
        zoneDefenseWeight = 0.4f;
        manMarkingWeight = 0.4f;
        anticipationWeight = 0.2f;

        // Interrupt System - Basic (cancel if ball leaves range)
        interruptEnabled = true;
        interruptDistanceMultiplier = 2.0f;
        interruptBallSpeedThreshold = 10f;
        chargeAdaptationEnabled = false;
        priorityOverrideEnabled = true;
        maxMagnetDuration = 1.5f;

        // Behavior Quality - Decent (occasional minor mistakes)
        whiffChance = 0.05f;
        positioningAccuracy = 0.85f;
        movementSpeedMultiplier = 0.85f;

        // Shot Aggressiveness - Moderate speed
        shotChargeTarget = 0.6f; // Charges to 0.6s (balanced)
        chargeCooldownDuration = 1.2f; // Moderate cooldown

        // Magnet Behavior - Decent hold
        minimumMagnetHoldTime = 0.5f;
        maxBallVelocityForMagnet = 5.0f;
    }

    /// <summary>
    /// HARD PRESET - Challenging AI for skilled players
    /// </summary>
    private void ApplyHardPreset()
    {
        // Action Probabilities - Aggressive
        shootProbability = 0.8f;
        wallPassProbability = 0.7f;
        magnetProbability = 0.7f;

        // Reaction & Timing - Fast
        reactionDelay = 0.1f;
        chargeTimeMultiplier = 1.0f;
        decisionInterval = 0.2f;

        // Goal Evaluation - Opportunistic (shoots at almost anything)
        minimumShootScore = 0.15f;
        minimumPassAdvantage = 0.1f;

        // Physical Parameters - Strong
        attractionForce = -15f;
        wallPassForce = 12f;
        shootableDistanceThreshold = 2.0f;
        possessionDistance = 2.5f;

        // Defensive Strategy - Unpredictable
        zoneDefenseWeight = 0.3f;
        manMarkingWeight = 0.3f;
        anticipationWeight = 0.4f;

        // Interrupt System - Full (strict thresholds + charge adaptation)
        interruptEnabled = true;
        interruptDistanceMultiplier = 1.3f;
        interruptBallSpeedThreshold = 6f;
        chargeAdaptationEnabled = true;
        priorityOverrideEnabled = true;
        maxMagnetDuration = 0.8f;

        // Behavior Quality - Precise (no mistakes)
        whiffChance = 0.0f;
        positioningAccuracy = 1.0f;
        movementSpeedMultiplier = 1.0f;

        // Shot Aggressiveness - Quick taps like a skilled player
        shotChargeTarget = 0.4f; // Charges to 0.4s (fast, aggressive)
        chargeCooldownDuration = 0.5f; // Short cooldown, rapid-fire capable

        // Magnet Behavior - Long holds, captures fast balls
        minimumMagnetHoldTime = 0.8f;
        maxBallVelocityForMagnet = 7.0f;
    }

    /// <summary>
    /// Pushes current difficulty settings to all child FSM components
    /// Called after changing settings or loading presets
    /// </summary>
    private void ApplyDifficultySettingsToRods()
    {
        for (int i = 0; i < rods.Length; i++)
        {
            if (rods[i] != null && rodStateMachines[i] != null)
            {
                AIRodStateMachine fsm = rodStateMachines[i];

                // Apply action probabilities
                fsm.SetShootProbability(shootProbability);
                fsm.SetWallPassProbability(wallPassProbability);
                fsm.SetMagnetProbability(magnetProbability);

                // Apply timing parameters
                fsm.SetReactionDelay(reactionDelay);
                fsm.SetChargeTimeMultiplier(chargeTimeMultiplier);
                fsm.SetDecisionInterval(decisionInterval);

                // Apply goal evaluation thresholds
                fsm.SetMinimumShootScore(minimumShootScore);
                fsm.SetMinimumPassAdvantage(minimumPassAdvantage);

                // Apply physical parameters
                fsm.SetAttractionForce(attractionForce);
                fsm.SetWallPassForce(wallPassForce);

                // Apply shootable distance threshold directly to action components
                AIRodShootAction shootAction = rods[i].GetComponent<AIRodShootAction>();
                if (shootAction != null)
                {
                    shootAction.SetShootableDistanceThreshold(shootableDistanceThreshold);
                    shootAction.SetBallBehindTolerance(0.5f);
                }
                AIRodMagnetAction magnetAction = rods[i].GetComponent<AIRodMagnetAction>();
                if (magnetAction != null)
                {
                    magnetAction.SetShootableDistanceThreshold(shootableDistanceThreshold);
                }

                // Apply interrupt & adaptation settings
                fsm.SetInterruptEnabled(interruptEnabled);
                fsm.SetInterruptDistanceMultiplier(interruptDistanceMultiplier);
                fsm.SetInterruptBallSpeedThreshold(interruptBallSpeedThreshold);
                fsm.SetChargeAdaptationEnabled(chargeAdaptationEnabled);
                fsm.SetPriorityOverrideEnabled(priorityOverrideEnabled);
                fsm.SetMaxMagnetDuration(maxMagnetDuration);

                // Apply behavior quality settings (difficulty-scaled)
                if (shootAction != null)
                {
                    shootAction.SetWhiffChance(whiffChance);
                    shootAction.SetShotChargeTarget(shotChargeTarget);
                    shootAction.SetChargeCooldownDuration(chargeCooldownDuration);
                }
                if (magnetAction != null)
                {
                    magnetAction.SetMinimumMagnetHoldTime(minimumMagnetHoldTime);
                    magnetAction.SetMaxBallVelocityForMagnet(maxBallVelocityForMagnet);
                }
                AIRodMovementAction movementAction = rods[i].GetComponent<AIRodMovementAction>();
                if (movementAction != null)
                {
                    movementAction.SetPositioningAccuracy(positioningAccuracy);
                    movementAction.SetMovementSpeedMultiplier(movementSpeedMultiplier);
                }
            }
        }

        Debug.Log($"[AITeamRodsController] Applied difficulty settings to {rods.Length} rods");
    }

    /// <summary>
    /// CENTRALIZED DIFFICULTY SYSTEM
    /// 
    /// All AI difficulty settings are managed here and pushed to child rods.
    /// This allows easy testing and tuning from a single Inspector location.
    /// 
    /// DIFFICULTY LEVELS:
    /// - Easy (1): Low probabilities, slow reactions, weak shots
    /// - Medium (2): Balanced values
    /// - Hard (3): High probabilities, fast reactions, strong shots
    /// 
    /// WORKFLOW:
    /// 1. Adjust values in Inspector (or load preset)
    /// 2. Settings automatically apply to all child rods
    /// 3. Test and iterate quickly
    /// </summary>

    #endregion

    #region Two-Hand Intelligent Rod Control

    /// <summary>
    /// INTELLIGENT TWO-HAND ROD CONTROL
    /// 
    /// Simulates realistic foosball gameplay where player controls 2 rods:
    /// 
    /// DEFENSIVE CONFIGURATION (Goalkeeper + Defense):
    /// - Activated when ball is in defensive third of field
    /// - Both rods focus on blocking shots and covering goal
    /// - Goalkeeper uses DefensiveBlocking, Defense uses DefensiveCovering
    /// 
    /// BALANCED CONFIGURATION (Defense + Midfield):
    /// - Activated when ball is in middle third of field
    /// - Defense covers back, Midfield prepares for transition
    /// - Flexible positioning for counter-attacks
    /// 
    /// ATTACKING CONFIGURATION (Midfield + Attacker):
    /// - Activated when ball is in attacking third
    /// - Both rods coordinate to create scoring opportunities
    /// - Midfield supports, Attacker focuses on shooting
    /// 
    /// BENEFITS:
    /// - More realistic foosball simulation
    /// - Better defensive coverage with coordinated rods
    /// - Smoother transitions between defense and attack
    /// - AI feels more "human" with realistic rod switching
    /// </summary>
    private void UpdateRodsWithTwoHandControl()
    {
        if (ball == null) return;

        // Determine which zone ball is in
        FieldZone ballZone = DetermineBallZone();

        // Determine optimal rod configuration for this zone
        RodConfiguration optimalConfig = DetermineOptimalConfiguration(ballZone);

        // Update configuration if changed
        if (optimalConfig != currentRodConfig)
        {
            currentRodConfig = optimalConfig;

            AIDebugLogger.LogRodConfig(currentRodConfig.ToString(), $"Ball in {ballZone}, Possession: {CurrentPossession}");

            if (rodStateMachines[0] != null && rodStateMachines[0].ShowDebugInfo)
            {
                Debug.Log($"[AITeamRodsController] Rod configuration changed to: {currentRodConfig} (Ball in {ballZone}, Possession: {CurrentPossession})");
            }
        }

        // Activate appropriate 2-rod combination
        bool[] activeRods = GetActiveRodsForConfiguration(currentRodConfig);

        // Apply reach-based refinement (only activate rods that can reach ball)
        bool[] reachableRods = CalculateReachableRods();

        // Combine configuration preference with reachability
        bool[] finalRods = CombineConfigurationWithReachability(activeRods, reachableRods);

        // Activate rods and set appropriate FSM states
        ActivateRodsWithStates(finalRods, ballZone);
    }

    /// <summary>
    /// Determines which field zone the ball is currently in
    /// </summary>
    private FieldZone DetermineBallZone()
    {
        float ballX = ball.transform.position.x;

        // Normalize ball position to 0-1 range across field
        float normalizedPosition = (ballX - fieldMinX) / fieldWidth;

        // Adjust for team side (flip if AI is on right side)
        if (teamSide == TeamSide.RightTeam)
        {
            normalizedPosition = 1.0f - normalizedPosition;
        }

        // Determine zone based on normalized position
        if (normalizedPosition < defensiveZoneThreshold)
        {
            return FieldZone.OwnDefensive;
        }
        else if (normalizedPosition > (1.0f - attackingZoneThreshold))
        {
            return FieldZone.OpponentDefensive;
        }
        else
        {
            return FieldZone.Midfield;
        }
    }

    /// <summary>
    /// Determines optimal 2-rod configuration based on ball possession and position.
    /// 
    /// DEFENDING (opponent has ball): activate rods BEHIND the ball (between ball and AI goal)
    /// ATTACKING (AI has ball): activate rod WITH ball + next rod FORWARD
    /// FREE BALL: activate 2 rods closest to ball, biased toward defensive side
    /// </summary>
    private RodConfiguration DetermineOptimalConfiguration(FieldZone zone)
    {
        float ballX = ball.transform.position.x;

        // Find which rod index the ball is closest to
        int closestRodIndex = GetClosestRodIndex(ballX);

        switch (CurrentPossession)
        {
            case BallPossession.Opponent:
            case BallPossession.Free:
                // DEFENDING: activate rods BEHIND the ball (between ball and own goal)
                return DetermineDefensiveConfiguration(ballX, closestRodIndex);

            case BallPossession.AI:
                // ATTACKING: rod with ball + next rod forward
                return DetermineAttackingConfiguration(ballX, closestRodIndex);

            default:
                return RodConfiguration.Balanced;
        }
    }

    /// <summary>
    /// When defending: pick 2 rods behind the ball (between ball and AI's own goal).
    /// If ball is very close to GK, only GK should be active to avoid self-blocking.
    /// </summary>
    private RodConfiguration DetermineDefensiveConfiguration(float ballX, int closestRodIndex)
    {
        // Determine which rods are BEHIND the ball (between ball and AI goal)
        // AI goal direction: LeftTeam's goal is on left, RightTeam's goal is on right
        bool aiGoalOnLeft = (teamSide == TeamSide.LeftTeam);

        // Count rods behind the ball
        int rodsBehind = 0;
        int firstBehind = -1;
        int secondBehind = -1;

        for (int i = 0; i < rods.Length; i++)
        {
            if (rods[i] == null) continue;
            bool isBehindBall;
            if (aiGoalOnLeft)
                isBehindBall = rodXPositions[i] < ballX;
            else
                isBehindBall = rodXPositions[i] > ballX;

            if (isBehindBall)
            {
                rodsBehind++;
                if (firstBehind == -1) firstBehind = i;
                else if (secondBehind == -1) secondBehind = i;
            }
        }

        // Map to closest matching configuration
        if (rodsBehind == 0)
        {
            // Ball is behind all rods (past GK) — GK only
            return RodConfiguration.Defensive;
        }
        else if (rodsBehind == 1 && firstBehind == 0)
        {
            // Only GK behind ball
            return RodConfiguration.Defensive;
        }
        else if (firstBehind <= 1 && (secondBehind == -1 || secondBehind <= 1))
        {
            // GK and/or Defense behind ball
            return RodConfiguration.Defensive;
        }
        else if (firstBehind <= 2)
        {
            return RodConfiguration.Balanced;
        }
        else
        {
            return RodConfiguration.Attacking;
        }
    }

    /// <summary>
    /// When attacking: activate rod WITH ball + next rod FORWARD toward opponent goal.
    /// Forward rod will need to clear the lane to avoid self-blocking (handled by movement).
    /// </summary>
    private RodConfiguration DetermineAttackingConfiguration(float ballX, int closestRodIndex)
    {
        // When GK has the ball, use GK-only mode to prevent defense from self-blocking
        if (closestRodIndex == 0)
        {
            return RodConfiguration.GoalkeeperOnly;
        }

        // Rod with ball + next forward rod
        // Forward = toward opponent goal
        // LeftTeam attacks right (higher X = forward), RightTeam attacks left (lower X = forward)
        bool forwardIsHigherIndex = (teamSide == TeamSide.LeftTeam);

        int nextForwardRod;
        if (forwardIsHigherIndex)
            nextForwardRod = Mathf.Min(closestRodIndex + 1, 3);
        else
            nextForwardRod = Mathf.Max(closestRodIndex - 1, 0);

        // If the forward rod is the same as closest (at boundary), use closest + backup
        if (nextForwardRod == closestRodIndex)
        {
            // At frontmost rod, use backup rod behind
            if (forwardIsHigherIndex)
                nextForwardRod = Mathf.Max(closestRodIndex - 1, 0);
            else
                nextForwardRod = Mathf.Min(closestRodIndex + 1, 3);
        }

        // Map to configuration based on which rods are active
        int lower = Mathf.Min(closestRodIndex, nextForwardRod);
        int higher = Mathf.Max(closestRodIndex, nextForwardRod);

        if (lower == 0 && higher == 1) return RodConfiguration.Defensive;
        if (lower == 1 && higher == 2) return RodConfiguration.Balanced;
        if (lower == 2 && higher == 3) return RodConfiguration.Attacking;

        // Non-standard pair — pick closest standard config
        if (higher <= 1) return RodConfiguration.Defensive;
        if (lower >= 2) return RodConfiguration.Attacking;
        return RodConfiguration.Balanced;
    }

    /// <summary>
    /// Gets the rod index closest to the given X position
    /// </summary>
    private int GetClosestRodIndex(float ballX)
    {
        int closest = 0;
        float closestDist = float.MaxValue;
        for (int i = 0; i < rods.Length; i++)
        {
            if (rods[i] == null) continue;
            float dist = Mathf.Abs(ballX - rodXPositions[i]);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = i;
            }
        }
        return closest;
    }

    /// <summary>
    /// Returns which 2 rods should be active for given configuration
    /// </summary>
    private bool[] GetActiveRodsForConfiguration(RodConfiguration config)
    {
        // [0]=GK, [1]=Defense, [2]=Midfield, [3]=Attack
        bool[] activeRods = new bool[4];

        switch (config)
        {
            case RodConfiguration.GoalkeeperOnly:
                activeRods[0] = true; // Goalkeeper only — defense cleared to avoid self-blocking
                break;

            case RodConfiguration.Defensive:
                activeRods[0] = true; // Goalkeeper
                activeRods[1] = true; // Defense
                break;

            case RodConfiguration.Balanced:
                activeRods[1] = true; // Defense
                activeRods[2] = true; // Midfield
                break;

            case RodConfiguration.Attacking:
                activeRods[2] = true; // Midfield
                activeRods[3] = true; // Attack
                break;
        }

        return activeRods;
    }

    /// <summary>
    /// Calculates which rods can physically reach the ball
    /// </summary>
    private bool[] CalculateReachableRods()
    {
        bool[] reachable = new bool[4];

        for (int i = 0; i < rods.Length; i++)
        {
            if (rods[i] != null && rodConfigurations[i] != null)
            {
                reachable[i] = CheckIfRodCanReachBall(i);
            }
        }

        return reachable;
    }

    /// <summary>
    /// Combines configuration preference with reachability constraints
    /// Ensures at least 2 rods are active (can activate non-preferred rod if needed)
    /// Exception: GoalkeeperOnly allows single-rod activation to prevent self-blocking
    /// </summary>
    private bool[] CombineConfigurationWithReachability(bool[] preferred, bool[] reachable)
    {
        bool[] final = new bool[4];
        int activeCount = 0;

        // GoalkeeperOnly: only GK active, skip minimum-2 enforcement
        bool isGKOnly = (currentRodConfig == RodConfiguration.GoalkeeperOnly);

        // First pass: Activate preferred rods that can reach ball
        for (int i = 0; i < 4; i++)
        {
            if (preferred[i] && reachable[i])
            {
                final[i] = true;
                activeCount++;
            }
        }

        // Second pass: If we have less than 2 active rods, activate closest reachable rods
        // Skip this when GoalkeeperOnly — single rod is intentional to prevent self-blocking
        if (activeCount < 2 && !isGKOnly)
        {
            // Calculate distances for reachable rods not yet active
            float[] distances = new float[4];
            for (int i = 0; i < 4; i++)
            {
                if (!final[i] && reachable[i])
                {
                    distances[i] = Mathf.Abs(ball.transform.position.x - rodXPositions[i]);
                }
                else
                {
                    distances[i] = float.MaxValue;
                }
            }

            // Activate closest rods until we have 2 active
            while (activeCount < 2)
            {
                int closestIndex = -1;
                float closestDistance = float.MaxValue;

                for (int i = 0; i < 4; i++)
                {
                    if (distances[i] < closestDistance)
                    {
                        closestDistance = distances[i];
                        closestIndex = i;
                    }
                }

                if (closestIndex >= 0 && closestDistance < float.MaxValue)
                {
                    final[closestIndex] = true;
                    distances[closestIndex] = float.MaxValue;
                    activeCount++;
                }
                else
                {
                    break; // No more reachable rods
                }
            }
        }

        // Final safety: If still no rods active, activate closest 2 regardless of reach
        if (activeCount == 0)
        {
            float[] distances = new float[4];
            for (int i = 0; i < 4; i++)
            {
                if (rods[i] != null)
                {
                    distances[i] = Mathf.Abs(ball.transform.position.x - rodXPositions[i]);
                }
                else
                {
                    distances[i] = float.MaxValue;
                }
            }

            // Activate 2 closest rods
            for (int j = 0; j < 2; j++)
            {
                int closestIndex = -1;
                float closestDistance = float.MaxValue;

                for (int i = 0; i < 4; i++)
                {
                    if (distances[i] < closestDistance)
                    {
                        closestDistance = distances[i];
                        closestIndex = i;
                    }
                }

                if (closestIndex >= 0)
                {
                    final[closestIndex] = true;
                    distances[closestIndex] = float.MaxValue;
                }
            }
        }

        return final;
    }

    /// <summary>
    /// Activates rods and sets appropriate FSM states based on game situation
    /// 
    /// UPDATED: Uses new PositioningState (ball-centric, role-agnostic)
    /// No more separate Defending/Tracking states - PositioningState handles everything
    /// </summary>
    private void ActivateRodsWithStates(bool[] activeRods, FieldZone ballZone)
    {
        for (int i = 0; i < rods.Length; i++)
        {
            if (rods[i] != null)
            {
                // Update movement action (used by FSM for rod positioning)
                AIRodMovementAction rodMovement = rods[i].GetComponent<AIRodMovementAction>();
                if (rodMovement != null)
                {
                    // GK centering when deactivated (Medium/Hard only)
                    if (!activeRods[i] && i == 0 && aiDifficultyLevel >= 2 && rodMovement.isActive)
                    {
                        // GK was active and is now being deactivated — center it first
                        rodMovement.StartCentering();
                    }

                    rodMovement.isActive = activeRods[i];
                }

                // All active rods use PositioningState (unified ball-centric behavior)
                if (activeRods[i] && rodStateMachines[i] != null)
                {
                    // Check if rod is idle and needs to activate
                    if (rodStateMachines[i].IsInState<IdleState>())
                    {
                        // Transition from Idle to Positioning (FSM handles the rest)
                        rodStateMachines[i].ChangeState<PositioningState>();
                    }
                    // If rod is in action state (Charging, Shooting, etc), let it finish
                    // PositioningState will handle movement mode selection based on ball context
                }

                // Update figure animations
                for (int j = 0; j < rods[i].transform.childCount; j++)
                {
                    Animator animator = rods[i].transform.GetChild(j).GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.SetBool("IsLineActive", activeRods[i]);
                    }
                }
            }
        }

        // Update visual indicators
        UpdateVisualIndicators(activeRods);
    }

    #region Visual Updates

    /// <summary>
    /// Updates UI sprites to show which rods are active
    /// </summary>
    private void UpdateVisualIndicators(bool[] conf)
    {
        for (int i = 0; i < rodsIndicators.Length; i++)
        {
            if (rodsIndicators[i] != null)
            {
                rodsIndicators[i].sprite = conf[i] ? activeRodUISprite : inactiveRodUISprite;
            }
        }
    }

    #endregion

    #region Public API / Debug Helpers

    /// <summary>
    /// Debug helper - returns current active rod indices
    /// </summary>
    public int[] GetActiveRodIndices()
    {
        System.Collections.Generic.List<int> activeIndices = new System.Collections.Generic.List<int>();

        for (int i = 0; i < rods.Length; i++)
        {
            if (rods[i] != null)
            {
                AIRodMovementAction rodMovement = rods[i].GetComponent<AIRodMovementAction>();
                if (rodMovement != null && rodMovement.isActive)
                {
                    activeIndices.Add(i);
                }
            }
        }

        return activeIndices.ToArray();
    }

    /// <summary>
    /// Debug helper - returns current FSM states of all rods
    /// </summary>
    public string[] GetCurrentFSMStates()
    {
        string[] states = new string[4];

        for (int i = 0; i < rods.Length; i++)
        {
            if (rodStateMachines[i] != null)
            {
                AIRodState currentState = rodStateMachines[i].GetCurrentState();
                states[i] = currentState != null ? currentState.GetStateName() : "None";
            }
            else
            {
                states[i] = "No FSM";
            }
        }

        return states;
    }

    /// <summary>
    /// Returns current rod configuration (for debugging)
    /// </summary>
    public RodConfiguration GetCurrentConfiguration()
    {
        return currentRodConfig;
    }

    /// <summary>
    /// Public API to get defensive strategy weights (used by DefendingState)
    /// </summary>
    public float[] GetDefensiveStrategyWeights()
    {
        float totalWeight = zoneDefenseWeight + manMarkingWeight + anticipationWeight;

        // Normalize weights to ensure they sum to 1.0
        if (totalWeight > 0)
        {
            return new float[]
            {
                zoneDefenseWeight / totalWeight,
                manMarkingWeight / totalWeight,
                anticipationWeight / totalWeight
            };
        }

        // Fallback to equal weights
        return new float[] { 0.33f, 0.33f, 0.34f };
    }

    /// <summary>
    /// Public API to get current difficulty level
    /// </summary>
    public int GetDifficultyLevel()
    {
        return aiDifficultyLevel;
    }

    #endregion

    #region Supporting Types

    /// <summary>
    /// Field zones for strategic decision making
    /// </summary>
    private enum FieldZone
    {
        OwnDefensive,      // Ball in own defensive third
        Midfield,       // Ball in middle third
        OpponentDefensive  // Ball in opponent's defensive third (AI attacking)
    }

    #endregion

#if UNITY_EDITOR
    /// <summary>
    /// Debug visualization in Scene view
    /// Shows reach zones, active rod connections, and current configuration
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || ball == null) return;

        // Draw field zone boundaries
        DrawFieldZones();

        // Draw reach zones for each rod
        for (int i = 0; i < rods.Length; i++)
        {
            if (rods[i] != null && rodConfigurations[i] != null)
            {
                AIRodMovementAction rodMovement = rods[i].GetComponent<AIRodMovementAction>();
                bool isActive = rodMovement != null && rodMovement.isActive;
                bool canReach = CheckIfRodCanReachBall(i);

                // Color coding: Green = active and can reach, Yellow = can reach but inactive, Red = cannot reach
                if (isActive && canReach)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(ball.transform.position, rods[i].transform.position);
                }
                else if (canReach)
                {
                    Gizmos.color = Color.yellow;
                }
                else
                {
                    Gizmos.color = Color.red;
                }

                // Draw vertical reach zone
                float movementLimit = rodConfigurations[i].rodMovementLimit;
                float colliderRadius = figureColliderRadii[i];
                float rodX = rodXPositions[i];
                float rodY = rods[i].transform.position.y;

                Vector3 topPoint = new Vector3(rodX, rodY + movementLimit + colliderRadius, 0);
                Vector3 bottomPoint = new Vector3(rodX, rodY - movementLimit - colliderRadius, 0);

                Gizmos.DrawLine(topPoint, bottomPoint);
                Gizmos.DrawWireSphere(topPoint, 0.1f);
                Gizmos.DrawWireSphere(bottomPoint, 0.1f);

                // Draw horizontal reach zone
                float horizontalReach = colliderRadius + reachBuffer;
                Gizmos.DrawWireSphere(new Vector3(rodX, rodY, 0), horizontalReach);
            }
        }

        // Draw ball position marker
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(ball.transform.position, 0.2f);

        // Draw current configuration label
        Vector3 labelPos = new Vector3(0, 8, 0);
        string configLabel = $"Config: {currentRodConfig}";
        UnityEditor.Handles.Label(labelPos, configLabel);
    }

    /// <summary>
    /// Draws field zone boundaries for debugging
    /// </summary>
    private void DrawFieldZones()
    {
        Gizmos.color = Color.cyan;

        // Calculate zone boundaries
        float defensiveZoneBoundary = fieldMinX + (fieldWidth * defensiveZoneThreshold);
        float attackingZoneBoundary = fieldMinX + (fieldWidth * (1.0f - attackingZoneThreshold));

        // Adjust for team side
        if (teamSide == TeamSide.RightTeam)
        {
            float temp = defensiveZoneBoundary;
            defensiveZoneBoundary = attackingZoneBoundary;
            attackingZoneBoundary = temp;
        }

        // Draw zone boundary lines
        Gizmos.DrawLine(new Vector3(defensiveZoneBoundary, -10, 0), new Vector3(defensiveZoneBoundary, 10, 0));
        Gizmos.DrawLine(new Vector3(attackingZoneBoundary, -10, 0), new Vector3(attackingZoneBoundary, 10, 0));
    }
#endif

    /// <summary>
    /// Configures rods for AI control using FSM system
    /// Called at startup to ensure proper configuration
    /// </summary>
    private void ConfigureRodsForAI()
    {
        for (int i = 0; i < rods.Length; i++)
        {
            if (rods[i] != null)
            {
                ConfigureRod(rods[i]);
            }
        }
    }

    /// <summary>
    /// Configures a single rod for AI control with FSM
    /// Ensures FSM is enabled and movement action is active
    /// </summary>
    private void ConfigureRod(GameObject rod)
    {
        if (rod == null) return;

        // Get AI components
        AIRodMovementAction aiMovement = rod.GetComponent<AIRodMovementAction>();
        AIRodStateMachine aiFSM = rod.GetComponent<AIRodStateMachine>();

        // Enable movement (required by FSM)
        if (aiMovement)
        {
            aiMovement.enabled = true;
        }
        else
        {
            Debug.LogError($"[AITeamRodsController] Rod '{rod.name}' is missing AIRodMovementAction component!");
        }

        // Enable and configure FSM
        if (aiFSM)
        {
            aiFSM.enabled = true;
            aiFSM.useFSM = true;
        }
        else
        {
            Debug.LogError($"[AITeamRodsController] Rod '{rod.name}' is missing AIRodStateMachine component! AI actions will not work.");
        }
    }

    #endregion

    #region Reach Calculation (Legacy/Fallback)

    /// <summary>
    /// Calculates if a specific rod can reach the ball
    /// 
    /// REACH CRITERIA:
    /// 1. Ball must be within horizontal reach range (X axis)
    /// 2. Ball must be within vertical reach range (Y axis + rod movement limits)
    /// 3. Magnet collider radius extends the effective reach
    /// </summary>
    private bool CheckIfRodCanReachBall(int rodIndex)
    {
        float ballX = ball.transform.position.x;
        float ballY = ball.transform.position.y;
        float rodX = rodXPositions[rodIndex];
        float rodY = rods[rodIndex].transform.position.y;

        // Get rod configuration values
        float movementLimit = rodConfigurations[rodIndex].rodMovementLimit;
        float colliderRadius = figureColliderRadii[rodIndex];

        // Calculate effective reach
        // Horizontal reach: collider radius + buffer
        float horizontalReach = colliderRadius + reachBuffer;

        // Vertical reach: movement limit determines how far rod can move in Y axis
        float verticalReach = movementLimit;

        // Check horizontal distance (considering team direction)
        float horizontalDistance = Mathf.Abs(ballX - rodX);
        bool withinHorizontalReach = horizontalDistance <= horizontalReach;

        // Check vertical distance
        // Rod can move within [-movementLimit, +movementLimit] from its center
        float minReachableY = rodY - verticalReach;
        float maxReachableY = rodY + verticalReach;

        // Add collider radius to vertical reach (figures can reach slightly beyond movement limit)
        minReachableY -= colliderRadius;
        maxReachableY += colliderRadius;

        bool withinVerticalReach = ballY >= minReachableY && ballY <= maxReachableY;

        // Ball is reachable if both horizontal and vertical conditions are met
        return withinHorizontalReach && withinVerticalReach;
    }

    #endregion
}
using UnityEngine;
using System.Collections;

/// <summary>
/// AI Rod Shoot Action - Manages shooting for AI-controlled rods
/// 
/// REFACTORED ARCHITECTURE: CONDITION-BASED EVALUATION + PARALLEL CHARGING
/// 
/// PHILOSOPHY:
/// Shooting is split into two phases:
/// 1. CHARGING: Can happen while rod is moving/positioning (PARALLEL ACTION)
/// 2. SHOOTING: Blocks movement during animation (STATE)
/// 
/// SHOOT CONDITION (No Probability, Pure Logic):
/// 1. Ball is in SHOOTABLE POSITION:
///    - Ball is CLOSE (distance < shootableDistanceThreshold)
///    - Ball is IN FRONT of figure (based on team attack direction)
/// 2. THEN evaluate shot quality with AIGoalEvaluator:
///    - Clear path to goal
///    - Shooting score above minimum threshold
/// 
/// KEY DIFFERENCES FROM OLD SYSTEM:
/// - ❌ NO probability checks for deciding to shoot
/// - ❌ NO state machine - PositioningState calls this
/// - ✅ Charging happens WHILE rod moves
/// - ✅ Condition-based: shoot if conditions met
/// - ✅ Shot quality determined by AIGoalEvaluator
/// 
/// PROCESS:
/// 1. PositioningState calls EvaluateShoot() periodically
/// 2. If conditions met → StartCharging()
/// 3. Charging builds up WHILE rod continues positioning
/// 4. When charged → ExecuteShoot() → FSM transitions to ShootingState
/// 5. ShootingState blocks movement during animation
/// </summary>
[RequireComponent(typeof(AIRodMovementAction))]
public class AIRodShootAction : MonoBehaviour
{
    #region Configuration

    [Header("Shootable Position Configuration")]
    [Tooltip("Maximum distance to consider ball in shootable position")]
    [SerializeField] private float shootableDistanceThreshold = 2.0f;

    [Tooltip("Tolerance for 'ball behind' check — ball this far behind figure still counts as shootable")]
    [SerializeField] private float ballBehindTolerance = 0.5f;

    [Header("Shot Configuration")]
    [Tooltip("Maximum time to charge a shot")]
    [SerializeField] private float maxChargeTime = 3f;

    [Tooltip("Charge time for light shot")]
    [SerializeField] private float lightShotThreshold = 1.0f;

    [Tooltip("Charge time for medium shot")]
    [SerializeField] private float mediumShotThreshold = 2.0f;

    [Header("Interrupt Settings")]
    [Tooltip("Enable charge interruption when ball moves away")]
    [SerializeField] private bool interruptEnabled = true;

    [Tooltip("Distance multiplier beyond shootable threshold to trigger interrupt")]
    [SerializeField] private float interruptDistanceMultiplier = 1.5f;

    [Tooltip("Ball speed threshold to trigger interrupt (ball flying away)")]
    [SerializeField] private float interruptBallSpeedThreshold = 8f;

    [Header("Charge Adaptation")]
    [Tooltip("Enable re-evaluating best figure during charge")]
    [SerializeField] private bool chargeAdaptationEnabled = false;

    [Tooltip("How often to re-evaluate best figure during charge (seconds)")]
    [SerializeField] private float figureReevalInterval = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    #endregion

    #region References

    private AIRodMovementAction rodMovement;
    private AIRodStateMachine stateMachine;
    private AIGoalEvaluator goalEvaluator;
    private AIRodMagnetAction magnetAction;
    private FoosballFigureAnimationController[] figures;
    private FoosballFigureShootAction[] shootActions;
    private GameObject ball;
    private bool isGoalkeeper = false;

    #endregion

    #region Charging State

    private bool isCharging = false;
    private float currentChargeTime = 0f;
    private float targetChargeTime = 0f;
    private int chargingFigureIndex = -1;
    private bool wasRodActive = false;
    private float figureReevalTimer = 0f;

    // Track failed charge attempts — if charge keeps restarting, let magnet stabilize first
    private int consecutiveChargeRestarts = 0;
    private float lastChargeStartTime = 0f;
    private const float CHARGE_RESTART_WINDOW = 2f; // Restarts within this window count as consecutive
    private const int MAX_CHARGE_RESTARTS = 3; // After this many restarts, yield to magnet
    private float chargeCooldownUntil = 0f; // Don't charge until this time
    private float chargeCooldownDuration = 1.5f; // Let magnet work for this long (difficulty-scaled)

    // GK clearing state — when true, GK is clearing toward defense rather than scoring
    private bool isGKClearing = false;

    #endregion

    #region Difficulty Parameters

    private float chargeTimeMultiplier = 0.75f; // Set by AITeamRodsController
    private float whiffChance = 0f; // Chance of whiffing a shot (0=never, set by difficulty)
    private float shotChargeTarget = 0.5f; // Target fraction of lightShotThreshold for non-GK shots (difficulty-scaled)

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        rodMovement = GetComponent<AIRodMovementAction>();
        stateMachine = GetComponent<AIRodStateMachine>();
        goalEvaluator = GetComponent<AIGoalEvaluator>();
        magnetAction = GetComponent<AIRodMagnetAction>();

        CollectFigures();
    }

    private void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Ball");
        isGoalkeeper = stateMachine != null && stateMachine.IsGoalkeeper;
    }

    private void Update()
    {
        if (ball == null)
        {
            ball = GameObject.FindGameObjectWithTag("Ball");
            return;
        }

        // Check if rod just became inactive - stop all particles
        if (wasRodActive && !rodMovement.isActive)
        {
            OnRodBecameInactive();
        }
        wasRodActive = rodMovement.isActive;

        // Handle charging (can happen while positioning)
        if (isCharging)
        {
            UpdateCharging();
        }
    }

    private void OnDisable()
    {
        StopCharging();
        ForceStopAllFigureParticles();
    }

    #endregion

    #region Initialization

    private void CollectFigures()
    {
        int childCount = transform.childCount;
        figures = new FoosballFigureAnimationController[childCount];
        shootActions = new FoosballFigureShootAction[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            figures[i] = child.GetComponent<FoosballFigureAnimationController>();
            shootActions[i] = child.GetComponent<FoosballFigureShootAction>();

            // Add shoot action component if it doesn't exist
            if (shootActions[i] == null && figures[i] != null)
            {
                shootActions[i] = child.gameObject.AddComponent<FoosballFigureShootAction>();
            }
        }
    }

    #endregion

    #region Rod State Change Handling

    /// <summary>
    /// Called when rod becomes inactive - stops all charging and particles
    /// </summary>
    private void OnRodBecameInactive()
    {
        if (isCharging)
        {
            StopCharging();
        }
        
        ForceStopAllFigureParticles();

        AIDebugLogger.Log(gameObject.name, "SHOOT_CHARGE", "Rod became inactive, stopped all particles");
    }

    /// <summary>
    /// Force stops all particles on all figures
    /// </summary>
    private void ForceStopAllFigureParticles()
    {
        for (int i = 0; i < shootActions.Length; i++)
        {
            if (shootActions[i] != null)
            {
                shootActions[i].ForceStopAllParticles();
            }
        }
    }

    #endregion

    #region Shoot Evaluation (CONDITION-BASED, NO PROBABILITY)

    /// <summary>
    /// Checks if shooting is allowed.
    /// Magnet being active alone does NOT block shooting — the magnet's job is to
    /// pull the ball into shootable position. Once the ball IS shootable, we should
    /// shoot immediately rather than waiting for magnet to deactivate first.
    /// </summary>
    private bool CanShoot()
    {
        return true;
    }

    /// <summary>
    /// Evaluates if shooting conditions are met
    /// Called by PositioningState periodically
    /// 
    /// RETURNS: True if should start/continue charging, false otherwise
    /// </summary>
    public bool EvaluateShoot()
    {
        if (!rodMovement.isActive || ball == null)
        {
            AIDebugLogger.LogShootEval(gameObject.name, false, "Rod inactive or no ball");
            return false;
        }

        // If charge keeps restarting, yield to magnet for a bit
        if (Time.time < chargeCooldownUntil)
        {
            AIDebugLogger.LogShootEval(gameObject.name, false, $"Charge cooldown — letting magnet stabilize ({chargeCooldownUntil - Time.time:F1}s left)");
            return false;
        }

        // Find best figure for shooting
        int figureIndex = FindBestFigureForShoot();

        if (figureIndex < 0)
        {
            // No figure in shootable position
            if (isCharging)
            {
                StopCharging();
            }
            AIDebugLogger.LogShootEval(gameObject.name, false, "No figure in shootable position");
            return false;
        }

        // Figure is in shootable position
        if (!isCharging)
        {
            // Start charging
            StartCharging(figureIndex);
        }

        AIDebugLogger.LogShootEval(gameObject.name, true, $"Figure {figureIndex} charging (charge: {currentChargeTime:F2}s)");
        return true;
    }

    /// <summary>
    /// Finds the best figure that can shoot
    /// 
    /// CONDITIONS:
    /// 1. Ball is CLOSE (distance < shootableDistanceThreshold)
    /// 2. Ball is IN FRONT of figure (based on team attack direction)
    /// 3. Shot quality is good (evaluated by AIGoalEvaluator)
    /// 
    /// Returns -1 if no figure meets conditions
    /// </summary>
    private int FindBestFigureForShoot()
    {
        if (ball == null || goalEvaluator == null) return -1;

        isGKClearing = false; // Reset — will be set to true only if GK clearing path is taken
        Vector2 ballPosition = ball.transform.position;
        TeamSide teamSide = stateMachine != null ? stateMachine.TeamSide : TeamSide.LeftTeam;

        int bestFigureIndex = -1;
        float bestShootingScore = 0f;

        for (int i = 0; i < figures.Length; i++)
        {
            if (figures[i] == null) continue;

            Transform figureTransform = figures[i].transform;
            Vector2 figurePos = figureTransform.position;
            float distanceToBall = Vector2.Distance(figurePos, ballPosition);

            // CONDITION 1: Check distance
            if (distanceToBall > shootableDistanceThreshold)
            {
                AIDebugLogger.LogShootEval(gameObject.name, false, $"Fig{i} too far (dist: {distanceToBall:F2} > {shootableDistanceThreshold:F2})");
                continue;
            }

            // CONDITION 2: Check if ball is IN FRONT (with tolerance for ball near/at figure)
            bool ballInFront = false;
            if (teamSide == TeamSide.LeftTeam)
            {
                // AI attacking RIGHT: ball.x >= figure.x - tolerance
                ballInFront = ballPosition.x >= figurePos.x - ballBehindTolerance;
            }
            else // RightTeam
            {
                // AI attacking LEFT: ball.x <= figure.x + tolerance
                ballInFront = ballPosition.x <= figurePos.x + ballBehindTolerance;
            }

            if (!ballInFront)
            {
                AIDebugLogger.LogShootEval(gameObject.name, false, $"Fig{i} ball behind (ballX: {ballPosition.x:F2}, figX: {figurePos.x:F2})");
                AIDebugLogger.Log(gameObject.name, "SHOOT_EVAL", $"Figure {i}: Ball behind figure (ballX: {ballPosition.x:F2}, figureX: {figurePos.x:F2})");
                continue;
            }

            // CONDITION 3: Evaluate shot quality
            ShootOpportunity shootOpp = goalEvaluator.EvaluateAngledShot(figureTransform);
            AIDebugLogger.LogShootEval(gameObject.name, false, $"Fig{i} shotScore: {shootOpp.shootingScore:F2}, clear: {shootOpp.isDirectShotClear}, dist: {distanceToBall:F2}");

            if (shootOpp.shootingScore > bestShootingScore)
            {
                bestShootingScore = shootOpp.shootingScore;
                bestFigureIndex = i;
            }
        }

        // Check if best score meets minimum threshold
        // Goalkeeper uses a lower threshold — prioritizes clearing over shot quality
        float minimumShootScore = stateMachine != null ? stateMachine.MinimumShootScore : 0.4f;
        if (isGoalkeeper)
        {
            // GK always tries to clear — use FindBestClearingTarget for direction,
            // but accept any shootable figure with very low threshold
            minimumShootScore *= 0.5f; // GK clears at half the normal threshold

            // If we have a figure in range but shot score is low, still clear toward defense
            if (bestFigureIndex >= 0 && bestShootingScore < minimumShootScore)
            {
                bool hasClearTarget;
                float clearTargetY = goalEvaluator.FindBestClearingTarget(out hasClearTarget);
                if (hasClearTarget)
                {
                    AIDebugLogger.Log(gameObject.name, "GK_CLEAR",
                        $"GK clearing toward defense Y={clearTargetY:F2} (shotScore too low: {bestShootingScore:F2})");
                    isGKClearing = true;
                    return bestFigureIndex;
                }
            }
        }
        if (bestShootingScore < minimumShootScore)
        {
            // Direct shot not good enough — check if passing forward is better
            if (goalEvaluator != null && bestFigureIndex >= 0)
            {
                PassOpportunity passOpp = goalEvaluator.EvaluatePassOpportunity(
                    figures[bestFigureIndex].transform, ball.transform.position);
                if (passOpp.shouldPass)
                {
                    AIDebugLogger.LogShootEval(gameObject.name, true, $"PASS to rod {passOpp.targetRodIndex} (passScore: {passOpp.passScore:F2}, shotScore: {bestShootingScore:F2})");
                    AIDebugLogger.Log(gameObject.name, "SHOOT_EVAL", $"Pass recommended to rod {passOpp.targetRodIndex} (passScore: {passOpp.passScore:F2})");
                    // Use a lighter shot toward the target rod (forward pass)
                    return bestFigureIndex;
                }
            }

            AIDebugLogger.LogShootEval(gameObject.name, false, $"Score too low ({bestShootingScore:F2} < {minimumShootScore:F2}), no pass available");
            AIDebugLogger.Log(gameObject.name, "SHOOT_EVAL", $"Best shooting score ({bestShootingScore:F2}) below threshold ({minimumShootScore:F2})");
            return -1;
        }

        AIDebugLogger.LogShootEval(gameObject.name, true, $"Fig{bestFigureIndex} READY (score: {bestShootingScore:F2} ≥ {minimumShootScore:F2})");
        if (bestFigureIndex >= 0)
        {
            AIDebugLogger.Log(gameObject.name, "SHOOT_EVAL", $"Figure {bestFigureIndex} ready to shoot (score: {bestShootingScore:F2})");
        }

        return bestFigureIndex;
    }

    #endregion

    #region Charging (PARALLEL WITH POSITIONING)

    /// <summary>
    /// Starts charging for a shot
    /// Charging happens WHILE rod continues positioning
    /// 
    /// UPDATED: Shows charging animation on ALL figures (they move together)
    /// </summary>
    private void StartCharging(int figureIndex)
    {
        if (isCharging) return;

        // Track consecutive charge restarts
        if (Time.time - lastChargeStartTime < CHARGE_RESTART_WINDOW)
        {
            consecutiveChargeRestarts++;
            if (consecutiveChargeRestarts >= MAX_CHARGE_RESTARTS)
            {
                // Too many restarts — enter cooldown to let magnet stabilize the ball
                chargeCooldownUntil = Time.time + chargeCooldownDuration;
                consecutiveChargeRestarts = 0;
                AIDebugLogger.LogShootEval(gameObject.name, false, $"Charge restarted {MAX_CHARGE_RESTARTS}x — cooldown for magnet");
                return;
            }
        }
        else
        {
            consecutiveChargeRestarts = 0;
        }
        lastChargeStartTime = Time.time;

        isCharging = true;
        currentChargeTime = 0f;
        chargingFigureIndex = figureIndex;

        // Deactivate magnet when charging starts — effector would fight the kick
        if (magnetAction != null && magnetAction.IsMagnetActive())
        {
            AIDebugLogger.Log(gameObject.name, "MAGNET_TO_SHOOT", "Magnet deactivated → charge started (magnet→shoot chain)");
            magnetAction.DeactivateMagnet();
        }

        // Determine target charge time based on shot quality
        targetChargeTime = CalculateTargetChargeTime();

        // Start charging animation on ALL figures (they move together)
        // Particles will only start when heavy threshold is reached
        for (int i = 0; i < figures.Length; i++)
        {
            if (figures[i] != null)
            {
                figures[i].StartCharging();
            }
        }

        for (int i = 0; i < shootActions.Length; i++)
        {
            if (shootActions[i] != null)
            {
                shootActions[i].StartCharging();
            }
        }

        AIDebugLogger.Log(gameObject.name, "SHOOT_CHARGE", $"Started charging ALL figures (target: {targetChargeTime:F2}s, triggered by figure {figureIndex})");
    }

    /// <summary>
    /// Updates charging progress
    /// Automatically executes shoot when fully charged
    /// 
    /// UPDATED: Updates charge animation and particle timing on ALL figures
    /// Now includes interrupt checks and figure re-evaluation
    /// </summary>
    private void UpdateCharging()
    {
        if (!isCharging) return;

        // Check for interrupt before continuing charge
        if (ShouldInterruptCharge())
        {
            AIDebugLogger.Log(gameObject.name, "SHOOT_CHARGE", "Charge interrupted - ball moved away");
            StopCharging();
            return;
        }

        // Re-evaluate best figure during charge (charge adaptation)
        if (chargeAdaptationEnabled)
        {
            figureReevalTimer += Time.deltaTime;
            if (figureReevalTimer >= figureReevalInterval)
            {
                figureReevalTimer = 0f;
                int bestFigure = FindBestFigureForShoot();
                if (bestFigure >= 0 && bestFigure != chargingFigureIndex)
                {
                    chargingFigureIndex = bestFigure;
                    AIDebugLogger.Log(gameObject.name, "SHOOT_CHARGE", $"Charge adapted to figure {bestFigure}");
                }
            }
        }

        // Increment charge time
        currentChargeTime = Mathf.Min(currentChargeTime + Time.deltaTime, maxChargeTime);

        // Update charge animation on ALL figures
        for (int i = 0; i < figures.Length; i++)
        {
            if (figures[i] != null)
            {
                figures[i].UpdateChargeAmount(currentChargeTime);
            }
        }

        // Update charge time on shoot actions for particle control
        for (int i = 0; i < shootActions.Length; i++)
        {
            if (shootActions[i] != null)
            {
                shootActions[i].UpdateChargeTime(currentChargeTime);
            }
        }

        // Check if charged enough
        if (currentChargeTime >= targetChargeTime)
        {
            ExecuteShoot();
        }
    }

    /// <summary>
    /// Calculates target charge time based on shot quality and difficulty
    /// Goalkeeper uses shorter charge time for quick clearances
    /// </summary>
    private float CalculateTargetChargeTime()
    {
        float baseChargeTime;

        if (isGoalkeeper)
        {
            if (isGKClearing)
            {
                // GK clearing: quick light kick to get ball to defense ASAP
                baseChargeTime = lightShotThreshold * 0.6f;
            }
            else
            {
                // GK scoring attempt: slightly longer charge
                baseChargeTime = lightShotThreshold * 0.8f;
            }
        }
        else
        {
            // Use difficulty-scaled shot charge target
            // shotChargeTarget controls how much of lightShotThreshold we aim for
            // Easy: 0.8 (slower, more telegraphed) → 0.8s charge
            // Medium: 0.6 (moderate) → 0.6s charge
            // Hard: 0.4 (quick taps like player) → 0.4s charge
            baseChargeTime = lightShotThreshold * shotChargeTarget;
        }

        // Apply difficulty multiplier
        float targetTime = baseChargeTime * chargeTimeMultiplier;

        // Clamp to valid range — minimum 0.15s for instant taps
        return Mathf.Clamp(targetTime, 0.15f, maxChargeTime);
    }

    /// <summary>
    /// Stops charging without shooting
    /// 
    /// UPDATED: Stops charging on ALL figures
    /// </summary>
    public void StopCharging()
    {
        if (!isCharging) return;

        isCharging = false;
        isGKClearing = false;

        // Stop charging on ALL figures
        for (int i = 0; i < shootActions.Length; i++)
        {
            if (shootActions[i] != null)
            {
                shootActions[i].StopCharging();
            }
        }

        // Reset charging animation on ALL figures
        for (int i = 0; i < figures.Length; i++)
        {
            if (figures[i] != null)
            {
                figures[i].TriggerKickAnimation(0f);
            }
        }

        currentChargeTime = 0f;
        chargingFigureIndex = -1;

        AIDebugLogger.Log(gameObject.name, "SHOOT_CHARGE", "Charging stopped on ALL figures");
    }

    #endregion

    #region Shoot Execution

    /// <summary>
    /// Executes the shot and transitions FSM to ShootingState
    /// This blocks movement during animation
    /// 
    /// UPDATED: Now activates ALL figures for shooting (not just one)
    /// Since all figures move together on the rod, they should all shoot together
    /// </summary>
    private void ExecuteShoot()
    {
        if (!isCharging || chargingFigureIndex < 0) return;

        // Whiff check — on Easy/Medium, AI may miss the shot
        bool isWhiff = whiffChance > 0f && Random.value < whiffChance;

        if (isWhiff)
        {
            AIDebugLogger.Log(gameObject.name, "SHOOT_WHIFF", $"WHIFFED! Charge: {currentChargeTime:F2}s (whiffChance: {whiffChance:F2})");
        }
        else
        {
            AIDebugLogger.Log(gameObject.name, "SHOOT_EXEC", $"SHOT FIRED! Charge: {currentChargeTime:F2}s, Figure: {chargingFigureIndex}");
        }

        isCharging = false;
        consecutiveChargeRestarts = 0; // Successful shot resets restart counter
        isGKClearing = false;

        // Get team side
        TeamSide teamSide = stateMachine != null ? stateMachine.TeamSide : TeamSide.LeftTeam;

        // Trigger kick animation on ALL figures (animation always plays, even on whiff)
        for (int i = 0; i < figures.Length; i++)
        {
            if (figures[i] != null)
            {
                figures[i].TriggerKickAnimation(currentChargeTime);
            }
        }

        // On whiff: use minimal charge time so shot has almost no force
        float effectiveCharge = isWhiff ? 0.05f : currentChargeTime;

        // Prepare shot on ALL figures (any figure might hit the ball)
        for (int i = 0; i < shootActions.Length; i++)
        {
            if (shootActions[i] != null)
            {
                shootActions[i].PrepareShot(0f, teamSide, effectiveCharge);
            }
        }

        AIDebugLogger.Log(gameObject.name, "SHOOT_EXEC", $"Shot executed on ALL figures (charge: {currentChargeTime:F2}s)");

        // Transition FSM to ShootingState (blocks movement during animation)
        if (stateMachine != null)
        {
            stateMachine.ChangeState<ShootingState>();
        }

        // Reset
        currentChargeTime = 0f;
        chargingFigureIndex = -1;
    }

    #endregion

    #region Charge Interruption

    /// <summary>
    /// Checks if the current charge should be interrupted
    /// Returns true if ball has moved away, is behind the figure, or is flying away
    /// </summary>
    private bool ShouldInterruptCharge()
    {
        if (!interruptEnabled) return false;
        if (ball == null || chargingFigureIndex < 0) return false;
        if (figures[chargingFigureIndex] == null) return false;

        Vector2 figurePos = figures[chargingFigureIndex].transform.position;
        Vector2 ballPosition = ball.transform.position;

        // 1. Ball out of shootable range?
        float distanceToBall = Vector2.Distance(figurePos, ballPosition);
        if (distanceToBall > shootableDistanceThreshold * interruptDistanceMultiplier)
            return true;

        // 2. Ball no longer in front of figure? (with tolerance)
        TeamSide teamSide = stateMachine != null ? stateMachine.TeamSide : TeamSide.LeftTeam;
        bool ballInFront;
        if (teamSide == TeamSide.LeftTeam)
            ballInFront = ballPosition.x >= figurePos.x - ballBehindTolerance;
        else
            ballInFront = ballPosition.x <= figurePos.x + ballBehindTolerance;

        if (!ballInFront)
            return true;

        // 3. Ball moving away at high speed?
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        if (ballRb != null)
        {
            Vector2 figureToBall = (ballPosition - figurePos).normalized;
            float velocityAwayFromFigure = Vector2.Dot(ballRb.linearVelocity, figureToBall);
            if (velocityAwayFromFigure > interruptBallSpeedThreshold)
                return true;
        }

        return false;
    }

    #endregion

    #region Public API

    /// <summary>
    /// Sets charge time multiplier (called by AITeamRodsController for difficulty)
    /// </summary>
    public void SetChargeTimeMultiplier(float multiplier)
    {
        chargeTimeMultiplier = Mathf.Clamp(multiplier, 0.1f, 2f);
    }

    /// <summary>
    /// Sets shootable distance threshold
    /// </summary>
    public void SetShootableDistanceThreshold(float threshold)
    {
        shootableDistanceThreshold = threshold;
    }

    /// <summary>
    /// Sets ball behind tolerance (how far behind a figure the ball can be and still count as shootable)
    /// </summary>
    public void SetBallBehindTolerance(float tolerance)
    {
        ballBehindTolerance = Mathf.Max(0f, tolerance);
    }

    /// <summary>
    /// Gets whether currently charging
    /// </summary>
    public bool IsCharging()
    {
        return isCharging;
    }

    /// <summary>
    /// Gets current charge time
    /// </summary>
    public float GetCurrentChargeTime()
    {
        return currentChargeTime;
    }

    /// <summary>
    /// Sets whether charge interruption is enabled (called by AITeamRodsController for difficulty)
    /// </summary>
    public void SetInterruptEnabled(bool enabled)
    {
        interruptEnabled = enabled;
    }

    /// <summary>
    /// Sets the interrupt distance multiplier
    /// </summary>
    public void SetInterruptDistanceMultiplier(float multiplier)
    {
        interruptDistanceMultiplier = Mathf.Max(1f, multiplier);
    }

    /// <summary>
    /// Sets the ball speed threshold for charge interruption
    /// </summary>
    public void SetInterruptBallSpeedThreshold(float threshold)
    {
        interruptBallSpeedThreshold = Mathf.Max(1f, threshold);
    }

    /// <summary>
    /// Sets whether charge adaptation (figure switching) is enabled
    /// </summary>
    public void SetChargeAdaptationEnabled(bool enabled)
    {
        chargeAdaptationEnabled = enabled;
    }

    /// <summary>
    /// Sets the whiff chance (probability of missing a shot)
    /// </summary>
    public void SetWhiffChance(float chance)
    {
        whiffChance = Mathf.Clamp01(chance);
    }

    public void SetShotChargeTarget(float target)
    {
        shotChargeTarget = Mathf.Clamp(target, 0.2f, 1.0f);
    }

    public void SetChargeCooldownDuration(float duration)
    {
        chargeCooldownDuration = Mathf.Max(0.3f, duration);
    }

    #endregion
}

using UnityEngine;

/// <summary>
/// Positioning State - Unified active state for rod behavior
/// 
/// PHILOSOPHY:
/// All rods are equal. Every rod can defend AND attack. The only thing that matters
/// is WHERE THE BALL IS and WHERE IT'S GOING.
/// 
/// This state replaces the old TrackingState and DefendingState with a unified approach:
/// - NO role-specific behavior (Goalkeeper vs Attacker)
/// - ALL decisions based on ball position and trajectory
/// - Movement adapts dynamically to game situation
/// - Actions (Shoot/Magnet/WallPass) are just tools to achieve goals
/// 
/// STATE PURPOSE:
/// - Keep rod positioned relative to ball
/// - Periodically evaluate action opportunities
/// - Delegate movement to AIRodMovementAction with appropriate mode
/// - Delegate actions to independent action components (PARALLEL EXECUTION)
/// 
/// REFACTORED: ACTIONS RUN IN PARALLEL
/// ================================================================
/// The key improvement is that actions now evaluate CONTINUOUSLY while positioning:
/// 
/// EVERY FRAME:
/// 1. AIRodMagnetAction evaluates → Activates/deactivates magnet
/// 2. Movement continues (AIRodMovementAction)
/// 
/// PERIODICALLY (decisionInterval):
/// 1. AIRodShootAction.EvaluateShoot() → Starts/continues charging if conditions met
/// 2. If charging complete → Executes shoot → Transitions to ShootingState
/// 3. If no shoot → Check wall pass conditions → AIRodWallPassAction.EvaluateAndExecuteWallPass()
/// 
/// MOVEMENT STRATEGY (Ball-Centric):
/// The rod analyzes the ball situation and chooses movement mode:
/// 
/// 1. BALL MOVING TOWARD OWN GOAL → Defensive positioning
///    - Use DefensiveBlocking or Intercepting mode
///    - Priority: Stop opponent from scoring
/// 
/// 2. BALL IN CONTESTED AREA → Tracking mode
///    - Follow ball, stay aligned
///    - Ready to transition to attack or defense
/// 
/// 3. BALL MOVING TOWARD OPPONENT GOAL → Attacking positioning
///    - Use AttackingPosition mode
///    - Priority: Create scoring opportunity
/// 
/// 4. BALL STATIC OR NEARBY → Opportunistic mode
///    - Position for best action (shoot/pass/control)
/// 
/// DECISION FLOW:
/// PositioningState → [Analyzes ball] → Sets MovementMode → Evaluates Actions (Parallel) → Shoot/WallPass/Continue
/// 
/// PROGRAMMING CONCEPTS:
/// - Single Responsibility: This state only handles positioning decisions
/// - Delegation: Movement execution delegated to AIRodMovementAction
/// - Parallel Execution: Actions evaluate independently and continuously
/// - Periodic Evaluation: Checks for action opportunities at intervals
/// - Context-Aware: Movement adapts to ball context dynamically
/// 
/// TRANSITIONS:
/// - To IdleState: When rod becomes inactive OR ball disappears
/// - To ShootingState: When AIRodShootAction executes shot
/// - To CooldownState: When AIRodWallPassAction executes wall pass
/// </summary>
public class PositioningState : AIRodState
{
    private float evaluationTimer = 0f;
    private MovementContext lastContext = MovementContext.Neutral;

    // Action component references
    private AIRodMagnetAction magnetAction;
    private AIRodShootAction shootAction;
    private AIRodWallPassAction wallPassAction;

    /// <summary>
    /// Movement context based on ball situation
    /// </summary>
    private enum MovementContext
    {
        Defensive,  // Ball threatening own goal
        Neutral,    // Ball in contested area
        Attacking   // Ball moving toward opponent goal
    }

    public PositioningState(AIRodStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        evaluationTimer = 0f;
        lastContext = MovementContext.Neutral;

        // Get action component references
        magnetAction = stateMachine.gameObject.GetComponent<AIRodMagnetAction>();
        shootAction = stateMachine.gameObject.GetComponent<AIRodShootAction>();
        wallPassAction = stateMachine.gameObject.GetComponent<AIRodWallPassAction>();

        AIDebugLogger.Log(stateMachine.gameObject.name, "POSITIONING", "Entering POSITIONING state - Ball-centric behavior active");
    }

    public override void Update()
    {
        // Check if rod is still active
        if (!IsRodActive() || GetBall() == null)
        {
            stateMachine.ChangeState<IdleState>();
            return;
        }

        // === ANALYZE BALL SITUATION ===
        MovementContext currentContext = AnalyzeBallContext();

        // === PRIORITY OVERRIDE: DEFENSIVE INTERCEPT ===
        // If ball is heading toward own goal at high speed, override lower-priority actions
        if (currentContext == MovementContext.Defensive)
        {
            Vector2 ballVelocity = GetBallVelocity();
            if (ballVelocity.magnitude > 5f)
            {
                stateMachine.TryInterruptWithPriority(AIActionPriority.DefensiveIntercept);
            }
        }
        else
        {
            // Reset priority when not in defensive emergency
            if (stateMachine.CurrentActionPriority == AIActionPriority.DefensiveIntercept)
            {
                stateMachine.SetActionPriority(AIActionPriority.Positioning);
            }
        }

        // Update movement mode if context changed
        if (currentContext != lastContext)
        {
            UpdateMovementMode(currentContext);
            lastContext = currentContext;
        }

        // === PERIODIC ACTION EVALUATION ===
        evaluationTimer += Time.deltaTime;

        if (evaluationTimer >= stateMachine.DecisionInterval)
        {
            evaluationTimer = 0f;

            // Check if ball is close enough to take action
            if (IsBallInActionRange())
            {
                EvaluateActions();
            }
        }

        // NOTE: AIRodMagnetAction evaluates EVERY FRAME automatically
        // We don't need to call it here - it runs independently
    }

    public override void Exit()
    {
        evaluationTimer = 0f;
    }

    public override string GetStateName()
    {
        return $"Positioning ({lastContext})";
    }

    #region Ball Analysis

    /// <summary>
    /// Analyzes ball situation to determine movement context
    /// 
    /// BALL-CENTRIC LOGIC:
    /// The only thing that matters is where the ball is and where it's going.
    /// No role-specific logic - same analysis for all rods.
    /// </summary>
    private MovementContext AnalyzeBallContext()
    {
        GameObject ball = GetBall();
        if (ball == null) return MovementContext.Neutral;

        Vector2 ballPosition = ball.transform.position;
        Vector2 rodPosition = stateMachine.transform.position;
        Vector2 ballVelocity = GetBallVelocity();

        // Get goal direction (direction we're defending)
        Vector2 ownGoalDirection = -stateMachine.GoalEvaluator.GoalDirection;

        // === DEFENSIVE CONTEXT ===
        // Ball moving toward own goal OR ball is behind this rod
        float velocityTowardOwnGoal = Vector2.Dot(ballVelocity.normalized, ownGoalDirection);
        bool ballMovingTowardOwnGoal = velocityTowardOwnGoal > 0.3f && ballVelocity.magnitude > 2f;

        Vector2 ballToRod = rodPosition - ballPosition;
        bool ballBehindRod = Vector2.Dot(ballToRod.normalized, stateMachine.GoalEvaluator.GoalDirection) > 0.5f;

        if (ballMovingTowardOwnGoal || ballBehindRod)
        {
            return MovementContext.Defensive;
        }

        // === ATTACKING CONTEXT ===
        // Ball moving toward opponent goal OR ball is in front of this rod
        float velocityTowardOpponentGoal = Vector2.Dot(ballVelocity.normalized, stateMachine.GoalEvaluator.GoalDirection);
        bool ballMovingTowardOpponentGoal = velocityTowardOpponentGoal > 0.3f && ballVelocity.magnitude > 2f;

        bool ballInFrontOfRod = Vector2.Dot(ballToRod.normalized, stateMachine.GoalEvaluator.GoalDirection) < -0.3f;

        if (ballMovingTowardOpponentGoal || ballInFrontOfRod)
        {
            return MovementContext.Attacking;
        }

        // === NEUTRAL CONTEXT ===
        // Ball is contested, slow, or in midfield
        return MovementContext.Neutral;
    }

    /// <summary>
    /// Gets ball velocity for trajectory analysis
    /// </summary>
    private Vector2 GetBallVelocity()
    {
        GameObject ball = GetBall();
        if (ball == null) return Vector2.zero;

        Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
        return rb != null ? rb.linearVelocity : Vector2.zero;
    }

    /// <summary>
    /// Checks if ball is close enough for this rod to take action
    /// </summary>
    private bool IsBallInActionRange()
    {
        int closestFigureIndex = FindClosestFigureIndex();
        if (closestFigureIndex < 0) return false;

        Transform closestFigure = stateMachine.Figures[closestFigureIndex].transform;
        float distanceToBall = GetDistanceToBall(closestFigure);

        return distanceToBall < stateMachine.DetectionDistance;
    }

    /// <summary>
    /// Checks if ball is BEHIND the rod (between this rod and own goal).
    /// When ball is behind, wall pass is useless — magnet should attract first.
    /// </summary>
    private bool IsBallBehindRod()
    {
        GameObject ball = GetBall();
        if (ball == null || stateMachine.GoalEvaluator == null) return false;

        int closestFigureIndex = FindClosestFigureIndex();
        if (closestFigureIndex < 0) return false;

        Vector2 ballPos = ball.transform.position;
        Vector2 figurePos = stateMachine.Figures[closestFigureIndex].transform.position;
        TeamSide teamSide = stateMachine.TeamSide;

        // Ball is "behind" when it's on the side of our own goal relative to the figure
        if (teamSide == TeamSide.LeftTeam)
            return ballPos.x < figurePos.x - 0.5f; // Ball is to the LEFT (own goal side)
        else
            return ballPos.x > figurePos.x + 0.5f; // Ball is to the RIGHT (own goal side)
    }

    #endregion

    #region Self-Blocking Prevention

    /// <summary>
    /// Determines if this rod should clear the lane to avoid blocking a teammate's shot/pass.
    /// Returns true when AI has possession on a rod that is BEHIND this rod (closer to own goal),
    /// meaning a forward shot from that rod would pass through this rod's figures.
    /// </summary>
    private bool ShouldClearLane()
    {
        AITeamRodsController teamController = stateMachine.GetComponentInParent<AITeamRodsController>();
        if (teamController == null || teamController.CurrentPossession != AITeamRodsController.BallPossession.AI)
            return false;

        GameObject ball = GetBall();
        if (ball == null) return false;

        float ballX = ball.transform.position.x;
        float rodX = stateMachine.transform.position.x;

        // "Behind" depends on team side:
        // LeftTeam attacks right → behind = lower X. If ball is at lower X than this rod, a rod behind has it.
        // RightTeam attacks left → behind = higher X.
        bool ballIsBehindThisRod;
        if (teamController.teamSide == TeamSide.LeftTeam)
            ballIsBehindThisRod = ballX < rodX;
        else
            ballIsBehindThisRod = ballX > rodX;

        if (!ballIsBehindThisRod) return false;

        // Check if the ball is close to a teammate rod behind this one
        // (i.e., the possessing rod is behind, and a shot would travel through this rod)
        float distanceBallToRod = Mathf.Abs(ballX - rodX);
        return distanceBallToRod > 0.5f; // Only clear if ball is meaningfully behind, not right at this rod
    }

    #endregion

    #region Movement Mode Selection

    /// <summary>
    /// Updates movement mode based on ball context
    /// 
    /// MOVEMENT PHILOSOPHY:
    /// The movement action handles HOW to move.
    /// This state only decides WHICH movement strategy to use based on ball context.
    /// 
    /// SELF-BLOCKING PREVENTION:
    /// When the AI has possession on a rod BEHIND this one, this rod switches to
    /// ClearingLane mode to move figures out of the way, preventing own shots/passes
    /// from being blocked by own figures.
    /// 
    /// GOALKEEPER SPECIALIZATION:
    /// When the rod is a goalkeeper, defensive behavior uses Intercepting mode
    /// (trajectory-based blocking) and neutral/attacking contexts still default
    /// to DefensiveBlocking to maintain stay-home positioning.
    /// </summary>
    private void UpdateMovementMode(MovementContext context)
    {
        AIRodMovementAction rodMovement = stateMachine.RodMovement;
        if (rodMovement == null) return;

        // Self-blocking prevention: if AI has possession behind this rod, clear the lane
        if (ShouldClearLane())
        {
            rodMovement.SetMovementMode(AIRodMovementAction.MovementMode.ClearingLane);
            AIDebugLogger.Log(stateMachine.gameObject.name, "POSITIONING", "CLEARING LANE - Teammate behind has ball");
            return;
        }

        // Goalkeeper specialization: always prioritize blocking/intercepting
        if (stateMachine.IsGoalkeeper)
        {
            UpdateGoalkeeperMovementMode(context, rodMovement);
            return;
        }

        switch (context)
        {
            case MovementContext.Defensive:
                // Ball threatening - use defensive positioning
                // Choose between blocking shots or intercepting trajectory
                Vector2 ballVelocity = GetBallVelocity();
                bool ballMovingFast = ballVelocity.magnitude > 5f;

                if (ballMovingFast)
                {
                    // Fast-moving ball - try to intercept
                    rodMovement.SetMovementMode(AIRodMovementAction.MovementMode.Intercepting);
                }
                else
                {
                    // Slow ball or static - block shooting lanes
                    rodMovement.SetMovementMode(AIRodMovementAction.MovementMode.DefensiveBlocking);
                }

                AIDebugLogger.Log(stateMachine.gameObject.name, "POSITIONING", $"Context: DEFENSIVE - Mode: {rodMovement.GetMovementMode()}");
                break;

            case MovementContext.Attacking:
                // Ball moving toward opponent - position for shot
                rodMovement.SetMovementMode(AIRodMovementAction.MovementMode.AttackingPosition);

                AIDebugLogger.Log(stateMachine.gameObject.name, "POSITIONING", "Context: ATTACKING - Mode: AttackingPosition");
                break;

            case MovementContext.Neutral:
                // Contested ball - simple tracking
                rodMovement.SetMovementMode(AIRodMovementAction.MovementMode.Tracking);

                AIDebugLogger.Log(stateMachine.gameObject.name, "POSITIONING", "Context: NEUTRAL - Mode: Tracking");
                break;
        }
    }

    /// <summary>
    /// Goalkeeper-specific movement mode selection.
    /// GK uses GoalkeeperIntercept for trajectory prediction with goal post awareness.
    /// Falls back to DefensiveBlocking when ball is far away.
    /// </summary>
    private void UpdateGoalkeeperMovementMode(MovementContext context, AIRodMovementAction rodMovement)
    {
        Vector2 ballVelocity = GetBallVelocity();

        switch (context)
        {
            case MovementContext.Defensive:
                // Ball coming toward goal — use GK-specific intercept with trajectory prediction
                rodMovement.SetMovementMode(AIRodMovementAction.MovementMode.GoalkeeperIntercept);
                break;

            case MovementContext.Neutral:
                // Ball contested — use GK intercept to stay ready (lower threat = more tracking-like)
                if (ballVelocity.magnitude > 2f)
                    rodMovement.SetMovementMode(AIRodMovementAction.MovementMode.GoalkeeperIntercept);
                else
                    rodMovement.SetMovementMode(AIRodMovementAction.MovementMode.DefensiveBlocking);
                break;

            case MovementContext.Attacking:
                // Ball far away — stay-home: block center
                rodMovement.SetMovementMode(AIRodMovementAction.MovementMode.DefensiveBlocking);
                break;
        }

        AIDebugLogger.Log(stateMachine.gameObject.name, "POSITIONING", $"GK Context: {context} - Mode: {rodMovement.GetMovementMode()}");
    }

    #endregion

    #region Action Evaluation (CONDITION-BASED, NO PROBABILITY)

    /// <summary>
    /// Evaluates which action to take
    /// 
    /// NEW APPROACH: CONDITION-BASED, NO PROBABILITY
    /// 
    /// DECISION HIERARCHY:
    /// 1. Check SHOOT conditions → If met, AIRodShootAction charges/executes
    /// 2. If shooting conditions not met → Check WALL PASS conditions (proactive)
    /// 3. Magnet evaluates continuously (handled by AIRodMagnetAction automatically)
    /// 
    /// TACTICAL WALL PASS:
    /// Wall pass is now proactive — if ball has been near this rod without a good
    /// shot for several evaluation cycles, try wall pass to create new angles.
    /// 
    /// NO PROBABILITY CHECKS - Pure condition-based logic
    /// </summary>
    private int evaluationCyclesWithoutShot = 0;
    private const int WALL_PASS_PATIENCE = 3; // Try wall pass after this many cycles without shooting

    // Follow-up shot after wall pass or magnet release
    private bool pendingFollowUpShot = false;
    private float followUpShotTime = 0f;
    private const float wallPassFollowUpDelay = 0.3f; // Shoot shortly after wall pass bounces

    private void EvaluateActions()
    {
        bool ballBehind = IsBallBehindRod();
        AIDebugLogger.Log(stateMachine.gameObject.name, "ACTION_EVAL", $"Cycle {evaluationCyclesWithoutShot}, ball in range: {IsBallInActionRange()}, ball behind: {ballBehind}");

        // === PRIORITY 0: FOLLOW-UP SHOT after wall pass/magnet combo ===
        if (pendingFollowUpShot && Time.time >= followUpShotTime)
        {
            pendingFollowUpShot = false;
            if (shootAction != null)
            {
                bool followUpShot = shootAction.EvaluateShoot();
                if (followUpShot)
                {
                    AIDebugLogger.Log(stateMachine.gameObject.name, "COMBO_SHOT", "Follow-up shot after wall pass");
                    evaluationCyclesWithoutShot = 0;
                    return;
                }
            }
        }

        // === PRIORITY 1: EVALUATE SHOOTING ===
        if (shootAction != null)
        {
            bool shouldShoot = shootAction.EvaluateShoot();

            if (shouldShoot)
            {
                evaluationCyclesWithoutShot = 0;
                return;
            }
        }

        evaluationCyclesWithoutShot++;

        // === PRIORITY 2: EVALUATE WALL PASS (proactive) ===
        // SKIP wall pass when ball is behind the rod — magnet should attract first.
        // Wall pass makes no sense when ball isn't in front of figures.
        if (ballBehind)
        {
            AIDebugLogger.Log(stateMachine.gameObject.name, "NO_ACTION", $"Ball behind rod — waiting for magnet to attract (cycles: {evaluationCyclesWithoutShot})");
            return;
        }

        // Try wall pass when: (a) traditional conditions met (blocked), OR
        //                      (b) ball has been nearby for too long without shooting
        if (wallPassAction != null && stateMachine.GoalEvaluator != null)
        {
            int closestFigureIndex = FindClosestFigureIndex();
            if (closestFigureIndex >= 0)
            {
                Transform closestFigure = stateMachine.Figures[closestFigureIndex].transform;

                bool shouldWallPass = false;

                // Traditional: shot is blocked by multiple opponents
                bool blockedShot = stateMachine.GoalEvaluator.ShouldUseWallPass(closestFigure);
                if (blockedShot)
                    shouldWallPass = true;

                // Proactive: ball has been near this rod for several cycles but no shot taken
                if (!shouldWallPass && evaluationCyclesWithoutShot >= WALL_PASS_PATIENCE && IsBallInActionRange())
                    shouldWallPass = true;

                if (shouldWallPass && wallPassAction.CanPerformWallPass())
                {
                    string reason = blockedShot ? "shots blocked" : $"proactive (no shot for {evaluationCyclesWithoutShot} cycles)";
                    AIDebugLogger.LogWallPass(stateMachine.gameObject.name, true, $"Triggered: {reason}");
                    AIDebugLogger.Log(stateMachine.gameObject.name, "AI_DECISION", $"AI Decision: WALL PASS ({reason})");

                    wallPassAction.EvaluateAndExecuteWallPass();
                    evaluationCyclesWithoutShot = 0;

                    // Schedule quick follow-up shoot after wall pass (combo: wallpass→shoot)
                    pendingFollowUpShot = true;
                    followUpShotTime = Time.time + wallPassFollowUpDelay;
                    return;
                }
            }
        }

        AIDebugLogger.Log(stateMachine.gameObject.name, "NO_ACTION", $"No shoot or wallpass (cycles without shot: {evaluationCyclesWithoutShot})");
    }

    #endregion
}

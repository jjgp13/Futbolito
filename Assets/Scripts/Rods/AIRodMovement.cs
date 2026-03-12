using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIRodMovementAction : MonoBehaviour
{
    [Header("References")]
    public GameObject ball;
    public bool isActive;

    [Header("Movement Configuration")]
    public float speed;
    public float nearDistance;

    [Header("Intelligent Positioning")]
    [SerializeField] private MovementMode currentMode = MovementMode.Tracking;
    [SerializeField] private float defensiveSpacing = 1.5f; // Gap coverage distance
    [SerializeField] private float predictionTime = 0.3f; // How far ahead to predict ball movement

    // Rod information
    private int paddlesInLine;
    private List<GameObject> figures = new List<GameObject>();
    private RodConfiguration rodConfig;

    // Team and positioning context
    private AITeamRodsController teamController;
    private TeamSide teamSide;
    private Vector2 goalDirection;

    // Ball prediction
    private Rigidbody2D ballRigidbody;
    private Vector2 predictedBallPosition;

    // Difficulty-scaled behavior quality
    private float positioningAccuracy = 1f; // 1=perfect, lower=overshoot
    private float movementSpeedMultiplier = 1f; // 1=full speed, lower=slower

    // BumpNudge state
    private MovementMode preBumpMode = MovementMode.Tracking;
    private float bumpTimer;
    private float bumpSlamDuration = 0.2f;
    private float bumpReverseDuration = 0.2f;
    private int bumpPhase; // 0=slam, 1=reverse
    private float bumpTargetY;

    public enum MovementMode
    {
        Tracking,           // Simple ball tracking (default)
        DefensiveBlocking,  // Block shooting lanes
        DefensiveCovering,  // Cover gaps with other rod
        AttackingPosition,  // Position for shot opportunity
        Intercepting,       // Move to intercept ball trajectory
        ClearingLane,       // Move figures out of teammate's shooting/passing lane
        CenteringIdle,      // Move to center position before going idle
        GoalkeeperIntercept, // GK-specific: predict ball crossing point at GK's X position
        BumpNudge           // Quick slam to nudge nearby ball via RodBumpEffect
    }

    // Use this for initialization
    void Start()
    {
        // Find ball and get rigidbody for prediction
        ball = GameObject.FindGameObjectWithTag("Ball");
        if (ball != null)
        {
            ballRigidbody = ball.GetComponent<Rigidbody2D>();
        }

        // Get rod configuration
        rodConfig = GetComponent<RodConfiguration>();
        paddlesInLine = rodConfig.rodFoosballFigureCount;
        RodConfigurationSpeed(paddlesInLine);

        // Get team context
        teamController = GetComponentInParent<AITeamRodsController>();
        if (teamController != null)
        {
            teamSide = teamController.teamSide;
            goalDirection = teamSide == TeamSide.LeftTeam ? Vector2.right : Vector2.left;
        }

        // Populate list of child figures
        for (int i = 0; i < transform.childCount; i++)
            figures.Add(transform.GetChild(i).gameObject);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isActive || ball == null)
        {
            // If centering, continue until done
            if (currentMode == MovementMode.CenteringIdle && ball != null)
            {
                ExecuteCenteringMovement();
                ApplyMovementLimits();
            }
            else if (ball == null)
            {
                ball = GameObject.FindGameObjectWithTag("Ball");
            }
            return;
        }

        // Update ball prediction
        UpdateBallPrediction();

        // Execute movement based on current mode
        switch (currentMode)
        {
            case MovementMode.Tracking:
                ExecuteTrackingMovement();
                break;
            case MovementMode.DefensiveBlocking:
                ExecuteDefensiveBlocking();
                break;
            case MovementMode.DefensiveCovering:
                ExecuteDefensiveCovering();
                break;
            case MovementMode.AttackingPosition:
                ExecuteAttackingPosition();
                break;
            case MovementMode.Intercepting:
                ExecuteInterceptingMovement();
                break;
            case MovementMode.ClearingLane:
                ExecuteClearingLaneMovement();
                break;
            case MovementMode.CenteringIdle:
                ExecuteCenteringMovement();
                break;
            case MovementMode.GoalkeeperIntercept:
                ExecuteGoalkeeperInterceptMovement();
                break;
            case MovementMode.BumpNudge:
                ExecuteBumpNudgeMovement();
                break;
        }

        // Apply movement limits
        ApplyMovementLimits();
    }

    #region Movement Mode Execution

    /// <summary>
    /// TRACKING MODE - Simple ball following (original behavior)
    /// Moves nearest figure to align with ball's Y position
    /// 
    /// UPDATED: Now uses GetBestAvailableFigure() instead of GetNearestPaddle()
    /// </summary>
    private void ExecuteTrackingMovement()
    {
        GameObject bestFigure = GetBestAvailableFigure();
        if (bestFigure == null) return;
        Vector2 targetPosition = ball.transform.position;

        MoveTowardYPosition(targetPosition.y, bestFigure.transform.position.y);
    }

    /// <summary>
    /// DEFENSIVE BLOCKING MODE - Block direct shooting lanes
    /// 
    /// STRATEGY:
    /// - Calculate shooting angle from opponent's closest figure to goal
    /// - Position figures to block the most dangerous angles
    /// - Cover center first, then spread to cover wider angles
    /// </summary>
    private void ExecuteDefensiveBlocking()
    {
        // Find opponent's closest attacking figure
        Transform opponentFigure = FindClosestOpponentFigure();
        if (opponentFigure == null)
        {
            // Fallback to tracking if no opponent found
            ExecuteTrackingMovement();
            return;
        }

        // Calculate blocking position
        Vector2 blockingPosition = CalculateBlockingPosition(opponentFigure.position);

        // Get figure that should do the blocking
        GameObject blockingFigure = GetOptimalBlockingFigure(blockingPosition.y);

        MoveTowardYPosition(blockingPosition.y, blockingFigure.transform.position.y);
    }

    /// <summary>
    /// DEFENSIVE COVERING MODE - Cover gaps with teammate rod
    /// 
    /// STRATEGY:
    /// - Check where teammate rod is positioned
    /// - Cover the gap that teammate isn't covering
    /// - Distribute coverage across goal mouth
    /// 
    /// UPDATED: Now uses GetBestAvailableFigure() instead of GetNearestPaddle()
    /// </summary>
    private void ExecuteDefensiveCovering()
    {
        // Get teammate rod (other defensive rod)
        GameObject teammateRod = GetTeammateDefensiveRod();
        if (teammateRod == null)
        {
            ExecuteDefensiveBlocking();
            return;
        }

        AIRodMovementAction teammateMovement = teammateRod.GetComponent<AIRodMovementAction>();
        if (teammateMovement == null)
        {
            ExecuteDefensiveBlocking();
            return;
        }

        // Calculate gap coverage position
        float teammateY = teammateRod.transform.position.y;
        float coverageY = CalculateGapCoveragePosition(teammateY);

        // Get best available figure to execute the coverage
        GameObject coveringFigure = GetBestAvailableFigure();
        if (coveringFigure == null) return;

        MoveTowardYPosition(coverageY, coveringFigure.transform.position.y);
    }

    /// <summary>
    /// ATTACKING POSITION MODE - Position for optimal shot
    /// 
    /// STRATEGY:
    /// - Predict ball arrival position
    /// - Position figure slightly offset for angled shot
    /// - Create shooting angle opportunities
    /// 
    /// UPDATED: Now uses GetBestAvailableFigure() instead of GetNearestPaddle()
    /// </summary>
    private void ExecuteAttackingPosition()
    {
        Vector2 targetPosition = predictedBallPosition;

        // Add slight offset for angled shot opportunity
        float angleOffset = 0.5f; // Offset to create shooting angle
        targetPosition.y += Random.Range(-angleOffset, angleOffset);

        GameObject attackingFigure = GetBestAvailableFigure();
        if (attackingFigure == null) return;
        MoveTowardYPosition(targetPosition.y, attackingFigure.transform.position.y);
    }

    /// <summary>
    /// INTERCEPTING MODE - Move to intercept ball trajectory
    /// 
    /// STRATEGY:
    /// - Predict where ball will be in near future
    /// - Move to intercept path
    /// - Useful for balls passing through the field
    /// </summary>
    private void ExecuteInterceptingMovement()
    {
        Vector2 interceptPosition = predictedBallPosition;
        GameObject interceptingFigure = GetFigureClosestToPath(interceptPosition);

        MoveTowardYPosition(interceptPosition.y, interceptingFigure.transform.position.y);
    }

    /// <summary>
    /// CLEARING LANE MODE - Move figures out of teammate's shooting/passing lane
    /// 
    /// STRATEGY:
    /// - When a rod behind this one has the ball, this rod's figures would block shots/passes
    /// - Move figures away from the ball's Y position to clear the path
    /// - Figures move to the opposite side of the ball to open a lane
    /// </summary>
    private void ExecuteClearingLaneMovement()
    {
        if (ball == null) return;

        float ballY = ball.transform.position.y;

        // Find which figure is closest to blocking the lane
        GameObject blockingFigure = GetFigureClosestToPath(new Vector2(0, ballY));
        float figureY = blockingFigure.transform.position.y;

        // Move away from the ball's Y axis to clear the lane
        // Move toward whichever side has more room
        float minY = -rodConfig.rodMovementLimit + rodConfig.halfPlayer;
        float maxY = rodConfig.rodMovementLimit - rodConfig.halfPlayer;
        float roomAbove = maxY - figureY;
        float roomBelow = figureY - minY;

        // Target: move figures away from ball Y by at least clearingOffset
        float clearingOffset = 1.5f;
        float targetY;

        if (Mathf.Abs(figureY - ballY) > clearingOffset)
        {
            // Already clear — hold position
            return;
        }

        // Move toward the side with more room
        if (roomAbove > roomBelow)
            targetY = ballY + clearingOffset;
        else
            targetY = ballY - clearingOffset;

        MoveTowardYPosition(targetY, figureY);
    }

    /// <summary>
    /// CENTERING MODE - Move rod to center (Y=0) before going idle
    /// Used for GK on Medium/Hard: keeps GK centered so it can react equally to both sides
    /// </summary>
    private void ExecuteCenteringMovement()
    {
        if (figures.Count == 0) return;

        float currentY = figures[0].transform.position.y;
        float centerThreshold = 0.15f;

        if (Mathf.Abs(currentY) < centerThreshold)
        {
            // Close enough to center — stop centering
            currentMode = MovementMode.Tracking;
            return;
        }

        MoveTowardYPosition(0f, currentY);
    }

    /// <summary>
    /// Start centering movement (called before deactivation)
    /// </summary>
    public void StartCentering()
    {
        currentMode = MovementMode.CenteringIdle;
    }

    /// <summary>
    /// GOALKEEPER INTERCEPT MODE - Predict where ball will cross GK's X position
    /// 
    /// STRATEGY:
    /// - Use ball velocity to predict WHERE the ball will arrive at GK's X coordinate
    /// - Weight prediction vs current ball Y based on threat level (ball speed + direction)
    /// - Apply goal post bias: when ball is on one side, GK biases toward that side
    /// - Clamp to goal post boundaries to never leave goal undefended
    /// </summary>
    private void ExecuteGoalkeeperInterceptMovement()
    {
        if (ballRigidbody == null)
        {
            ExecuteDefensiveBlocking();
            return;
        }

        Vector2 ballPos = ball.transform.position;
        Vector2 ballVel = ballRigidbody.linearVelocity;
        float gkX = transform.position.x;

        // Calculate threat level (0-1) based on ball speed and direction toward GK
        Vector2 ownGoalDir = teamSide == TeamSide.LeftTeam ? Vector2.left : Vector2.right;
        float speedTowardGoal = Vector2.Dot(ballVel, ownGoalDir);
        float threatLevel = Mathf.Clamp01(speedTowardGoal / 10f); // 10 m/s = max threat

        // Predict where ball crosses GK's X position
        float predictedY = ballPos.y;
        if (speedTowardGoal > 1f)
        {
            // Ball is moving toward our goal — calculate crossing point
            float distanceToGK = Mathf.Abs(ballPos.x - gkX);
            float timeToReach = distanceToGK / Mathf.Abs(speedTowardGoal);
            timeToReach = Mathf.Min(timeToReach, 1f); // Cap prediction to 1 second
            predictedY = ballPos.y + ballVel.y * timeToReach;
        }

        // Blend between current ball Y (low threat) and predicted crossing Y (high threat)
        float targetY = Mathf.Lerp(ballPos.y, predictedY, threatLevel);

        // Goal post awareness: bias toward ball side, clamp to goal area
        float goalPostBias = 0.3f; // How much to bias toward ball side
        float ballSideBias = Mathf.Sign(ballPos.y) * goalPostBias * (1f - threatLevel);
        targetY += ballSideBias;

        // Clamp to goal movement limits (GK should never leave goal undefended)
        float minY = -rodConfig.rodMovementLimit + rodConfig.halfPlayer;
        float maxY = rodConfig.rodMovementLimit - rodConfig.halfPlayer;
        targetY = Mathf.Clamp(targetY, minY, maxY);

        // Use the figure closest to the predicted crossing point
        GameObject interceptFigure = GetFigureClosestToPath(new Vector2(gkX, targetY));

        AIDebugLogger.Log(gameObject.name, "GK_INTERCEPT",
            $"threat:{threatLevel:F2} predicted_y:{predictedY:F2} target_y:{targetY:F2} ball_speed:{ballVel.magnitude:F1}");

        MoveTowardYPosition(targetY, interceptFigure.transform.position.y);
    }

    /// <summary>
    /// BUMP NUDGE MODE — Quick slam toward ball's Y then reverse.
    /// The fast direction change triggers RodBumpEffect to nudge the ball.
    /// Auto-returns to previous mode after one oscillation cycle.
    /// </summary>
    private void ExecuteBumpNudgeMovement()
    {
        if (ball == null)
        {
            currentMode = preBumpMode;
            return;
        }

        bumpTimer += Time.deltaTime;
        float effectiveSpeed = speed * movementSpeedMultiplier * 1.5f; // 50% faster for the slam

        if (bumpPhase == 0)
        {
            // Phase 0: Slam toward ball's Y position
            float direction = Mathf.Sign(bumpTargetY - transform.position.y);
            transform.Translate(Vector2.up * direction * effectiveSpeed * Time.deltaTime);
            currentVelocity = direction * effectiveSpeed;

            if (bumpTimer >= bumpSlamDuration)
            {
                bumpPhase = 1;
                bumpTimer = 0f;
            }
        }
        else
        {
            // Phase 1: Reverse direction (this triggers the bump)
            float direction = -Mathf.Sign(bumpTargetY - transform.position.y);
            transform.Translate(Vector2.up * direction * effectiveSpeed * Time.deltaTime);
            currentVelocity = direction * effectiveSpeed;

            if (bumpTimer >= bumpReverseDuration)
            {
                // Done — return to previous mode
                currentMode = preBumpMode;
                bumpPhase = 0;
                bumpTimer = 0f;
            }
        }
    }

    #endregion

    #region Position Calculation Helpers

    /// <summary>
    /// Calculates optimal blocking position against opponent figure
    /// </summary>
    private Vector2 CalculateBlockingPosition(Vector2 opponentPosition)
    {
        // Get goal position
        Vector2 goalPosition = GetOwnGoalPosition();

        // Calculate shooting angle from opponent to goal
        Vector2 opponentToGoal = (goalPosition - opponentPosition).normalized;

        // Position on the line between opponent and goal
        float blockingDistance = 2f; // Distance from this rod's X position
        Vector2 blockingPos = opponentPosition + opponentToGoal * blockingDistance;

        // Clamp to valid Y range
        float clampedY = Mathf.Clamp(blockingPos.y,
            -rodConfig.rodMovementLimit,
            rodConfig.rodMovementLimit);

        return new Vector2(transform.position.x, clampedY);
    }

    /// <summary>
    /// Calculates position to cover gap left by teammate rod
    /// </summary>
    private float CalculateGapCoveragePosition(float teammateY)
    {
        // Divide goal coverage into zones
        float goalHeight = rodConfig.rodMovementLimit * 2f;
        float zoneHeight = goalHeight / 3f; // Divide into 3 zones

        // Determine which zone teammate is in
        int teammateZone = Mathf.FloorToInt((teammateY + rodConfig.rodMovementLimit) / zoneHeight);

        // Cover a different zone
        int coverageZone = (teammateZone + 1) % 3;

        // Calculate Y position for coverage zone
        float coverageY = -rodConfig.rodMovementLimit + (coverageZone * zoneHeight) + (zoneHeight / 2f);

        return coverageY;
    }

    /// <summary>
    /// Updates predicted ball position based on current velocity
    /// </summary>
    private void UpdateBallPrediction()
    {
        if (ballRigidbody != null)
        {
            Vector2 currentPos = ball.transform.position;
            Vector2 velocity = ballRigidbody.linearVelocity;
            predictedBallPosition = currentPos + velocity * predictionTime;
        }
        else
        {
            predictedBallPosition = ball.transform.position;
        }
    }

    /// <summary>
    /// Gets the figure closest to a specific Y position on predicted path
    /// </summary>
    private GameObject GetFigureClosestToPath(Vector2 targetPosition)
    {
        GameObject closestFigure = figures[0];
        float closestDistance = Mathf.Abs(closestFigure.transform.position.y - targetPosition.y);

        for (int i = 1; i < figures.Count; i++)
        {
            float distance = Mathf.Abs(figures[i].transform.position.y - targetPosition.y);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestFigure = figures[i];
            }
        }

        return closestFigure;
    }

    /// <summary>
    /// Gets figure optimal for blocking at specific Y position
    /// </summary>
    private GameObject GetOptimalBlockingFigure(float targetY)
    {
        return GetFigureClosestToPath(new Vector2(0, targetY));
    }

    #endregion

    #region Team Coordination

    /// <summary>
    /// Finds teammate defensive rod for gap coverage
    /// </summary>
    private GameObject GetTeammateDefensiveRod()
    {
        if (teamController == null) return null;

        // Get all rods from team controller
        GameObject[] rods = teamController.rods;

        // Find other active defensive rod (not this one)
        foreach (GameObject rod in rods)
        {
            if (rod == gameObject) continue;

            AIRodMovementAction rodMovement = rod.GetComponent<AIRodMovementAction>();
            if (rodMovement != null && rodMovement.isActive)
            {
                // Check if it's a defensive rod (goalkeeper or defense line)
                if (IsDefensiveRod(rod))
                {
                    return rod;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Checks if a rod is defensive (goalkeeper or defense)
    /// </summary>
    private bool IsDefensiveRod(GameObject rod)
    {
        // Check rod name or index in team controller
        string rodName = rod.name.ToLower();
        return rodName.Contains("goalkeeper") ||
     rodName.Contains("defense") ||
         rodName.Contains("gk") ||
       rodName.Contains("def");
    }

    /// <summary>
    /// Gets own goal position for defensive calculations
    /// </summary>
    private Vector2 GetOwnGoalPosition()
    {
        // Find goal based on team side
        string goalTag = teamSide == TeamSide.LeftTeam ? "LeftGoalTrigger" : "RightGoalTrigger";
        GameObject goal = GameObject.FindGameObjectWithTag(goalTag);

        if (goal != null)
        {
            return goal.transform.position;
        }

        // Fallback: estimate based on field bounds and team side
        float goalX = teamSide == TeamSide.LeftTeam ? -15f : 15f;
        return new Vector2(goalX, 0f);
    }

    /// <summary>
    /// Finds closest opponent figure to ball (threat assessment)
    /// </summary>
    private Transform FindClosestOpponentFigure()
    {
        GameObject[] allFigures = GameObject.FindGameObjectsWithTag("RodFigure");
        Transform closestOpponent = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject figure in allFigures)
        {
            // Check if figure belongs to opponent team
            if (!IsOwnTeamFigure(figure))
            {
                float distance = Vector2.Distance(figure.transform.position, ball.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestOpponent = figure.transform;
                }
            }
        }

        return closestOpponent;
    }

    /// <summary>
    /// Checks if a figure belongs to own team
    /// </summary>
    private bool IsOwnTeamFigure(GameObject figure)
    {
        // Check if figure is child of any rod in this team
        Transform parent = figure.transform.parent;
        if (parent == null) return false;

        AITeamRodsController figureTeam = parent.GetComponentInParent<AITeamRodsController>();
        return figureTeam == teamController;
    }

    #endregion

    #region Movement Execution

    // Exposed for shot angle calculation
    [HideInInspector] public float currentVelocity;

    /// <summary>
    /// Moves rod toward target Y position
    /// Applies positioning accuracy (overshoot on lower difficulty) and speed multiplier
    /// </summary>
    private void MoveTowardYPosition(float targetY, float currentY)
    {
        float effectiveSpeed = speed * movementSpeedMultiplier;

        // Apply positioning accuracy — when < 1, the rod overshoots the target
        // by moving past the target before correcting
        if (positioningAccuracy < 1f)
        {
            float distance = Mathf.Abs(targetY - currentY);
            float overshootThreshold = 0.1f; // Only apply when close enough
            if (distance < overshootThreshold)
            {
                // Close to target — accuracy determines if we stop or overshoot
                // Lower accuracy = higher chance of overshooting past target
                float overshootFactor = (1f - positioningAccuracy) * 2f;
                targetY += (targetY - currentY) * overshootFactor;
            }
        }

        if (targetY > currentY)
        {
            transform.Translate(Vector2.up * effectiveSpeed * Time.deltaTime);
            currentVelocity = effectiveSpeed;
        }
        else if (targetY < currentY)
        {
            transform.Translate(-Vector2.up * effectiveSpeed * Time.deltaTime);
            currentVelocity = -effectiveSpeed;
        }
        else
        {
            currentVelocity = 0f;
        }
    }

    /// <summary>
    /// Applies movement limits to keep rod in bounds
    /// </summary>
    private void ApplyMovementLimits()
    {
        float minY = -rodConfig.rodMovementLimit + rodConfig.halfPlayer;
        float maxY = rodConfig.rodMovementLimit - rodConfig.halfPlayer;

        if (transform.position.y < minY)
            transform.position = new Vector2(transform.position.x, minY);
        if (transform.position.y > maxY)
            transform.position = new Vector2(transform.position.x, maxY);
    }

    #endregion

    #region Legacy Methods (preserved for compatibility)

    /// <summary>
    /// Given the number of paddles in this line. Calculate the one who is nearest to the ball
    /// 
    /// DEPRECATED: Use GetBestAvailableFigure() instead for better figure selection
    /// This method only considers distance, not movement limits or figure availability
    /// </summary>
    GameObject GetNearestPaddle(List<GameObject> paddlesInThisLine)
    {
        GameObject nearChild = paddlesInThisLine[0];
        nearDistance = Vector2.Distance(ball.transform.position, nearChild.transform.position);

        for (int i = 1; i < paddlesInThisLine.Count; i++)
        {
            float distance = Vector2.Distance(ball.transform.position, paddlesInThisLine[i].transform.position);
            if (distance < nearDistance)
            {
                nearDistance = distance;
                nearChild = paddlesInThisLine[i];
            }
        }

        return nearChild;
    }

    /// <summary>
    /// Gets the best available figure considering:
    /// - Distance to ball
    /// - Whether figure can reach ball (not stuck at movement limit)
    /// - Whether figure is in position to shoot/attract
    /// 
    /// NEW METHOD - Fixes the bug where closest figure is stuck at movement limit
    /// </summary>
    public GameObject GetBestAvailableFigure()
    {
        if (ball == null || figures.Count == 0)
            return figures.Count > 0 ? figures[0] : null;

        Vector2 ballPosition = ball.transform.position;
        float rodY = transform.position.y;

        // Create list of candidates with scores
        List<FigureCandidate> candidates = new List<FigureCandidate>();

        for (int i = 0; i < figures.Count; i++)
        {
            GameObject figure = figures[i];
            if (figure == null) continue;

            Vector2 figurePos = figure.transform.position;
            float distanceToBall = Vector2.Distance(figurePos, ballPosition);

            // Calculate if figure can reach ball (not at movement limit)
            float figureY = figurePos.y;
            float ballY = ballPosition.y;
            float yDistanceToBall = Mathf.Abs(ballY - figureY);

            // Calculate how much the rod would need to move to align figure with ball
            float requiredRodMovement = ballY - figureY;
            float newRodY = rodY + requiredRodMovement;

            // Check if rod can move to required position (within movement limits)
            float minY = -rodConfig.rodMovementLimit + rodConfig.halfPlayer;
            float maxY = rodConfig.rodMovementLimit - rodConfig.halfPlayer;

            bool canReachBall = newRodY >= minY && newRodY <= maxY;

            // Calculate distance from movement limit (prefer figures not at edges)
            float distanceFromLimit = 0f;
            if (canReachBall)
            {
                float distanceFromMin = newRodY - minY;
                float distanceFromMax = maxY - newRodY;
                distanceFromLimit = Mathf.Min(distanceFromMin, distanceFromMax);
            }

            // Calculate score
            float score = CalculateFigureScore(distanceToBall, canReachBall, distanceFromLimit, yDistanceToBall);

            candidates.Add(new FigureCandidate
            {
                figure = figure,
                index = i,
                distanceToBall = distanceToBall,
                canReachBall = canReachBall,
                distanceFromLimit = distanceFromLimit,
                score = score
            });
        }

        // Sort by score (highest first)
        candidates.Sort((a, b) => b.score.CompareTo(a.score));

        // Return best candidate
        if (candidates.Count > 0 && candidates[0].score > 0)
        {
            return candidates[0].figure;
        }

        // Fallback to first figure
        return figures[0];
    }

    /// <summary>
    /// Calculates score for figure candidate
    /// Higher score = better candidate
    /// </summary>
    private float CalculateFigureScore(float distanceToBall, bool canReachBall, float distanceFromLimit, float yDistanceToBall)
    {
        if (!canReachBall)
        {
            // Figure cannot reach ball - very low score
            // But not zero, in case all figures are stuck
            return 0.1f / (distanceToBall + 1f);
        }

        // Start with base score from distance (closer = better)
        float distanceScore = 1f / (distanceToBall + 0.1f); // +0.1 to avoid division by zero

        // Add bonus for being further from movement limits (more room to maneuver)
        float limitBonus = distanceFromLimit * 0.5f;

        // Add bonus for being vertically aligned with ball
        float alignmentBonus = 1f / (yDistanceToBall + 0.1f);

        // Combine scores
        float totalScore = distanceScore + limitBonus + alignmentBonus;

        return totalScore;
    }

    /// <summary>
    /// Helper struct for figure candidate evaluation
    /// </summary>
    private struct FigureCandidate
    {
        public GameObject figure;
        public int index;
        public float distanceToBall;
        public bool canReachBall;
        public float distanceFromLimit;
        public float score;
    }

    void RodConfigurationSpeed(int numberOfFigureInRod)
    {
        // Use FormationPreset speed if active, otherwise fall back to hardcoded defaults
        if (rodConfig != null && rodConfig.activeFormationPreset != null)
        {
            speed = rodConfig.activeFormationPreset.GetAISpeed(numberOfFigureInRod);
        }
        else
        {
            switch (numberOfFigureInRod)
            {
                case 1:
                    speed = 3f;
                    break;
                case 2:
                    speed = 2.5f;
                    break;
                case 3:
                    speed = 2f;
                    break;
                case 4:
                    speed = 1.5f;
                    break;
                case 5:
                    speed = 1f;
                    break;
            }
        }

        if (MatchInfo.instance != null)
        {
            if (MatchInfo.instance.matchLevel == 1) speed -= 0.5f;
            if (MatchInfo.instance.matchLevel == 3) speed += 0.5f;
        }
    }

    #endregion

    #region Public API for FSM

    /// <summary>
    /// Sets the movement mode (called by FSM states)
    /// </summary>
    public void SetMovementMode(MovementMode mode)
    {
        // When entering BumpNudge, save current mode to restore after
        if (mode == MovementMode.BumpNudge && currentMode != MovementMode.BumpNudge)
        {
            preBumpMode = currentMode;
            bumpPhase = 0;
            bumpTimer = 0f;
            bumpTargetY = ball != null ? ball.transform.position.y : transform.position.y;
        }
        currentMode = mode;
    }

    /// <summary>
    /// Gets current movement mode
    /// </summary>
    public MovementMode GetMovementMode()
    {
        return currentMode;
    }

    /// <summary>
    /// Gets predicted ball position
    /// </summary>
    public Vector2 GetPredictedBallPosition()
    {
        return predictedBallPosition;
    }

    /// <summary>
    /// Sets positioning accuracy (1=perfect, lower=overshoot). Called by AITeamRodsController.
    /// </summary>
    public void SetPositioningAccuracy(float accuracy)
    {
        positioningAccuracy = Mathf.Clamp(accuracy, 0.3f, 1f);
    }

    /// <summary>
    /// Sets movement speed multiplier (1=full speed). Called by AITeamRodsController.
    /// </summary>
    public void SetMovementSpeedMultiplier(float multiplier)
    {
        movementSpeedMultiplier = Mathf.Clamp(multiplier, 0.3f, 1.5f);
    }

    #endregion

    #region Debug

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!isActive || ball == null) return;

        // Draw predicted ball position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(predictedBallPosition, 0.3f);
        Gizmos.DrawLine(ball.transform.position, predictedBallPosition);

        // Draw movement mode indicator
        Vector3 labelPos = transform.position + Vector3.up * 3f;
        UnityEditor.Handles.Label(labelPos, $"Mode: {currentMode}");
    }
#endif

    #endregion
}

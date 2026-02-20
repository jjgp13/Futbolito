using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// AI Goal Evaluator - Analyzes shooting opportunities and paths to goal
/// 
/// PURPOSE:
/// This class provides strategic analysis for AI decision-making focused on scoring goals.
/// Evaluates the best way to score based on:
/// - Shot path analysis (is there a clear path to goal?)
/// - Opponent figure detection (which figures are blocking?)
/// - Angle calculations (can we shoot around blockers?)
/// - Passing opportunities (is a teammate in a better position?)
/// 
/// GOAL-ORIENTED DECISION MAKING:
/// The AI's primary objective is to score goals. This class helps determine:
/// 1. Can we score directly from this position?
/// 2. If blocked, can we shoot at an angle to bypass defenders?
/// 3. Should we pass to a better-positioned rod instead?
/// 4. Should we use wall pass to create confusion and open space?
/// 
/// PLAYER vs AI CONTROL:
/// - Automatically disables when team is player-controlled
/// - Checks for TeamRodsController (player) vs AITeamRodsController (AI)
/// - No AI calculations run when player is in control
/// - Seamlessly switches based on team assignment in SetMatchController
/// </summary>
public class AIGoalEvaluator : MonoBehaviour
{
    #region Configuration

    [Header("Goal Analysis Settings")]
    [Tooltip("Distance to raycast when checking for blockers")]
    [SerializeField] private float raycastDistance = 20f;

    [Tooltip("Minimum clear width required for a direct shot")]
    [SerializeField] private float minimumClearWidth = 1.5f;

    [Tooltip("Maximum angle deviation for angled shots (degrees)")]
    [SerializeField] private float maxShootAngle = 45f;

    [Tooltip("Number of angle tests to perform when checking angled shots")]
    [SerializeField] private int angleTestCount = 5;

    [Header("Pass Evaluation Settings")]
    [Tooltip("Maximum distance to consider passing to another rod")]
    [SerializeField] private float maxPassDistance = 10f;

    [Tooltip("How much better positioned must another rod be to consider passing")]
    [SerializeField] private float passingAdvantageThreshold = 0.3f;

    [Header("Decision Thresholds")]
    [Tooltip("Minimum shooting score required to recommend shooting (0-1)")]
    [SerializeField] private float minimumShootScore = 0.4f;

    #endregion

    #region Cached References

    private AITeamRodsController teamController;
    private TeamSide teamSide;
    private Transform opponentGoalTransform;
    private Vector2 goalPosition;
    private float goalWidth;

    #endregion

    #region Properties

    /// <summary>
    /// Returns the direction vector toward the opponent's goal
    /// </summary>
    public Vector2 GoalDirection
    {
        get
        {
            return teamSide == TeamSide.LeftTeam ? Vector2.right : Vector2.left;
        }
    }

    /// <summary>
    /// Returns the X position of the opponent's goal
    /// </summary>
    public float GoalXPosition => goalPosition.x;

    #endregion

    #region Initialization

    private void Awake()
    {
        teamController = GetComponentInParent<AITeamRodsController>();
        
        if (teamController == null)
        {
            Debug.LogError($"[AIGoalEvaluator] No AITeamRodsController found on {gameObject.name}! This component requires AITeamRodsController.");
        }
    }

    private void OnEnable()
    {
        // Only initialize when enabled by SetMatchController
        // No self-check logic - we trust the manager to enable us correctly
        if (teamController != null)
        {
            teamSide = teamController.teamSide;
            Debug.Log($"[AIGoalEvaluator] ✅ Initialized for AI control on {teamSide} team");
            CacheGoalInformation();
        }
        else
        {
            Debug.LogError($"[AIGoalEvaluator] Cannot initialize - No AITeamRodsController found!");
            enabled = false;
        }
    }

    /// <summary>
    /// Caches goal position and dimensions for quick access
    /// </summary>
    private void CacheGoalInformation()
    {
        // Find goal based on team side
        string goalTag = teamSide == TeamSide.LeftTeam ? "RightGoalTrigger" : "LeftGoalTrigger";
        GameObject goalObject = GameObject.FindGameObjectWithTag(goalTag);

        if (goalObject != null)
        {
            opponentGoalTransform = goalObject.transform;
            goalPosition = opponentGoalTransform.position;

            // Debug verification
            Debug.Log($"[AIGoalEvaluator] Goal Cached Successfully:\n" +
                $"  Team: {teamSide}\n" +
                $"  Looking for tag: '{goalTag}'\n" +
                $"  Found goal: '{goalObject.name}'\n" +
                $"  Goal Position: {goalPosition}\n" +
                $"  Expected: {(teamSide == TeamSide.LeftTeam ? "Right side (positive X)" : "Left side (negative X)")}\n" +
                $"  Actual X: {goalPosition.x:F2} {(teamSide == TeamSide.LeftTeam ? (goalPosition.x > 0 ? "✅" : "❌ WRONG!") : (goalPosition.x < 0 ? "✅" : "❌ WRONG!"))}");

            // Get goal width from collider
            BoxCollider2D goalCollider = goalObject.GetComponent<BoxCollider2D>();
            if (goalCollider != null)
            {
                goalWidth = goalCollider.size.y; // Height in 2D is vertical goal opening
                Debug.Log($"  Goal Width: {goalWidth}");
            }
            else
            {
                goalWidth = 3f; // Default fallback
                Debug.LogWarning($"  Goal Width: {goalWidth} (using default - no collider found!)");
            }
        }
        else
        {
            Debug.LogError($"[AIGoalEvaluator] ❌ CRITICAL: Could not find goal with tag '{goalTag}' for team {teamSide}!\n" +
                $"  Make sure goals have correct tags:\n" +
                $"  - LeftGoalTrigger for left goal\n" +
                $"  - RightGoalTrigger for right goal");
        }
    }

    #endregion

    #region Shot Path Analysis

    /// <summary>
    /// Evaluates if there's a clear direct shot path from figure to goal
    /// 
    /// ALGORITHM:
    /// 1. Cast a ray from figure toward goal center
    /// 2. Check if ray hits any opponent figures
    /// 3. Calculate clear width around blocking figures
    /// 4. Determine if gap is wide enough for ball to pass through
    /// 
    /// RETURNS:
    /// ShootOpportunity with:
    /// - isDirectShotClear: True if no blockers or gap is wide enough
    /// - blockingFigures: List of figures in the way
    /// - clearPathWidth: Estimated width of clear path
    /// </summary>
    public ShootOpportunity EvaluateDirectShot(Transform figureTransform)
    {
        Vector2 figurePos = figureTransform.position;
        Vector2 directionToGoal = (goalPosition - figurePos).normalized;

        // Raycast to check for blockers
        RaycastHit2D[] hits = Physics2D.RaycastAll(
             figurePos,
        directionToGoal,
         raycastDistance,
           LayerMask.GetMask("figures") // Assuming figures are on "figures" layer
                );

        List<Transform> blockers = new List<Transform>();

        // Collect opponent figures blocking the path
        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.transform != figureTransform)
            {
                // Check if it's an opponent figure
                if (IsOpponentFigure(hit.collider.gameObject))
                {
                    blockers.Add(hit.collider.transform);
                }
            }
        }

        // Calculate clear path width
        float clearWidth = CalculateClearPathWidth(figurePos, directionToGoal, blockers);

        // Determine if shot is clear
        bool isDirectShotClear = blockers.Count == 0 || clearWidth >= minimumClearWidth;

        ShootOpportunity opportunity = new ShootOpportunity
        {
            figurePosition = figurePos,
            isDirectShotClear = isDirectShotClear,
            blockingFigures = blockers,
            clearPathWidth = clearWidth,
            recommendedAngle = 0f,
            shootingScore = CalculateShootingScore(isDirectShotClear, clearWidth, Vector2.Distance(figurePos, goalPosition))
        };

#if UNITY_EDITOR
        UpdateDebugData(figureTransform, opportunity);
#endif

        return opportunity;
    }

    /// <summary>
    /// Evaluates angled shot opportunities to bypass blockers
    /// 
    /// ALGORITHM:
    /// 1. Test multiple angles from -maxShootAngle to +maxShootAngle
    /// 2. For each angle, check if path is clearer than direct shot
    /// 3. Consider ball physics (will it reach goal at this angle?)
    /// 4. Return best angle with highest clear path width
    /// 
    /// USE CASE:
    /// When direct shot is blocked, AI can shoot at an angle to curve around defenders
    /// </summary>
    public ShootOpportunity EvaluateAngledShot(Transform figureTransform)
    {
        ShootOpportunity bestOpportunity = EvaluateDirectShot(figureTransform);

        // If direct shot is clear, no need for angled shot
        if (bestOpportunity.isDirectShotClear)
        {
#if UNITY_EDITOR
            UpdateDebugData(figureTransform, bestOpportunity);
#endif
            return bestOpportunity;
        }

        Vector2 figurePos = figureTransform.position;
        float bestScore = bestOpportunity.shootingScore;

        // Test different angles
        for (int i = 0; i < angleTestCount; i++)
        {
            // Calculate test angle (split between positive and negative)
            float angleStep = maxShootAngle / (angleTestCount / 2f);
            float testAngle = (i % 2 == 0 ? 1 : -1) * angleStep * ((i + 1) / 2);

            // Calculate direction at this angle
            Vector2 baseDirection = (goalPosition - figurePos).normalized;
            Vector2 angledDirection = RotateVector(baseDirection, testAngle);

            // Check if this angle provides a clearer path
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                figurePos,
                angledDirection,
                raycastDistance,
                LayerMask.GetMask("figures")
           );

            List<Transform> angledBlockers = new List<Transform>();
            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider.transform != figureTransform)
                {
                    if (IsOpponentFigure(hit.collider.gameObject))
                    {
                        angledBlockers.Add(hit.collider.transform);
                    }
                }
            }

            float angledClearWidth = CalculateClearPathWidth(figurePos, angledDirection, angledBlockers);
            bool isAngledShotClear = angledBlockers.Count == 0 || angledClearWidth >= minimumClearWidth;

            // Calculate score for this angle (penalize extreme angles)
            float anglePenalty = Mathf.Abs(testAngle) / maxShootAngle;
            float score = CalculateShootingScore(isAngledShotClear, angledClearWidth, Vector2.Distance(figurePos, goalPosition));
            score *= (1f - anglePenalty * 0.3f); // Up to 30% penalty for extreme angles

            // Update best opportunity if this angle is better
            if (score > bestScore)
            {
                bestScore = score;
                bestOpportunity = new ShootOpportunity
                {
                    figurePosition = figurePos,
                    isDirectShotClear = isAngledShotClear,
                    blockingFigures = angledBlockers,
                    clearPathWidth = angledClearWidth,
                    recommendedAngle = testAngle,
                    shootingScore = score
                };
            }
        }

#if UNITY_EDITOR
        UpdateDebugData(figureTransform, bestOpportunity);
#endif

        return bestOpportunity;
    }

    /// <summary>
    /// Calculates estimated clear path width considering blockers
    /// 
    /// ALGORITHM:
    /// 1. Find closest blocker
    /// 2. Calculate perpendicular distance from blocker to shot line
    /// 3. Estimate available space on either side of blocker
    /// 4. Return minimum gap width ball could pass through
    /// </summary>
    private float CalculateClearPathWidth(Vector2 origin, Vector2 direction, List<Transform> blockers)
    {
        if (blockers.Count == 0)
        {
            return goalWidth; // Entire goal is clear
        }

        // Find closest blocker
        Transform closestBlocker = blockers.OrderBy(b => Vector2.Distance(origin, b.position)).First();
        Vector2 blockerPos = closestBlocker.position;

        // Calculate perpendicular distance from blocker to shot line
        Vector2 originToBlocker = blockerPos - origin;
        float projectionLength = Vector2.Dot(originToBlocker, direction);
        Vector2 projectionPoint = origin + direction * projectionLength;
        float perpendicularDistance = Vector2.Distance(blockerPos, projectionPoint);

        // Get blocker's width (approximate from collider)
        float blockerWidth = 0.5f; // Default
        Collider2D blockerCollider = closestBlocker.GetComponent<Collider2D>();
        if (blockerCollider != null)
        {
            blockerWidth = blockerCollider.bounds.size.x;
        }

        // Calculate available space around blocker
        // This is a simplified calculation - assumes blocker is centered on shot line
        float clearWidth = Mathf.Max(0, perpendicularDistance * 2f - blockerWidth);

        return clearWidth;
    }

    /// <summary>
    /// Calculates a shooting quality score (0-1)
    /// 
    /// FACTORS:
    /// - Is path clear? (major factor)
    /// - How wide is the clear path? (wider = better)
    /// - How far from goal? (closer = better)
    /// </summary>
    private float CalculateShootingScore(bool isClear, float clearWidth, float distanceToGoal)
    {
        float score = 0f;

        // Base score from clear path
        if (isClear)
        {
            score += 0.5f;
        }

        // Score from clear width (normalized to 0-0.3)
        float widthScore = Mathf.Clamp01(clearWidth / goalWidth) * 0.3f;
        score += widthScore;

        // Score from distance (closer is better, normalized to 0-0.2)
        float maxDistance = 15f; // Reasonable maximum shooting distance
        float distanceScore = Mathf.Clamp01(1f - (distanceToGoal / maxDistance)) * 0.2f;
        score += distanceScore;

        return Mathf.Clamp01(score);
    }

    #endregion

    #region Passing Analysis

    /// <summary>
    /// Evaluates if passing to another rod would create a better scoring opportunity
    /// 
    /// STRATEGY:
    /// 1. Check other AI rods on the same team
    /// 2. Calculate their shooting score if they had the ball
    /// 3. Compare with current rod's shooting score
    /// 4. If another rod has significantly better position, recommend passing
    /// 
    /// RETURNS:
    /// PassOpportunity with:
    /// - shouldPass: True if passing is strategically better
    /// - targetRod: Which rod to pass to
    /// - passScore: How much better the pass target's position is
    /// </summary>
    public PassOpportunity EvaluatePassOpportunity(Transform currentFigure, Vector2 ballPosition)
    {
        if (teamController == null)
        {
            return new PassOpportunity { shouldPass = false };
        }

        // Get current rod's shooting score
        ShootOpportunity currentShot = EvaluateAngledShot(currentFigure);
        float currentScore = currentShot.shootingScore;

        // Check other rods
        GameObject[] rods = teamController.rods;
        float bestPassScore = 0f;
        int bestRodIndex = -1;

        for (int i = 0; i < rods.Length; i++)
        {
            if (rods[i] == null || rods[i] == currentFigure.parent.gameObject)
                continue;

            // Check if this rod is closer to opponent goal (forward pass only)
            float rodX = rods[i].transform.position.x;
            float currentX = currentFigure.position.x;

            bool isForwardPass = teamSide == TeamSide.LeftTeam ? rodX > currentX : rodX < currentX;
            if (!isForwardPass)
                continue;

            // Calculate distance from ball to target rod
            float passDistance = Mathf.Abs(rodX - ballPosition.x);
            if (passDistance > maxPassDistance)
                continue;

            // Find closest figure on target rod to ball's Y position
            Transform closestFigure = FindClosestFigureOnRod(rods[i], ballPosition.y);
            if (closestFigure == null)
                continue;

            // Evaluate shooting opportunity from that figure's position
            ShootOpportunity targetShot = EvaluateAngledShot(closestFigure);

            // Calculate pass score (how much better is target position?)
            float passAdvantage = targetShot.shootingScore - currentScore;
            if (passAdvantage > passingAdvantageThreshold)
            {
                // Factor in pass difficulty (longer passes are riskier)
                float passRiskPenalty = (passDistance / maxPassDistance) * 0.2f;
                float passScore = passAdvantage - passRiskPenalty;

                if (passScore > bestPassScore)
                {
                    bestPassScore = passScore;
                    bestRodIndex = i;
                }
            }
        }

        bool shouldPass = bestRodIndex >= 0 && bestPassScore > 0;

        return new PassOpportunity
        {
            shouldPass = shouldPass,
            targetRodIndex = bestRodIndex,
            targetRod = bestRodIndex >= 0 ? rods[bestRodIndex] : null,
            passScore = bestPassScore,
            currentScore = currentScore
        };
    }

    /// <summary>
    /// Finds the figure on a rod closest to a given Y position
    /// </summary>
    private Transform FindClosestFigureOnRod(GameObject rod, float targetY)
    {
        Transform closestFigure = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < rod.transform.childCount; i++)
        {
            Transform child = rod.transform.GetChild(i);
            float distance = Mathf.Abs(child.position.y - targetY);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestFigure = child;
            }
        }

        return closestFigure;
    }

    #endregion

    #region Wall Pass Analysis

    /// <summary>
    /// Evaluates if wall pass should be used to create scoring opportunities
    /// 
    /// WALL PASS STRATEGY:
    /// Use wall pass when:
    /// 1. Direct and angled shots are BLOCKED (low shooting score)
    /// 2. Multiple blockers present (path heavily contested)
    /// 3. Figure can physically perform wall pass (ball in trigger collider)
    /// 
    /// PURPOSE:
    /// - Create confusion and unpredictable ball movement
    /// - Force opponent to react, potentially opening shooting lanes
    /// - Bounce ball off wall to create new position for teammate rod
    /// 
    /// RETURNS:
    /// True if wall pass is recommended, false otherwise
    /// </summary>
    public bool ShouldUseWallPass(Transform currentFigure)
    {
        if (currentFigure == null) return false;

        // Evaluate current shooting opportunity
        ShootOpportunity shootOpp = EvaluateAngledShot(currentFigure);

        // Wall pass when shot quality is poor (not just when heavily blocked)
        // A shot score below 0.5 means the angle isn't great — wall pass can create a better one
        bool shotSuboptimal = shootOpp.shootingScore < 0.5f;

        if (!shotSuboptimal)
        {
            // Shot is good enough, no need for wall pass
            return false;
        }

        // If direct shot is blocked OR we have any blockers, recommend wall pass
        bool hasBlockers = shootOpp.blockingFigures != null && shootOpp.blockingFigures.Count >= 1;
        bool shotBlocked = !shootOpp.isDirectShotClear;

        if (shotBlocked || hasBlockers)
        {
            return true;
        }

        return false;
    }

    #endregion

    #region Opponent Gap Analysis (Change 5)

    /// <summary>
    /// Finds gaps between opponent figures that can be targeted for shots or passes.
    /// Returns the Y position of the largest gap in front of this rod toward the goal.
    /// </summary>
    public float FindBestOpponentGap(Transform figureTransform, out float gapWidth)
    {
        gapWidth = 0f;
        if (figureTransform == null) return 0f;

        Vector2 figurePos = figureTransform.position;
        Vector2 goalDir = GoalDirection;

        // Collect Y positions of opponent figures between this rod and the goal
        List<float> opponentYPositions = new List<float>();

        GameObject[] allFigures = GameObject.FindGameObjectsWithTag("RodFigure");
        foreach (GameObject fig in allFigures)
        {
            if (!IsOpponentFigure(fig)) continue;

            float figX = fig.transform.position.x;

            // Only consider figures between this rod and the goal
            bool isBetween = teamSide == TeamSide.LeftTeam
                ? figX > figurePos.x
                : figX < figurePos.x;

            if (isBetween)
            {
                opponentYPositions.Add(fig.transform.position.y);
            }
        }

        if (opponentYPositions.Count == 0)
        {
            // No opponents ahead — shoot center
            gapWidth = float.MaxValue;
            return 0f;
        }

        // Sort Y positions to find gaps between figures
        opponentYPositions.Sort();

        float bestGapCenter = 0f;
        float bestGapWidth = 0f;

        // Check gap below the lowest figure (to bottom wall)
        float bottomWall = -5f; // approximate playfield boundary
        float bottomGap = opponentYPositions[0] - bottomWall;
        if (bottomGap > bestGapWidth)
        {
            bestGapWidth = bottomGap;
            bestGapCenter = bottomWall + bottomGap / 2f;
        }

        // Check gaps between consecutive figures
        for (int i = 0; i < opponentYPositions.Count - 1; i++)
        {
            float gap = opponentYPositions[i + 1] - opponentYPositions[i];
            if (gap > bestGapWidth)
            {
                bestGapWidth = gap;
                bestGapCenter = opponentYPositions[i] + gap / 2f;
            }
        }

        // Check gap above the highest figure (to top wall)
        float topWall = 5f; // approximate playfield boundary
        float topGap = topWall - opponentYPositions[opponentYPositions.Count - 1];
        if (topGap > bestGapWidth)
        {
            bestGapWidth = topGap;
            bestGapCenter = opponentYPositions[opponentYPositions.Count - 1] + topGap / 2f;
        }

        gapWidth = bestGapWidth;
        return bestGapCenter;
    }

    #endregion

    #region Goalkeeper Clearing Analysis

    /// <summary>
    /// Finds the best Y position to aim a GK clear toward the defense rod.
    /// Returns the Y position of the best-positioned defense figure, or 0 (center) as fallback.
    /// 
    /// STRATEGY:
    /// 1. Find the defense rod (rod index 1) in the AI team
    /// 2. Find which defense figure is best positioned to receive the clear
    /// 3. Prefer figures that are not at movement limits (more room to control)
    /// 4. Return that figure's Y position as the clearing target
    /// </summary>
    public float FindBestClearingTarget(out bool hasTarget)
    {
        hasTarget = false;
        if (teamController == null) return 0f;

        GameObject[] rods = teamController.rods;
        if (rods.Length < 2 || rods[1] == null) return 0f;

        // Defense rod is index 1
        GameObject defenseRod = rods[1];
        float bestY = 0f;
        float bestScore = -1f;

        for (int i = 0; i < defenseRod.transform.childCount; i++)
        {
            Transform figure = defenseRod.transform.GetChild(i);
            float figureY = figure.position.y;

            // Score: prefer figures closer to center (more room to maneuver)
            float centerScore = 1f - Mathf.Abs(figureY) / 5f;
            // Slight bonus for being on the same side as the ball
            GameObject ball = GameObject.FindGameObjectWithTag("Ball");
            float sideBias = 0f;
            if (ball != null)
            {
                sideBias = (Mathf.Sign(ball.transform.position.y) == Mathf.Sign(figureY)) ? 0.2f : 0f;
            }

            float score = centerScore + sideBias;
            if (score > bestScore)
            {
                bestScore = score;
                bestY = figureY;
                hasTarget = true;
            }
        }

        if (hasTarget)
        {
            AIDebugLogger.Log("GK", "GK_CLEAR_TARGET", $"Clearing toward defense figure at Y={bestY:F2}");
        }

        return bestY;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Checks if a GameObject belongs to the opponent team
    /// </summary>
    private bool IsOpponentFigure(GameObject figure)
    {
        // Check parent hierarchy for team controller
        Transform parent = figure.transform;
        while (parent != null)
        {
            TeamRodsController controller = parent.GetComponent<TeamRodsController>();
            if (controller != null)
            {
                return controller.teamSide != teamSide;
            }
            parent = parent.parent;
        }

        return false; // If we can't determine, assume not opponent
    }

    /// <summary>
    /// Rotates a 2D vector by given angle in degrees
    /// </summary>
    private Vector2 RotateVector(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(
       vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }

    #endregion

    #region Debug Visualization

    [Header("Debug Visualization")]
    [Tooltip("Enable debug visualization in Scene view")]
    [SerializeField] private bool enableDebugVisualization = true;

    [Tooltip("Show shot path raycasts")]
    [SerializeField] private bool showShotPaths = true;

    [Tooltip("Show blocking figures")]
    [SerializeField] private bool showBlockingFigures = true;

    [Tooltip("Show shooting scores as text")]
    [SerializeField] private bool showShootingScores = true;

    [Tooltip("Show angled shot tests")]
    [SerializeField] private bool showAngledShotTests = true;

    [Tooltip("Show clear path width calculations")]
    [SerializeField] private bool showClearPathWidth = true;

    [Tooltip("Which rod to debug (leave null for all active rods)")]
    [SerializeField] private GameObject debugSpecificRod = null;

    // Cached debug data from last evaluation
    private ShootOpportunity lastEvaluatedShot;
    private Transform lastEvaluatedFigure;
    private bool hasDebugData = false;

#if UNITY_EDITOR
    /// <summary>
    /// Visualizes shot paths and blockers in Scene view
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!enableDebugVisualization || !Application.isPlaying)
            return;

        // Draw goal position
        if (opponentGoalTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(goalPosition, 0.3f);

            // Draw goal width indicator
            if (showClearPathWidth)
            {
                Gizmos.color = Color.green;
                Vector3 topGoal = new Vector3(goalPosition.x, goalPosition.y + goalWidth / 2f, 0);
                Vector3 bottomGoal = new Vector3(goalPosition.x, goalPosition.y - goalWidth / 2f, 0);
                Gizmos.DrawLine(topGoal, bottomGoal);
            }
        }

        // Draw last evaluated shot data
        if (hasDebugData && lastEvaluatedFigure != null)
        {
            DrawShotDebugInfo(lastEvaluatedFigure, lastEvaluatedShot);
        }
    }

    /// <summary>
    /// Draws detailed debug information for a shot evaluation
    /// </summary>
    private void DrawShotDebugInfo(Transform figure, ShootOpportunity shot)
    {
        Vector3 figurePos = figure.position;

        // Draw shot path to goal
        if (showShotPaths)
        {
            // Direct shot path
            Gizmos.color = shot.isDirectShotClear ? Color.green : Color.red;
            Gizmos.DrawLine(figurePos, goalPosition);

            // Draw recommended angle shot if different from direct
            if (Mathf.Abs(shot.recommendedAngle) > 0.1f)
            {
                Vector2 baseDirection = (goalPosition - (Vector2)figurePos).normalized;
                Vector2 angledDirection = RotateVector(baseDirection, shot.recommendedAngle);
                Vector3 angledTarget = figurePos + (Vector3)angledDirection * raycastDistance;

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(figurePos, angledTarget);
            }
        }

        // Draw blocking figures
        if (showBlockingFigures && shot.blockingFigures != null)
        {
            foreach (Transform blocker in shot.blockingFigures)
            {
                if (blocker != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(blocker.position, 0.3f);
                    Gizmos.DrawLine(figurePos, blocker.position);
                }
            }
        }

        // Draw clear path width visualization
        if (showClearPathWidth && shot.blockingFigures != null && shot.blockingFigures.Count > 0)
        {
            Transform closestBlocker = shot.blockingFigures.OrderBy(b => Vector2.Distance(figurePos, b.position)).First();
            Vector2 directionToGoal = (goalPosition - (Vector2)figurePos).normalized;

            // Calculate perpendicular line showing clear width
            Vector2 perpendicular = new Vector2(-directionToGoal.y, directionToGoal.x);
            Vector3 blockerPos = closestBlocker.position;

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.7f); // Orange
            float halfClearWidth = shot.clearPathWidth / 2f;
            Vector3 clearStart = blockerPos + (Vector3)(perpendicular * halfClearWidth);
            Vector3 clearEnd = blockerPos - (Vector3)(perpendicular * halfClearWidth);
            Gizmos.DrawLine(clearStart, clearEnd);
        }

        // Draw shooting score as GUI text
        if (showShootingScores)
        {
            UnityEditor.Handles.color = shot.shootingScore >= 0.4f ? Color.green : Color.red;
            string scoreText = $"Score: {shot.shootingScore:F2}\n" +
                $"Clear: {shot.isDirectShotClear}\n" +
                $"Width: {shot.clearPathWidth:F2}\n" +
                $"Angle: {shot.recommendedAngle:F1}°\n" +
                $"Blockers: {(shot.blockingFigures?.Count ?? 0)}";

            Vector3 labelPos = figurePos + Vector3.up * 1.5f;
            UnityEditor.Handles.Label(labelPos, scoreText);
        }

        // Draw angled shot tests
        if (showAngledShotTests)
        {
            DrawAngledShotTests(figure);
        }
    }

    /// <summary>
    /// Visualizes all angle test rays
    /// </summary>
    private void DrawAngledShotTests(Transform figure)
    {
        Vector2 figurePos = figure.position;
        Vector2 baseDirection = (goalPosition - figurePos).normalized;

        for (int i = 0; i < angleTestCount; i++)
        {
            float angleStep = maxShootAngle / (angleTestCount / 2f);
            float testAngle = (i % 2 == 0 ? 1 : -1) * angleStep * ((i + 1) / 2);

            Vector2 angledDirection = RotateVector(baseDirection, testAngle);
            Vector3 endPoint = figurePos + angledDirection * raycastDistance;

            // Color based on angle magnitude
            float t = Mathf.Abs(testAngle) / maxShootAngle;
            Gizmos.color = new Color(1f - t, t, 0f, 0.3f); // Yellow to red gradient
            Gizmos.DrawLine(figurePos, endPoint);
        }
    }

    /// <summary>
    /// Call this method to update debug data when evaluating a shot
    /// This allows the Gizmos to show the most recent evaluation
    /// </summary>
    private void UpdateDebugData(Transform figure, ShootOpportunity opportunity)
    {
        if (!enableDebugVisualization)
            return;

        lastEvaluatedFigure = figure;
        lastEvaluatedShot = opportunity;
        hasDebugData = true;

        // Log detailed information to console
        if (showShootingScores)
        {
            string rodName = figure.parent != null ? figure.parent.name : "Unknown Rod";

            // Enhanced debug: Show direction calculations
            Vector2 figurePos = figure.position;
            Vector2 directionToGoal = (goalPosition - figurePos).normalized;
            Vector2 expectedDirection = teamSide == TeamSide.LeftTeam ? Vector2.right : Vector2.left;

            // Calculate if direction is correct
            float directionDot = Vector2.Dot(directionToGoal, expectedDirection);
            bool directionCorrect = directionDot > 0.5f; // Should be close to 1.0 if correct

            Debug.Log($"[AIGoalEvaluator] {rodName} - Shot Evaluation:\n" +
          $"  Team Side: {teamSide}\n" +
          $"  Figure Position: {figurePos}\n" +
          $"  Goal Position: {goalPosition}\n" +
          $"  Direction to Goal: {directionToGoal}\n" +
          $"  Expected Direction: {expectedDirection} ({(teamSide == TeamSide.LeftTeam ? "RIGHT ?" : "LEFT ?")})\n" +
          $"  Direction Match: {(directionCorrect ? "? CORRECT" : "? WRONG")} (dot: {directionDot:F2})\n" +
          $"  Shooting Score: {opportunity.shootingScore:F3}\n" +
          $"  Direct Shot Clear: {opportunity.isDirectShotClear}\n" +
          $"  Clear Path Width: {opportunity.clearPathWidth:F2} (min required: {minimumClearWidth:F2})\n" +
          $"  Recommended Angle: {opportunity.recommendedAngle:F1}°\n" +
          $"  Blocking Figures: {(opportunity.blockingFigures?.Count ?? 0)}\n" +
          $"  Distance to Goal: {Vector2.Distance(opportunity.figurePosition, goalPosition):F2}\n" +
          $"  Should Shoot: {(opportunity.shootingScore >= minimumShootScore ? "YES" : "NO")}");

            // Log blocker details
            if (opportunity.blockingFigures != null && opportunity.blockingFigures.Count > 0)
            {
                Debug.Log($"  Blockers:");
                foreach (Transform blocker in opportunity.blockingFigures)
                {
                    if (blocker != null)
                    {
                        string blockerRod = blocker.parent != null ? blocker.parent.name : "Unknown";
                        Debug.Log($"    - {blocker.name} on {blockerRod} at {blocker.position}");
                    }
                }
            }
        }
    }
#endif

    #endregion
}

#region Data Structures

/// <summary>
/// Contains information about a shooting opportunity
/// </summary>
[System.Serializable]
public struct ShootOpportunity
{
    public Vector2 figurePosition;
    public bool isDirectShotClear;
    public List<Transform> blockingFigures;
    public float clearPathWidth;
    public float recommendedAngle;
    public float shootingScore; // 0-1, higher is better
}

/// <summary>
/// Contains information about a passing opportunity
/// </summary>
[System.Serializable]
public struct PassOpportunity
{
    public bool shouldPass;
    public int targetRodIndex;
    public GameObject targetRod;
    public float passScore;
    public float currentScore;
}

#endregion

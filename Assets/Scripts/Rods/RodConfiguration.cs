using UnityEngine;

public class RodConfiguration : MonoBehaviour
{
    [Header("References to Foosball Figure Prefab")]
    public GameObject FoosballFigurePrefab;

    [Header("Line Configuration")]
    public int rodFoosballFigureCount;
    public float rodMovementLimit;
    public float halfPlayer;

    [Header("Rod Role")]
    public RodRole rodRole;

    private GameObject foosballFigureGameObject;
    private Team teamInfo;
    private string teamUniform;
    private Formation teamFormation;
    private float screenHalfWidthInWorldUnits;

    /// <summary>
    /// When set, overrides team formation with formation preset values.
    /// Also provides speed values to movement scripts via GetActiveFormationPreset().
    /// </summary>
    [HideInInspector] public FormationPreset activeFormationPreset;

    void Awake()
    {
        foosballFigureGameObject = FoosballFigurePrefab;
        rodRole = RodRoleExtensions.FromRodName(gameObject.name);

        // Override formation from active FormationPreset if set
        if (FormationPreset.Active != null)
        {
            activeFormationPreset = FormationPreset.Active;
        }

        // Initialize based on game state
        if (AutoMatchRunner.IsAutoMode)
        {
            RodConfigurationFromTeamScriptableObject();
        }
        else if (MatchInfo.instance != null)
        {
            RodConfigurationFromMatchInfoObject();
        }
        else
        {
            RodConfigurationFromTeamScriptableObject();
        }
    }

    private void RodConfigurationFromMatchInfoObject()
    {
        InitializeTeamInfo();

        // Call SetupControlScripts on the TeamRodsController instead
        TeamRodsController teamController = GetComponentInParent<TeamRodsController>();
        if (teamController != null)
        {
            teamController.SetupControlScripts(gameObject, false);
        }

        SetupRodFigures();
    }

    private void RodConfigurationFromTeamScriptableObject()
    {
        TeamRodsController teamRodsController = GetComponentInParent<TeamRodsController>();
        teamInfo = teamRodsController.testingTeam;
        teamUniform = "Local";
        teamFormation = activeFormationPreset != null
            ? activeFormationPreset.ToFormation()
            : teamRodsController.testingTeam.teamFormation;

        // Enable/disable scripts based on players assigned
        bool hasPlayers = teamRodsController.playersAssignedToThisTeamSide > 0;

        // Call SetupControlScripts on the TeamRodsController
        teamRodsController.SetupControlScripts(gameObject, hasPlayers);

        SetupRodFigures();
    }

    private void InitializeTeamInfo()
    {
        if (transform.parent.name == "LeftTeam")
        {
            teamInfo = MatchInfo.instance.leftTeam;
            teamUniform = MatchInfo.instance.leftTeamUniform;
            teamFormation = activeFormationPreset != null
                ? activeFormationPreset.ToFormation()
                : MatchInfo.instance.leftTeamLineUp;
        }
        else if (transform.parent.name == "RightTeam")
        {
            teamInfo = MatchInfo.instance.rightTeam;
            teamUniform = MatchInfo.instance.rightTeamUniform;
            teamFormation = activeFormationPreset != null
                ? activeFormationPreset.ToFormation()
                : MatchInfo.instance.rightTeamLineUp;
        }
    }

    private void SetupRodFigures()
    {
        // Determine number of foosballFigureGameObjectdles for this line
        rodFoosballFigureCount = GetNumberOffoosballFigureGameObjectdles(gameObject.name);

        // Get wall boundaries
        (float topWallY, float bottomWallY) = GetWallBoundaries();

        // Calculate available space
        float availableHeight = Mathf.Abs(topWallY - bottomWallY);

        // Create foosballFigureGameObjectdles
        DevidefoosballFigureGameObjectdlesInLine(availableHeight, rodFoosballFigureCount, bottomWallY);

        // Set movement limits
        CalculateRodMovementLimits(topWallY, bottomWallY);

        // Store foosballFigureGameObjectdle half-size
        halfPlayer = foosballFigureGameObject.transform.localScale.x / 2;

        // Set magnet effector radius based on actual figure spacing
        // Each figure's magnet radius = half the spacing to adjacent figures (with 10% gap)
        // This prevents magnet fields from overlapping between figures on the same rod
        SetMagnetRadiiFromSpacing(availableHeight, rodFoosballFigureCount);
    }

    /// <summary>
    /// Calculates magnet CircleCollider2D radius per figure based on actual spacing.
    /// For N figures: spacing = effectiveHeight / (N-1), radius = spacing / 2 * 0.9
    /// For GK (1 figure): radius = availableHeight / 4 (generous since no neighbors)
    /// </summary>
    private void SetMagnetRadiiFromSpacing(float availableHeight, int figureCount)
    {
        float radius;

        if (figureCount <= 1)
        {
            // Goalkeeper — no adjacent figures, cap at 3 to avoid oversized field
            radius = Mathf.Min(availableHeight / 4f, 3f);
        }
        else
        {
            // Calculate figure spacing using same spreadFactor as DevidefoosballFigureGameObjectdlesInLine
            float spreadFactor;
            switch (figureCount)
            {
                case 2: spreadFactor = 0.4f; break;
                case 3: spreadFactor = 0.6f; break;
                case 4: spreadFactor = 0.75f; break;
                case 5: spreadFactor = 0.85f; break;
                default: spreadFactor = 0.8f; break;
            }

            float effectiveHeight = availableHeight * spreadFactor;
            float spacing = effectiveHeight / (figureCount - 1);

            // Radius = half spacing with 10% gap to prevent overlap
            radius = (spacing / 2f) * 0.9f;
        }

        // Clamp to reasonable bounds
        radius = Mathf.Clamp(radius, 1f, 4f);

        foreach (CircleCollider2D effector in GetComponentsInChildren<CircleCollider2D>())
        {
            effector.radius = radius;
        }

        if (Debug.isDebugBuild && !AutoMatchRunner.IsAutoMode)
        {
            Debug.Log($"[RodConfig] {gameObject.name}: {figureCount} figures, magnet radius = {radius:F2}");
        }
    }

    private (float topY, float bottomY) GetWallBoundaries()
    {
        GameObject topWall = GameObject.FindWithTag("TopWall");
        GameObject bottomWall = GameObject.FindWithTag("BottomWall");

        if (topWall != null && bottomWall != null)
        {
            float topWallHeight = topWall.transform.localScale.y;
            float bottomWallHeight = bottomWall.transform.localScale.y;

            // Calculate inner edges
            float topWallY = topWall.transform.position.y - (topWallHeight / 2);
            float bottomWallY = bottomWall.transform.position.y + (bottomWallHeight / 2);

            return (topWallY, bottomWallY);
        }

        // Fallback to screen dimensions
        screenHalfWidthInWorldUnits = Camera.main.orthographicSize;
        return (screenHalfWidthInWorldUnits, -screenHalfWidthInWorldUnits);
    }

    private void CalculateRodMovementLimits(float topWallY, float bottomWallY)
    {
        float availableHeight = Mathf.Abs(topWallY - bottomWallY);
        float figureHeight = foosballFigureGameObject.transform.localScale.y;

        // Calculate the effective spread based on number of figures
        float spreadFactor;
        switch (rodFoosballFigureCount)
        {
            case 1: spreadFactor = 0f; break;
            case 2: spreadFactor = 0.4f; break;
            case 3: spreadFactor = 0.6f; break;
            case 4: spreadFactor = 0.75f; break;
            case 5: spreadFactor = 0.85f; break;
            default: spreadFactor = 0.8f; break;
        }

        float effectiveHeight = availableHeight * spreadFactor;
        float centerY = bottomWallY + (availableHeight / 2f);

        // Calculate how far the outermost figures are from center
        float maxDistanceFromCenter;
        if (rodFoosballFigureCount == 1)
        {
            maxDistanceFromCenter = 0f;
        }
        else
        {
            maxDistanceFromCenter = effectiveHeight / 2f;
        }

        // Calculate movement limit: from center to wall, minus figure placement, minus half figure size
        float distanceToWall = availableHeight / 2f;
        rodMovementLimit = distanceToWall - maxDistanceFromCenter - (figureHeight / 2f);

        // Ensure minimum movement (at least allow some movement)
        rodMovementLimit = Mathf.Max(rodMovementLimit, 0.5f);

        // Special case for goalkeeper
        if (rodFoosballFigureCount == 1 && gameObject.name == "GoalKepperRod")
        {
            CalculateGoalkeeperLimits();
        }
    }

    private void CalculateGoalkeeperLimits()
    {
        GameObject topGoalWall = GameObject.FindWithTag("TopGoalWall");
        GameObject bottomGoalWall = GameObject.FindWithTag("BottomGoalWall");

        if (topGoalWall == null || bottomGoalWall == null) return;

        // Get BoxCollider2D components to calculate actual wall sizes
        BoxCollider2D topGoalCollider = topGoalWall.GetComponent<BoxCollider2D>();
        BoxCollider2D bottomGoalCollider = bottomGoalWall.GetComponent<BoxCollider2D>();

        if (topGoalCollider == null || bottomGoalCollider == null)
        {
            Debug.LogError("GoalWalls are missing BoxCollider2D components!");
            return;
        }

        // Calculate the actual wall heights from box collider size
        float topGoalWallHeight = topGoalCollider.size.y;
        float bottomGoalWallHeight = bottomGoalCollider.size.y;

        // Calculate the inner edges of the goal walls using collider bounds
        float topGoalWallY = topGoalWall.transform.position.y - (topGoalWallHeight / 2);
        float bottomGoalWallY = bottomGoalWall.transform.position.y + (bottomGoalWallHeight / 2);

        // Calculate the available space within the goal
        float goalAvailableHeight = Mathf.Abs(topGoalWallY - bottomGoalWallY);

        // Get figure height from CapsuleCollider2D instead of transform scale
        CapsuleCollider2D figureCollider = foosballFigureGameObject.GetComponent<CapsuleCollider2D>();
        float figureHeight = figureCollider != null ? figureCollider.size.y : foosballFigureGameObject.transform.localScale.y;

        // Set goalkeeper movement limit to half the goal height
        // This allows the goalkeeper to move from center to either edge of the goal
        rodMovementLimit = (goalAvailableHeight / 2);

        // Ensure goalkeeper can at least move enough to cover minimal distance
        rodMovementLimit = Mathf.Max(rodMovementLimit, figureHeight);
    }

    /// <summary>
    /// This method distributes the foosballFigureGameObjectdles in the rod evenly along the available height
    /// </summary>
    void DevidefoosballFigureGameObjectdlesInLine(float availableHeight, int numfoosballFigureGameObjectdles, float startY)
    {
        TeamSide side = GetComponentInParent<TeamRodsController>().teamSide;
        float rotationZ = (side == TeamSide.RightTeam) ? -180f : 0f;

        // Calculate center-biased distribution
        float centerY = startY + (availableHeight / 2f);

        // Adjust spread based on number of figures
        // More figures = use more space, fewer figures = more concentrated
        float spreadFactor;
        switch (numfoosballFigureGameObjectdles)
        {
            case 1: spreadFactor = 0f; break;      // Single figure at center
            case 2: spreadFactor = 0.4f; break;    // 40% of height
            case 3: spreadFactor = 0.6f; break;    // 60% of height
            case 4: spreadFactor = 0.75f; break;   // 75% of height
            case 5: spreadFactor = 0.85f; break;   // 85% of height
            default: spreadFactor = 0.8f; break;
        }

        float effectiveHeight = availableHeight * spreadFactor;

        for (int i = 0; i < rodFoosballFigureCount; i++)
        {
            float currentY;

            if (rodFoosballFigureCount == 1)
            {
                // Single figure at center
                currentY = centerY;
            }
            else
            {
                // Distribute evenly within effective height
                float normalizedPos = (float)i / (rodFoosballFigureCount - 1);
                currentY = (centerY - effectiveHeight / 2f) + (normalizedPos * effectiveHeight);
            }

            // Configure foosballFigureGameObjectdle appearance
            foosballFigureGameObject.GetComponent<FoosballFigureUniformAnimator>().teamPicked = teamInfo.teamName;
            foosballFigureGameObject.GetComponent<FoosballFigureUniformAnimator>().uniform = teamUniform;

            // Create new foosballFigureGameObjectdle
            GameObject newfoosballFigureGameObjectdle = Instantiate(
                foosballFigureGameObject,
                new Vector2(transform.position.x, currentY),
                Quaternion.Euler(0f, 0f, rotationZ)
            );
            newfoosballFigureGameObjectdle.transform.parent = transform;
        }
    }

    /// <summary>
    /// Return the numbers of foosballFigureGameObjectdles in certain line
    /// </summary>
    int GetNumberOffoosballFigureGameObjectdles(string lineType)
    {
        switch (lineType)
        {
            case "AttackerRod": return teamFormation.attack;
            case "MidfieldRod": return teamFormation.mid;
            case "DefenseRod": return teamFormation.defense;
            default: return 1; // Goalkeeper
        }
    }

    #region Analysis Methods

    /// <summary>
    /// Calculate and return the distances between each consecutive child object in this line
    /// </summary>
    public float[] GetDistancesBetweenPlayers()
    {
        if (transform.childCount < 2) return new float[0];

        Transform[] children = GetSortedChildren();

        float[] distances = new float[children.Length - 1];
        for (int i = 0; i < children.Length - 1; i++)
        {
            distances[i] = Vector2.Distance(children[i].position, children[i + 1].position);
        }

        return distances;
    }

    /// <summary>
    /// Calculate and log the average, minimum, and maximum distances between players
    /// </summary>
    public void AnalyzePlayerDistances()
    {
        float[] distances = GetDistancesBetweenPlayers();

        if (distances.Length == 0)
        {
            if (!AutoMatchRunner.IsAutoMode) Debug.Log($"Line {gameObject.name}: No distances to analyze (less than 2 players)");
            return;
        }

        // Calculate statistics
        float sum = 0f;
        float min = float.MaxValue;
        float max = float.MinValue;

        foreach (float dist in distances)
        {
            sum += dist;
            min = Mathf.Min(min, dist);
            max = Mathf.Max(max, dist);
        }

        float average = sum / distances.Length;
        if (!AutoMatchRunner.IsAutoMode) Debug.Log($"Line {gameObject.name}: Avg distance = {average:F2}, Min = {min:F2}, Max = {max:F2}");
    }

    /// <summary>
    /// Calculate the distance from the first player to bottom wall and from the last player to top wall
    /// </summary>
    public Vector2 GetDistancesToWalls()
    {
        GameObject topWall = GameObject.FindWithTag("TopWall");
        GameObject bottomWall = GameObject.FindWithTag("BottomWall");

        if (topWall == null || bottomWall == null || transform.childCount == 0)
        {
            Debug.LogWarning($"Line {gameObject.name}: Cannot measure wall distances - missing walls or players");
            return Vector2.zero;
        }

        float topWallLowerEdge = topWall.transform.position.y - (topWall.transform.localScale.y / 2);
        float bottomWallUpperEdge = bottomWall.transform.position.y + (bottomWall.transform.localScale.y / 2);

        Transform[] children = GetSortedChildren();

        // Get distances to walls
        float distanceToBottomWall = Mathf.Abs(children[0].position.y - bottomWallUpperEdge);
        float distanceToTopWall = Mathf.Abs(children[children.Length - 1].position.y - topWallLowerEdge);

        if (!AutoMatchRunner.IsAutoMode) Debug.Log($"Line {gameObject.name}: Distance to bottom wall = {distanceToBottomWall:F2}, Distance to top wall = {distanceToTopWall:F2}");

        return new Vector2(distanceToBottomWall, distanceToTopWall);
    }

    /// <summary>
    /// Checks if the distances to walls are appropriate and logs any irregularities
    /// </summary>
    public void AnalyzeWallDistances()
    {
        Vector2 distances = GetDistancesToWalls();

        GameObject topWall = GameObject.FindWithTag("TopWall");
        GameObject bottomWall = GameObject.FindWithTag("BottomWall");

        if (topWall == null || bottomWall == null || rodFoosballFigureCount <= 0) return;

        // Calculate expected spacing
        float topWallLowerEdge = topWall.transform.position.y - (topWall.transform.localScale.y / 2);
        float bottomWallUpperEdge = bottomWall.transform.position.y + (bottomWall.transform.localScale.y / 2);
        float availableHeight = Mathf.Abs(topWallLowerEdge - bottomWallUpperEdge);
        float expectedDistance = availableHeight / (rodFoosballFigureCount + 1);

        // Check if spacing is consistent
        float bottomDifference = Mathf.Abs(distances.x - expectedDistance);
        float topDifference = Mathf.Abs(distances.y - expectedDistance);

        if (bottomDifference > 0.1f || topDifference > 0.1f)
        {
            Debug.LogWarning($"Line {gameObject.name}: Irregular wall spacing detected!");
            Debug.LogWarning($"Expected: {expectedDistance:F2}, Bottom actual: {distances.x:F2}, Top actual: {distances.y:F2}");
        }
        else
        {
            if (!AutoMatchRunner.IsAutoMode) Debug.Log($"Line {gameObject.name}: Wall distances are consistent with player spacing ({expectedDistance:F2})");
        }
    }

    /// <summary>
    /// Get children sorted by Y position
    /// </summary>
    private Transform[] GetSortedChildren()
    {
        Transform[] children = new Transform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            children[i] = transform.GetChild(i);
        }

        System.Array.Sort(children, (a, b) => a.position.y.CompareTo(b.position.y));
        return children;
    }

    #endregion
}
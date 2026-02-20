using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ControlType
{
    Automatic,
    Manual
}

public enum TeamSide
{
    LeftTeam,
    RightTeam
}

[DefaultExecutionOrder(0)]
public class TeamRodsController : MonoBehaviour
{
    [Header("Team Configuration")]
    public TeamSide teamSide;
    public ControlType controlType;

    [Header("Player Configuration")]
    public int playersAssignedToThisTeamSide;
    public PlayerInput defensePlayerInput;
    public PlayerInput attackerPlayerInput;

    [Header("Line References")]
    [Tooltip("Lines[0]=GK, Lines[1]=Defense, Lines[2]=Midfield, Lines[3]=Attack")]
    public GameObject[] lines = new GameObject[4];
    public bool[] activeLines = new bool[4];

    [Header("Visual Indicators")]
    public SpriteRenderer[] linesIndicators = new SpriteRenderer[4];
    public Sprite inactiveLineSprite;
    public Sprite activeLineSprite;

    [Header("Testing")]
    public Team testingTeam;

    // Input Actions
    private InputAction defenseToggleLeftAction;   // For GK line
    private InputAction defenseToggleRightAction;  // For Defense line
    private InputAction attackToggleLeftAction;    // For Midfield line
    private InputAction attackToggleRightAction;   // For Attack line

    private InputAction changeLineToLeftAction;    // For single player mode
    private InputAction changeLineToRightAction;   // For single player mode

    // Field boundaries for automatic line selection
    private readonly float[] linesActiveBallLimit = new float[3];
    private GameObject ball;
    private int lineIndex = 1;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Check for match info or use testing setup
        if (MatchInfo.instance != null)
        {
            SetupFromMatchInfo();
        }
        else
        {
            SetupForTestMode();
        }

        // Configure input based on player count
        SetupInputActions();
    }

    private void SetupFromMatchInfo()
    {
        if (teamSide == TeamSide.LeftTeam && MatchInfo.instance.leftControllers.Count > 0)
        {
            AssignControllers(TeamSide.LeftTeam);
            // Configure all rods for player control
            ConfigureAllRodsControlScripts(true);
        }
        else if (teamSide == TeamSide.RightTeam && MatchInfo.instance.rightControllers.Count > 0)
        {
            AssignControllers(TeamSide.RightTeam);
            // Configure all rods for player control
            ConfigureAllRodsControlScripts(true);
        }
        else
        {
            // No players assigned to this team, configure for AI control
            ConfigureAllRodsControlScripts(false);
        }
    }

    private void SetupForTestMode()
    {
        if (playersAssignedToThisTeamSide <= 0)
        {
            // Configure all rods for AI control
            ConfigureAllRodsControlScripts(false);
            this.enabled = false;
        }
        else
        {
            // Configure all rods for player control
            ConfigureAllRodsControlScripts(true);
        }
    }

    private void DisablePlayerControls()
    {
        ConfigureAllRodsControlScripts(false);
        this.enabled = false;
    }

    private void SetupInputActions()
    {
        if (playersAssignedToThisTeamSide == 1)
        {
            SetupSinglePlayerControls();
        }
        else if (playersAssignedToThisTeamSide == 2)
        {
            SetupTwoPlayerControls();
        }
    }

    private void SetupSinglePlayerControls()
    {
        if (defensePlayerInput == null) return;

        changeLineToLeftAction = defensePlayerInput.actions["ChangeLinesToLeft"];
        changeLineToRightAction = defensePlayerInput.actions["ChangeLinesToRight"];

        if (changeLineToLeftAction != null)
        {
            changeLineToLeftAction.performed += OnChangeLineToLeft;
        }

        if (changeLineToRightAction != null)
        {
            changeLineToRightAction.performed += OnChangeLineToRight;
        }
    }

    private void SetupTwoPlayerControls()
    {
        // Set up defense player controls (Goalkeeper and Defense)
        if (defensePlayerInput != null)
        {
            defenseToggleLeftAction = defensePlayerInput.actions["ToggleLeftLine"];
            defenseToggleRightAction = defensePlayerInput.actions["ToggleRightLine"];

            if (defenseToggleLeftAction != null)
                defenseToggleLeftAction.performed += OnDefenseToggleLeftLine;

            if (defenseToggleRightAction != null)
                defenseToggleRightAction.performed += OnDefenseToggleRightLine;
        }

        // Set up attacker player controls (Midfield and Attack)
        if (attackerPlayerInput != null)
        {
            attackToggleLeftAction = attackerPlayerInput.actions["ToggleLeftLine"];
            attackToggleRightAction = attackerPlayerInput.actions["ToggleRightLine"];

            if (attackToggleLeftAction != null)
                attackToggleLeftAction.performed += OnAttackToggleLeftLine;

            if (attackToggleRightAction != null)
                attackToggleRightAction.performed += OnAttackToggleRightLine;
        }
    }

    private void Start()
    {
        ball = GameObject.Find("Ball");

        // Set field boundaries based on team side
        //SetFieldBoundaries();

        // Initial line activation
        if (controlType == ControlType.Manual)
        {
            if (playersAssignedToThisTeamSide == 1)
                ManualActiveLines(lineIndex);
            else if (playersAssignedToThisTeamSide == 2)
                SetTwoPlayerDefaultLines();
        }
    }

    private void SetTwoPlayerDefaultLines()
    {
        // Default to all lines active in two-player mode
        activeLines = new bool[] { true, true, true, true };
        UpdateLinesActiveStatus();
    }

    private void OnEnable()
    {
        EnableInputActions();
    }

    private void OnDisable()
    {
        DisableInputActions();
    }

    private void EnableInputActions()
    {
        // Single player actions
        changeLineToLeftAction?.Enable();
        changeLineToRightAction?.Enable();

        // Two player actions
        defenseToggleLeftAction?.Enable();
        defenseToggleRightAction?.Enable();
        attackToggleLeftAction?.Enable();
        attackToggleRightAction?.Enable();
    }

    private void DisableInputActions()
    {
        // Unsubscribe from single player events
        if (changeLineToLeftAction != null)
        {
            changeLineToLeftAction.performed -= OnChangeLineToLeft;
            changeLineToLeftAction.Disable();
        }

        if (changeLineToRightAction != null)
        {
            changeLineToRightAction.performed -= OnChangeLineToRight;
            changeLineToRightAction.Disable();
        }

        // Unsubscribe from two-player events
        if (defenseToggleLeftAction != null)
        {
            defenseToggleLeftAction.performed -= OnDefenseToggleLeftLine;
            defenseToggleLeftAction.Disable();
        }

        if (defenseToggleRightAction != null)
        {
            defenseToggleRightAction.performed -= OnDefenseToggleRightLine;
            defenseToggleRightAction.Disable();
        }

        if (attackToggleLeftAction != null)
        {
            attackToggleLeftAction.performed -= OnAttackToggleLeftLine;
            attackToggleLeftAction.Disable();
        }

        if (attackToggleRightAction != null)
        {
            attackToggleRightAction.performed -= OnAttackToggleRightLine;
            attackToggleRightAction.Disable();
        }
    }

    #region Input Action Callbacks

    /// <summary>
    /// Handles line selection when user presses the "change line left" button
    /// Changes line selection in direction appropriate to team orientation
    /// </summary>
    private void OnChangeLineToLeft(InputAction.CallbackContext context)
    {
        if (controlType != ControlType.Manual || playersAssignedToThisTeamSide != 1)
            return;

        if (teamSide == TeamSide.LeftTeam)
        {
            // For left team, "left" means toward goalkeeper (decrease index)
            lineIndex = Mathf.Max(0, lineIndex - 1);
        }
        else // Right team
        {
            // For right team, "left" means toward attacker (increase index)
            lineIndex = Mathf.Min(2, lineIndex + 1);
        }

        ManualActiveLines(lineIndex);
    }

    /// <summary>
    /// Handles line selection when user presses the "change line right" button
    /// Changes line selection in direction appropriate to team orientation
    /// </summary>
    private void OnChangeLineToRight(InputAction.CallbackContext context)
    {
        if (controlType != ControlType.Manual || playersAssignedToThisTeamSide != 1)
            return;

        if (teamSide == TeamSide.LeftTeam)
        {
            // For left team, "right" means toward attacker (increase index)
            lineIndex = Mathf.Min(2, lineIndex + 1);
        }
        else // Right team
        {
            // For right team, "right" means toward goalkeeper (decrease index)
            lineIndex = Mathf.Max(0, lineIndex - 1);
        }

        ManualActiveLines(lineIndex);
    }

    // Two-player line toggles
    private void OnDefenseToggleLeftLine(InputAction.CallbackContext context)
    {
        if (controlType != ControlType.Manual || playersAssignedToThisTeamSide != 2)
            return;

        // Toggle Goalkeeper line (index 0)
        activeLines[0] = !activeLines[0];
        UpdateLinesActiveStatus();
        Debug.Log($"GK Line toggled: {activeLines[0]}");
    }

    private void OnDefenseToggleRightLine(InputAction.CallbackContext context)
    {
        if (controlType != ControlType.Manual || playersAssignedToThisTeamSide != 2)
            return;

        // Toggle Defense line (index 1)
        activeLines[1] = !activeLines[1];
        UpdateLinesActiveStatus();
        Debug.Log($"Defense Line toggled: {activeLines[1]}");
    }

    private void OnAttackToggleLeftLine(InputAction.CallbackContext context)
    {
        if (controlType != ControlType.Manual || playersAssignedToThisTeamSide != 2)
            return;

        // Toggle Midfield line (index 2)
        activeLines[2] = !activeLines[2];
        UpdateLinesActiveStatus();
        Debug.Log($"Midfield Line toggled: {activeLines[2]}");
    }

    private void OnAttackToggleRightLine(InputAction.CallbackContext context)
    {
        if (controlType != ControlType.Manual || playersAssignedToThisTeamSide != 2)
            return;

        // Toggle Attack line (index 3)
        activeLines[3] = !activeLines[3];
        UpdateLinesActiveStatus();
        Debug.Log($"Attack Line toggled: {activeLines[3]}");
    }

    #endregion

    void FixedUpdate()
    {
        //if (controlType == ControlType.Automatic)
        //    AutomaticControls();
    }

    private void AutomaticControls()
    {
        if (ball == null)
        {
            ball = GameObject.FindGameObjectWithTag("Ball");
            return;
        }

        // Activate lines based on ball position
        if (teamSide == TeamSide.LeftTeam)
            GetClosestLinesLeftSide();
        else
            GetClosestLinesRightSide();
    }

    private void GetClosestLinesLeftSide()
    {
        float ballPos = ball.transform.position.x;

        if (ballPos < linesActiveBallLimit[0])
            activeLines = new bool[] { true, false, false, false };
        else if (ballPos < linesActiveBallLimit[1])
            activeLines = new bool[] { true, true, false, false };
        else if (ballPos > linesActiveBallLimit[2])
            activeLines = new bool[] { false, false, true, true };
        else
            activeLines = new bool[] { false, true, true, false };

        UpdateLinesActiveStatus();
    }

    private void GetClosestLinesRightSide()
    {
        float ballPos = ball.transform.position.x;

        if (ballPos > linesActiveBallLimit[0])
            activeLines = new bool[] { true, false, false, false };
        else if (ballPos > linesActiveBallLimit[1])
            activeLines = new bool[] { true, true, false, false };
        else if (ballPos < linesActiveBallLimit[2])
            activeLines = new bool[] { false, false, true, true };
        else
            activeLines = new bool[] { false, true, true, false };

        UpdateLinesActiveStatus();
    }

    /// <summary>
    /// Updates all lines and indicators based on the current activeLines array
    /// </summary>
    private void UpdateLinesActiveStatus()
    {
        // Update line active states
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] != null)
            {
                var PlayerRodMovementAction = lines[i].GetComponent<PlayerRodMovementAction>();
                if (PlayerRodMovementAction) PlayerRodMovementAction.isActive = activeLines[i];

                // Update animator states in children (players)
                for (int j = 0; j < lines[i].transform.childCount; j++)
                {
                    var animator = lines[i].transform.GetChild(j).GetComponent<Animator>();
                    if (animator) animator.SetBool("IsLineActive", activeLines[i]);
                }
            }
        }

        // Update visual indicators
        for (int i = 0; i < linesIndicators.Length; i++)
        {
            if (linesIndicators[i] != null)
                linesIndicators[i].sprite = activeLines[i] ? activeLineSprite : inactiveLineSprite;
        }
    }

    /// <summary>
    /// Activate predefined line configurations for single-player manual mode
    /// </summary>
    private void ManualActiveLines(int index)
    {
        switch (index)
        {
            case 0:
                activeLines = new bool[] { true, true, false, false };
                break;
            case 1:
                activeLines = new bool[] { false, true, true, false };
                break;
            case 2:
                activeLines = new bool[] { false, false, true, true };
                break;
        }

        UpdateLinesActiveStatus();
    }

    private void AssignControllers(TeamSide side)
    {
        List<PlayerInput> controlList;
        int controlsCount;

        if (side == TeamSide.LeftTeam)
        {
            controlsCount = MatchInfo.instance.leftControllers.Count;
            controlList = MatchInfo.instance.leftControllers;
        }
        else
        {
            controlsCount = MatchInfo.instance.rightControllers.Count;
            controlList = MatchInfo.instance.rightControllers;
        }

        playersAssignedToThisTeamSide = controlsCount;

        if (controlsCount == 0)
        {
            defensePlayerInput = null;
            attackerPlayerInput = null;
            //Disable this component
            DisablePlayerControls();
            this.enabled = false;
        }
        else if (controlsCount == 1)
        {
            // Single player controls both defense and attack
            defensePlayerInput = controlList[0];
            attackerPlayerInput = controlList[0];
        }
        else if (controlsCount >= 2)
        {
            // Split controls between two players
            defensePlayerInput = controlList[0];  // First player handles GK and Defense
            attackerPlayerInput = controlList[1]; // Second player handles Mid and Attack
        }
    }

    /// <summary>
    /// Returns the appropriate PlayerInput for the given line
    /// </summary>
    public PlayerInput GetPlayerInputForRodActions(string lineName)
    {
        if (playersAssignedToThisTeamSide <= 0)
            return null;

        if (playersAssignedToThisTeamSide == 1)
            return defensePlayerInput; // One player controls everything

        // Two players - split control
        if (lineName == "GoalKepperRod" || lineName == "DefenseRod")
            return defensePlayerInput;
        else
            return attackerPlayerInput;
    }

    /// <summary>
    /// Configures all rods in this team to use either player control or AI control
    /// </summary>
    /// <param name="usePlayerControl">True for player control, false for AI control</param>
    private void ConfigureAllRodsControlScripts(bool usePlayerControl)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] != null)
            {
                SetupControlScripts(lines[i], usePlayerControl);
            }
        }
    }

    /// <summary>
    /// Configures the control scripts on a rod based on whether it should be player-controlled or AI-controlled
    /// 
    /// IMPROVED: Now REMOVES unused components instead of just disabling them
    /// This makes debugging easier by showing only relevant components in the Inspector
    /// 
    /// PLAYER CONTROL:
    /// - Removes: AIRodMovementAction, AIRodStateMachine, AIGoalEvaluator, AIRodMagnetAction, AIRodShootAction, AIRodWallPassAction
    /// - Keeps/Adds: PlayerRodMovementAction, PlayerRodShootAction, PlayerRodMagnetAction, PlayerRodWallPassAction
    /// 
    /// AI CONTROL:
    /// - Removes: PlayerRodMovementAction, PlayerRodShootAction, PlayerRodMagnetAction, PlayerRodWallPassAction
    /// - Keeps/Adds: AIRodMovementAction, AIRodStateMachine, AIGoalEvaluator, AIRodMagnetAction, AIRodShootAction, AIRodWallPassAction
    /// </summary>
    /// <param name="rod">The rod GameObject to configure</param>
    /// <param name="playerControlled">Whether the rod should be player-controlled (true) or AI-controlled (false)</param>
    public void SetupControlScripts(GameObject rod, bool playerControlled = false)
    {
        if (rod == null) return;

        if (playerControlled)
        {
            // === PLAYER CONTROL ===
            Debug.Log($"[TeamRodsController] Configuring {rod.name} for PLAYER control - Removing AI components, adding Player components");

            // REMOVE AI components
            RemoveAIComponents(rod);

            // ADD/ENSURE Player components
            AddPlayerComponents(rod);
        }
        else
        {
            // === AI CONTROL ===
            Debug.Log($"[TeamRodsController] Configuring {rod.name} for AI control - Removing Player components, adding AI components");

            // REMOVE Player components
            RemovePlayerComponents(rod);

            // ADD/ENSURE AI components
            AddAIComponents(rod);
        }
    }

    /// <summary>
    /// Removes all AI-related components from the rod
    /// </summary>
    private void RemoveAIComponents(GameObject rod)
    {
        // Remove AI action components (added in refactoring)
        DestroyComponentIfExists<AIRodMagnetAction>(rod);
        DestroyComponentIfExists<AIRodShootAction>(rod);
        DestroyComponentIfExists<AIRodWallPassAction>(rod);

        // Remove AI core components
        DestroyComponentIfExists<AIRodStateMachine>(rod);
        DestroyComponentIfExists<AIGoalEvaluator>(rod);
        DestroyComponentIfExists<AIRodMovementAction>(rod);

        Debug.Log($"✓ Removed AI components from {rod.name}");
    }

    /// <summary>
    /// Removes all Player-related components from the rod
    /// </summary>
    private void RemovePlayerComponents(GameObject rod)
    {
        // Remove Player action components
        DestroyComponentIfExists<PlayerRodMagnetAction>(rod);
        DestroyComponentIfExists<PlayerRodShootAction>(rod);
        DestroyComponentIfExists<PlayerRodWallPassAction>(rod);

        // Remove Player movement component
        DestroyComponentIfExists<PlayerRodMovementAction>(rod);

        Debug.Log($"✓ Removed Player components from {rod.name}");
    }

    /// <summary>
    /// Adds/ensures all necessary Player components are present
    /// </summary>
    private void AddPlayerComponents(GameObject rod)
    {
        // Get rod configuration for reference
        RodConfiguration rodConfig = rod.GetComponent<RodConfiguration>();
        if (rodConfig == null)
        {
            Debug.LogError($"[TeamRodsController] {rod.name} is missing RodConfiguration component!");
            return;
        }

        // Add PlayerRodMovementAction
        PlayerRodMovementAction playerMovement = rod.GetComponent<PlayerRodMovementAction>();
        if (playerMovement == null)
        {
            playerMovement = rod.AddComponent<PlayerRodMovementAction>();
            Debug.Log($"✓ Added PlayerRodMovementAction to {rod.name}");
        }
        playerMovement.enabled = true;
        playerMovement.isActive = false; // Will be activated by UpdateLinesActiveStatus()

        // Add PlayerRodShootAction
        PlayerRodShootAction playerShoot = rod.GetComponent<PlayerRodShootAction>();
        if (playerShoot == null)
        {
            playerShoot = rod.AddComponent<PlayerRodShootAction>();
            Debug.Log($"✓ Added PlayerRodShootAction to {rod.name}");
        }
        playerShoot.enabled = true;

        // Add PlayerRodMagnetAction
        PlayerRodMagnetAction playerMagnet = rod.GetComponent<PlayerRodMagnetAction>();
        if (playerMagnet == null)
        {
            playerMagnet = rod.AddComponent<PlayerRodMagnetAction>();
            Debug.Log($"✓ Added PlayerRodMagnetAction to {rod.name}");
        }
        playerMagnet.enabled = true;

        // Add PlayerRodWallPassAction
        PlayerRodWallPassAction playerWallPass = rod.GetComponent<PlayerRodWallPassAction>();
        if (playerWallPass == null)
        {
            playerWallPass = rod.AddComponent<PlayerRodWallPassAction>();
            Debug.Log($"✓ Added PlayerRodWallPassAction to {rod.name}");
        }
        playerWallPass.enabled = true;

        Debug.Log($"✅ Player components configured on {rod.name}");
    }

    /// <summary>
    /// Adds/ensures all necessary AI components are present
    /// </summary>
    private void AddAIComponents(GameObject rod)
    {
        // Get rod configuration for reference
        RodConfiguration rodConfig = rod.GetComponent<RodConfiguration>();
        if (rodConfig == null)
        {
            Debug.LogError($"[TeamRodsController] {rod.name} is missing RodConfiguration component!");
            return;
        }

        // Add AIRodMovementAction
        AIRodMovementAction aiMovement = rod.GetComponent<AIRodMovementAction>();
        if (aiMovement == null)
        {
            aiMovement = rod.AddComponent<AIRodMovementAction>();
            Debug.Log($"✓ Added AIRodMovementAction to {rod.name}");
        }
        aiMovement.enabled = true;
        aiMovement.isActive = false; // Will be activated by AITeamRodsController

        // Add AIGoalEvaluator
        AIGoalEvaluator aiGoalEvaluator = rod.GetComponent<AIGoalEvaluator>();
        if (aiGoalEvaluator == null)
        {
            aiGoalEvaluator = rod.AddComponent<AIGoalEvaluator>();
            Debug.Log($"✓ Added AIGoalEvaluator to {rod.name}");
        }
        aiGoalEvaluator.enabled = true;

        // Add AIRodStateMachine (must be before action components as they depend on it)
        AIRodStateMachine aiFSM = rod.GetComponent<AIRodStateMachine>();
        if (aiFSM == null)
        {
            aiFSM = rod.AddComponent<AIRodStateMachine>();
            Debug.Log($"✓ Added AIRodStateMachine to {rod.name}");
        }
        aiFSM.enabled = true;
        aiFSM.useFSM = true;

        // NOTE: AIRodMagnetAction, AIRodShootAction, AIRodWallPassAction are automatically added
        // by AIRodStateMachine.Awake() if missing, so we don't need to manually add them here.
        // But we can verify they exist after a frame

        Debug.Log($"✅ AI components configured on {rod.name} (Action components will be auto-added by AIRodStateMachine)");
    }

    /// <summary>
    /// Helper method to destroy a component if it exists
    /// Safe to call even if component doesn't exist
    /// </summary>
    private void DestroyComponentIfExists<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component != null)
        {
            // Use DestroyImmediate in editor, Destroy at runtime
            if (Application.isPlaying)
            {
                Destroy(component);
            }
            else
            {
                DestroyImmediate(component);
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Manages team and player assignments in the game setup screen.
/// Detects raw input to manually join players with the correct control scheme.
/// </summary>
[DefaultExecutionOrder(0)]
public class GameControlsConfigPanel : MonoBehaviour
{
    #region Singleton

    public static GameControlsConfigPanel instance;

    #endregion

    #region UI References

    [Header("Panels")]
    public GameObject noControlsPanel;
    public GameObject assignControlsPanel;

    [Header("Player UI")]
    public Image playerOneCircleSprite;
    public Image playerTwoCircleSprite;
    public Image playerThreeCircleSprite;
    public Image playerFourCircleSprite;

    [Header("Team UI")]
    public Image defenderleftTeamCircleSprite;
    public Image attackerleftTeamCircleSprite;
    public Image defenderRightTeamCircleSprite;
    public Image attackerRightTeamCircleSprite;

    [Header("UI Elements")]
    public Text matchTypeText;
    public Button StartGameButton;

    #endregion

    #region Player State

    // Track player states
    [SerializeField] private bool[] isPlayerActive = new bool[4] { false, false, false, false };
    [SerializeField] private string[] playerCurrentPosition = new string[4] { "mid", "mid", "mid", "mid" };

    // Track team assignments
    [SerializeField] private List<int> leftTeamPlayerIds = new List<int>();
    [SerializeField] private List<int> rightTeamPlayerIds = new List<int>();

    #endregion

    #region Join Detection

    // P1 join key — only the dedicated join key, NOT team selection keys (A/D)
    private static readonly Key[] P1JoinKeys = {
        Key.F
    };

    // P2 join key — only the dedicated join key, NOT team selection keys (arrows)
    private static readonly Key[] P2JoinKeys = {
        Key.NumpadEnter
    };

    private bool isWaitingForJoin = true;

    #endregion

    private Sprite comCircleSprite;

    #region Unity Lifecycle Methods

    private void Awake()
    {
        InitializeSingleton();
        SaveComSpriteReference();
    }

    private void OnEnable()
    {
        InitializeTeamLists();
        ResetPlayerStates();
        isWaitingForJoin = true;
    }

    private void Update()
    {
        if (!isWaitingForJoin) return;
        DetectJoinInput();
    }

    #endregion

    #region Initialization Methods

    private void InitializeSingleton()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeTeamLists()
    {
        leftTeamPlayerIds = new List<int>();
        rightTeamPlayerIds = new List<int>();
    }

    private void ResetPlayerStates()
    {
        for (int i = 0; i < isPlayerActive.Length; i++)
        {
            isPlayerActive[i] = false;
            playerCurrentPosition[i] = "mid";
        }
        StartGameButton.gameObject.SetActive(false);
    }

    private void SaveComSpriteReference()
    {
        comCircleSprite = defenderleftTeamCircleSprite.sprite;
    }

    #endregion

    #region Join Input Detection

    /// <summary>
    /// Polls raw input each frame to detect join requests from keyboard or gamepad.
    /// </summary>
    private void DetectJoinInput()
    {
        if (PlayersInputController.instance == null) return;
        if (PlayersInputController.instance.playerInputs.Count >= PlayersInputController.MaxPlayers) return;

        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            // Check P1 keyboard join
            if (!PlayersInputController.instance.IsSchemeAlreadyJoined(PlayersInputController.SchemeKeyboardP1))
            {
                foreach (var key in P1JoinKeys)
                {
                    if (keyboard[key].wasPressedThisFrame)
                    {
                        PlayersInputController.instance.JoinPlayerManually(
                            PlayersInputController.SchemeKeyboardP1, keyboard);
                        break;
                    }
                }
            }

            // Check P2 keyboard join
            if (!PlayersInputController.instance.IsSchemeAlreadyJoined(PlayersInputController.SchemeKeyboardP2))
            {
                foreach (var key in P2JoinKeys)
                {
                    if (keyboard[key].wasPressedThisFrame)
                    {
                        PlayersInputController.instance.JoinPlayerManually(
                            PlayersInputController.SchemeKeyboardP2, keyboard);
                        break;
                    }
                }
            }
        }

        // Check gamepad join (each gamepad is unique, use Xbox scheme)
        foreach (var gamepad in Gamepad.all)
        {
            if (gamepad.startButton.wasPressedThisFrame ||
                gamepad.buttonSouth.wasPressedThisFrame)
            {
                // Only join if this specific gamepad isn't already paired
                bool alreadyPaired = PlayersInputController.instance.playerInputs.Any(
                    p => p.devices.Contains(gamepad));
                if (!alreadyPaired)
                {
                    PlayersInputController.instance.JoinPlayerManually(
                        PlayersInputController.SchemeXbox, gamepad);
                }
            }
        }
    }

    #endregion

    #region Player Management

    /// <summary>
    /// Handles UI updates when a player joins the game
    /// </summary>
    public void PlayerJoinedUiActivation(int playerNumber)
    {
        if (!assignControlsPanel.activeSelf)
        {
            assignControlsPanel.SetActive(true);
            noControlsPanel.SetActive(false);
        }

        switch (playerNumber)
        {
            case 0:
                CheckInputToActivatePlayers(isPlayerActive[0], playerOneCircleSprite, 0);
                break;
            case 1:
                CheckInputToActivatePlayers(isPlayerActive[1], playerTwoCircleSprite, 1);
                break;
            case 2:
                CheckInputToActivatePlayers(isPlayerActive[2], playerThreeCircleSprite, 2);
                break;
            case 3:
                CheckInputToActivatePlayers(isPlayerActive[3], playerFourCircleSprite, 3);
                break;
        }
    }

    /// <summary>
    /// Updates UI when a player leaves the game
    /// </summary>
    public void PlayerLeftWhileThisPanelIsActive(int playerNumber)
    {
        if (playerNumber < 0 || playerNumber >= isPlayerActive.Length) return;

        Image playerUI = ReturnPlayerUiImage(playerNumber);
        if (playerUI != null)
        {
            playerUI.color = new Color(1f, 1f, 1f, 0.5f);
        }

        isPlayerActive[playerNumber] = false;
        RemovePlayerFromTeams(playerNumber);
    }

    private void RemovePlayerFromTeams(int playerNumber)
    {
        if (leftTeamPlayerIds.Contains(playerNumber))
        {
            HandleReturnToMiddle(playerNumber, leftTeamPlayerIds, defenderleftTeamCircleSprite, attackerleftTeamCircleSprite);
        }
        else if (rightTeamPlayerIds.Contains(playerNumber))
        {
            HandleReturnToMiddle(playerNumber, rightTeamPlayerIds, defenderRightTeamCircleSprite, attackerRightTeamCircleSprite);
        }

        SetMatchType();
    }

    private void CheckInputToActivatePlayers(bool isObjectActive, Image playerBall, int playerNumber)
    {
        if (!isObjectActive)
        {
            playerBall.color = new Color(1f, 1f, 1f, 1f);
            isPlayerActive[playerNumber] = true;
        }
    }

    #endregion

    #region Team Selection Logic

    /// <summary>
    /// Process player input to assign teams
    /// </summary>
    public void CheckInputToChooseTeamSide(int playerIndex, string buttonPressed)
    {
        if (!isPlayerActive[playerIndex]) return;

        switch (playerCurrentPosition[playerIndex])
        {
            case "mid":
                if (buttonPressed == "left")
                {
                    HandleLeftTeamSelection(playerIndex);
                }
                else if (buttonPressed == "right")
                {
                    HandleRightTeamSelection(playerIndex);
                }
                break;

            case "left":
                if (buttonPressed == "right")
                {
                    HandleReturnToMiddle(
                        playerIndex,
                        leftTeamPlayerIds,
                        defenderleftTeamCircleSprite,
                        attackerleftTeamCircleSprite);
                }
                break;

            case "right":
                if (buttonPressed == "left")
                {
                    HandleReturnToMiddle(
                        playerIndex,
                        rightTeamPlayerIds,
                        defenderRightTeamCircleSprite,
                        attackerRightTeamCircleSprite);
                }
                break;
        }
    }

    private void HandleLeftTeamSelection(int playerIndex)
    {
        if (leftTeamPlayerIds.Count >= 2)
        {
            Debug.LogWarning("Left team is already full");
            return;
        }

        if (leftTeamPlayerIds.Count == 0)
        {
            leftTeamPlayerIds.Add(playerIndex);
            SetPlayerUiImage(playerIndex, ReturnPlayerUiImage(playerIndex).sprite, defenderleftTeamCircleSprite, attackerleftTeamCircleSprite);
        }
        else if (leftTeamPlayerIds.Count == 1)
        {
            leftTeamPlayerIds.Add(playerIndex);
            SetPlayerUiImage(playerIndex, ReturnPlayerUiImage(playerIndex).sprite, attackerleftTeamCircleSprite);
        }

        SetPlayerUiImageTransparency(playerIndex, 0.5f);
        playerCurrentPosition[playerIndex] = "left";
        SetMatchType();
    }

    private void HandleRightTeamSelection(int playerIndex)
    {
        if (rightTeamPlayerIds.Count >= 2)
        {
            Debug.LogWarning("Right team is already full");
            return;
        }

        if (rightTeamPlayerIds.Count == 0)
        {
            rightTeamPlayerIds.Add(playerIndex);
            SetPlayerUiImage(playerIndex, ReturnPlayerUiImage(playerIndex).sprite, defenderRightTeamCircleSprite, attackerRightTeamCircleSprite);
        }
        else if (rightTeamPlayerIds.Count == 1)
        {
            rightTeamPlayerIds.Add(playerIndex);
            SetPlayerUiImage(playerIndex, ReturnPlayerUiImage(playerIndex).sprite, attackerRightTeamCircleSprite);
        }

        SetPlayerUiImageTransparency(playerIndex, 0.5f);
        playerCurrentPosition[playerIndex] = "right";
        SetMatchType();
    }

    private void HandleReturnToMiddle(int playerIndex, List<int> teamPlayerIds, Image defenderCircleSprite, Image attackerCircleSprite)
    {
        if (!teamPlayerIds.Contains(playerIndex))
        {
            return;
        }

        if (teamPlayerIds.Count == 1)
        {
            teamPlayerIds.Remove(playerIndex);
            SetPlayerUiImage(playerIndex, comCircleSprite, defenderCircleSprite, attackerCircleSprite);
        }
        else if (teamPlayerIds.Count == 2)
        {
            int remainingPlayerIndex = teamPlayerIds.First(id => id != playerIndex);
            teamPlayerIds.Remove(playerIndex);

            if (teamPlayerIds[0] == remainingPlayerIndex)
            {
                SetPlayerUiImage(remainingPlayerIndex, ReturnPlayerUiImage(remainingPlayerIndex).sprite, defenderCircleSprite, attackerCircleSprite);
            }
        }

        SetPlayerUiImageTransparency(playerIndex, 1f);
        playerCurrentPosition[playerIndex] = "mid";
        SetMatchType();
    }

    #endregion

    #region UI Management

    private void SetPlayerUiImage(int playerIndex, Sprite sprite, Image defenderCircleSprite, Image attackerCircleSprite)
    {
        defenderCircleSprite.sprite = sprite;
        attackerCircleSprite.sprite = sprite;
    }

    private void SetPlayerUiImage(int playerIndex, Sprite sprite, Image circleSprite)
    {
        circleSprite.sprite = sprite;
    }

    private void SetPlayerUiImageTransparency(int playerIndex, float transparency)
    {
        Image playerImage = ReturnPlayerUiImage(playerIndex);
        if (playerImage != null)
        {
            playerImage.color = new Color(1f, 1f, 1f, transparency);
        }
    }

    private Image ReturnPlayerUiImage(int playerNumber)
    {
        switch (playerNumber)
        {
            case 0: return playerOneCircleSprite;
            case 1: return playerTwoCircleSprite;
            case 2: return playerThreeCircleSprite;
            case 3: return playerFourCircleSprite;
            default: return null;
        }
    }

    private void SetMatchType()
    {
        string matchType;

        if (leftTeamPlayerIds.Count == 0 && rightTeamPlayerIds.Count == 0)
        {
            matchType = "Select a team";
        }
        else if (leftTeamPlayerIds.Count == 1 && rightTeamPlayerIds.Count == 0)
        {
            matchType = "1 vs COM";
        }
        else if (leftTeamPlayerIds.Count == 0 && rightTeamPlayerIds.Count == 1)
        {
            matchType = "COM vs 1";
        }
        else if (leftTeamPlayerIds.Count == 1 && rightTeamPlayerIds.Count == 1)
        {
            matchType = "1 vs 1";
        }
        else if (leftTeamPlayerIds.Count == 2 && rightTeamPlayerIds.Count == 0)
        {
            matchType = "2 vs COM";
        }
        else if (leftTeamPlayerIds.Count == 0 && rightTeamPlayerIds.Count == 2)
        {
            matchType = "COM vs 2";
        }
        else if (leftTeamPlayerIds.Count == 2 && rightTeamPlayerIds.Count == 1)
        {
            matchType = "2 vs 1";
        }
        else if (leftTeamPlayerIds.Count == 1 && rightTeamPlayerIds.Count == 2)
        {
            matchType = "1 vs 2";
        }
        else if (leftTeamPlayerIds.Count == 2 && rightTeamPlayerIds.Count == 2)
        {
            matchType = "2 vs 2";
        }
        else
        {
            matchType = "Select a team";
        }

        matchTypeText.text = matchType;
    }

    #endregion

    #region Game Start Logic

    /// <summary>
    /// Called by PlayerController.OnAccept() when a player presses their accept key.
    /// Shows the start button if at least one team has players.
    /// </summary>
    public void OnPlayerAccept(int playerIndex)
    {
        if (!isPlayerActive[playerIndex]) return;

        if (leftTeamPlayerIds.Count > 0 || rightTeamPlayerIds.Count > 0)
        {
            StartGameButton.gameObject.SetActive(true);
        }
        else
        {
            matchTypeText.text = "Select a team side";
        }
    }

    /// <summary>
    /// Checks if controls are assigned and sends them to MatchInfo
    /// </summary>
    public bool AreControlsAssigned()
    {
        AssignControlsToMatchInfoClass();
        return leftTeamPlayerIds.Count > 0 || rightTeamPlayerIds.Count > 0;
    }

    private void AssignControlsToMatchInfoClass()
    {
        foreach (PlayerInput playerInput in PlayersInputController.instance.playerInputs)
        {
            if (leftTeamPlayerIds.Contains(playerInput.playerIndex))
            {
                MatchInfo.instance.leftControllers.Add(playerInput);
            }
            else if (rightTeamPlayerIds.Contains(playerInput.playerIndex))
            {
                MatchInfo.instance.rightControllers.Add(playerInput);
            }
        }
    }

    #endregion

    #region Testing scene

    [Header("Only for testing scene")]
    public GameObject leftTeam;
    public GameObject rightTeam;

    /// <summary>
    /// Shows the start button if at least one team has players
    /// </summary>
    public void ShowStartButton()
    {
        if (leftTeamPlayerIds.Count > 0 || rightTeamPlayerIds.Count > 0)
        {
            StartGameButton.gameObject.SetActive(true);
        }
        else
        {
            matchTypeText.text = "Select a team side";
        }
    }

    /// <summary>
    /// Hides the start button
    /// </summary>
    public void HideStartButton()
    {
        StartGameButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Starts the game by assigning controls and hiding the panel
    /// </summary>
    public void StartGame()
    {
        isWaitingForJoin = false;
        AssignControlsToTeams();
        GameObject.Find("MatchController").GetComponent<MatchController>().enabled = true;
        gameObject.SetActive(false);
    }

    private void AssignControlsToTeams()
    {
        var playerInputs = PlayersInputController.instance.playerInputs.ToList();

        AssignControlsToTeam(leftTeam, leftTeamPlayerIds, playerInputs);
        AssignControlsToTeam(rightTeam, rightTeamPlayerIds, playerInputs);

        leftTeam.gameObject.SetActive(true);
        rightTeam.gameObject.SetActive(true);
    }

    private void AssignControlsToTeam(GameObject team, List<int> teamPlayerIds, List<PlayerInput> playerInputs)
    {
        if (team == null || teamPlayerIds.Count == 0)
        {
            team.GetComponent<TeamRodsController>().enabled = false;
            return;
        }
        team.GetComponent<AITeamRodsController>().enabled = false;

        TeamRodsController TeamRodsController = team.GetComponent<TeamRodsController>();
        if (TeamRodsController == null) return;

        TeamRodsController.playersAssignedToThisTeamSide = teamPlayerIds.Count;

        List<PlayerInput> teamInputs = new List<PlayerInput>();
        foreach (PlayerInput input in playerInputs)
        {
            if (teamPlayerIds.Contains(input.playerIndex))
            {
                teamInputs.Add(input);
            }
        }

        if (teamInputs.Count == 1)
        {
            TeamRodsController.defensePlayerInput = teamInputs[0];
            TeamRodsController.attackerPlayerInput = teamInputs[0];
        }
        else if (teamInputs.Count >= 2)
        {
            TeamRodsController.defensePlayerInput = teamInputs[0];
            TeamRodsController.attackerPlayerInput = teamInputs[1];
        }
    }
    #endregion
}
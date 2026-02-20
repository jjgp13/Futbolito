using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Manages team and player assignments in the game setup screen
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

    private Sprite comCircleSprite;

    #region Unity Lifecycle Methods

    private void Awake()
    {
        InitializeSingleton();
        InitializeTeamLists();
        SaveComSpriteReference();
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

    private void SaveComSpriteReference()
    {
        // Save COM sprite for later use
        comCircleSprite = defenderleftTeamCircleSprite.sprite;
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

        // Get the player's current UI image
        Image playerUI = ReturnPlayerUiImage(playerNumber);
        if (playerUI != null)
        {
            playerUI.color = new Color(1f, 1f, 1f, 0.5f);
        }

        // Update player state
        isPlayerActive[playerNumber] = false;

        // Remove player from teams if assigned
        RemovePlayerFromTeams(playerNumber);
    }

    private void RemovePlayerFromTeams(int playerNumber)
    {
        // If player was on a team, remove them
        if (leftTeamPlayerIds.Contains(playerNumber))
        {
            HandleReturnToMiddle(playerNumber, leftTeamPlayerIds, defenderleftTeamCircleSprite, attackerleftTeamCircleSprite);
        }
        else if (rightTeamPlayerIds.Contains(playerNumber))
        {
            HandleReturnToMiddle(playerNumber, rightTeamPlayerIds, defenderRightTeamCircleSprite, attackerRightTeamCircleSprite);
        }

        // Update match type display
        SetMatchType();
    }

    /// <summary>
    /// Checks if player is active and activates their UI
    /// </summary>
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
        // Check if team is full
        if (leftTeamPlayerIds.Count >= 2)
        {
            Debug.LogWarning("Left team is already full");
            return;
        }

        // First player on team controls defense positions
        if (leftTeamPlayerIds.Count == 0)
        {
            leftTeamPlayerIds.Add(playerIndex);
            SetPlayerUiImage(playerIndex, ReturnPlayerUiImage(playerIndex).sprite, defenderleftTeamCircleSprite, attackerleftTeamCircleSprite);
        }
        // Second player on team controls attack positions
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
        // Check if team is full
        if (rightTeamPlayerIds.Count >= 2)
        {
            Debug.LogWarning("Right team is already full");
            return;
        }

        // First player on team controls defense positions
        if (rightTeamPlayerIds.Count == 0)
        {
            rightTeamPlayerIds.Add(playerIndex);
            SetPlayerUiImage(playerIndex, ReturnPlayerUiImage(playerIndex).sprite, defenderRightTeamCircleSprite, attackerRightTeamCircleSprite);
        }
        // Second player on team controls attack positions
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

        // Handle single player on team
        if (teamPlayerIds.Count == 1)
        {
            teamPlayerIds.Remove(playerIndex);
            SetPlayerUiImage(playerIndex, comCircleSprite, defenderCircleSprite, attackerCircleSprite);
        }
        // Handle two players on team
        else if (teamPlayerIds.Count == 2)
        {
            // Get the remaining player's sprite
            int remainingPlayerIndex = teamPlayerIds.First(id => id != playerIndex);
            teamPlayerIds.Remove(playerIndex);

            // If removing the first player, update UI accordingly
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

    /// <summary>
    /// Updates UI with player sprite
    /// </summary>
    private void SetPlayerUiImage(int playerIndex, Sprite sprite, Image defenderCircleSprite, Image attackerCircleSprite)
    {
        defenderCircleSprite.sprite = sprite;
        attackerCircleSprite.sprite = sprite;
    }

    /// <summary>
    /// Updates single UI element with player sprite
    /// </summary>
    private void SetPlayerUiImage(int playerIndex, Sprite sprite, Image circleSprite)
    {
        circleSprite.sprite = sprite;
    }

    /// <summary>
    /// Sets transparency of player UI element
    /// </summary>
    private void SetPlayerUiImageTransparency(int playerIndex, float transparency)
    {
        Image playerImage = ReturnPlayerUiImage(playerIndex);
        if (playerImage != null)
        {
            playerImage.color = new Color(1f, 1f, 1f, transparency);
        }
    }

    /// <summary>
    /// Returns the UI image for the specified player
    /// </summary>
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

    /// <summary>
    /// Sets match type text based on team compositions
    /// </summary>
    private void SetMatchType()
    {
        string matchType;

        // Determine match type based on team compositions
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
    /// Checks if controls are assigned and sends them to MatchInfo
    /// </summary>
    public bool AreControlsAssigned()
    {
        AssignControlsToMatchInfoClass();
        return leftTeamPlayerIds.Count > 0 || rightTeamPlayerIds.Count > 0;
    }

    /// <summary>
    /// Assigns controllers to the MatchInfo class
    /// </summary>
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
        AssignControlsToTeams();
        //Find GameController script and start the game
        GameObject.Find("MatchController").GetComponent<MatchController>().enabled = true;
        //Deactivate this panel
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Assigns player inputs to team TeamRodsControllers
    /// </summary>
    private void AssignControlsToTeams()
    {
        // Get all player inputs
        var playerInputs = PlayersInputController.instance.playerInputs.ToList();

        // Handle left team assignments
        AssignControlsToTeam(leftTeam, leftTeamPlayerIds, playerInputs);

        // Handle right team assignments
        AssignControlsToTeam(rightTeam, rightTeamPlayerIds, playerInputs);

        // Activate teams
        leftTeam.gameObject.SetActive(true);
        rightTeam.gameObject.SetActive(true);
    }

    /// <summary>
    /// Assigns player inputs to a specific team
    /// </summary>
    private void AssignControlsToTeam(GameObject team, List<int> teamPlayerIds, List<PlayerInput> playerInputs)
    {
        if (team == null || teamPlayerIds.Count == 0)
        {
            //It means that the team is not being controlled by a player
            team.GetComponent<TeamRodsController>().enabled = false;
            return;
        }
        //it means that the team is being controlled by a player
        team.GetComponent<AITeamRodsController>().enabled = false;

        TeamRodsController TeamRodsController = team.GetComponent<TeamRodsController>();
        if (TeamRodsController == null) return;

        // Set the number of players on this team
        TeamRodsController.playersAssignedToThisTeamSide = teamPlayerIds.Count;

        // Collect player inputs for this team
        List<PlayerInput> teamInputs = new List<PlayerInput>();
        foreach (PlayerInput input in playerInputs)
        {
            if (teamPlayerIds.Contains(input.playerIndex))
            {
                teamInputs.Add(input);
            }
        }

        // Assign inputs based on team size
        if (teamInputs.Count == 1)
        {
            // Single player controls both defense and offense
            TeamRodsController.defensePlayerInput = teamInputs[0];
            TeamRodsController.attackerPlayerInput = teamInputs[0];
        }
        else if (teamInputs.Count >= 2)
        {
            // First player controls defense, second player controls offense
            TeamRodsController.defensePlayerInput = teamInputs[0];
            TeamRodsController.attackerPlayerInput = teamInputs[1];
        }
    }
    #endregion
}
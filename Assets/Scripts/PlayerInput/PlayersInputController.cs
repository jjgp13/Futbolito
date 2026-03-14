using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayersInputController : MonoBehaviour
{
    public static PlayersInputController instance;

    public PlayerInputManager playerInputManager;
    public List<PlayerInput> playerInputs;

    public const int MaxPlayers = 4;
    public const string SchemeKeyboardP1 = "Keyboard_P1";
    public const string SchemeKeyboardP2 = "Keyboard_P2";
    public const string SchemeXbox = "Xbox";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        playerInputManager = GetComponent<PlayerInputManager>();
        playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;

        playerInputs = new List<PlayerInput>();

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        playerInputManager.onPlayerJoined += OnPlayerJoined;
        playerInputManager.onPlayerLeft += OnPlayerLeft;
    }

    private void OnDisable()
    {
        playerInputManager.onPlayerJoined -= OnPlayerJoined;
        playerInputManager.onPlayerLeft -= OnPlayerLeft;
    }

    /// <summary>
    /// Manually join a player with a specific control scheme and device.
    /// Uses PlayerInput.Instantiate instead of PlayerInputManager.JoinPlayer
    /// to preserve the actions asset from the prefab.
    /// </summary>
    public PlayerInput JoinPlayerManually(string controlScheme, InputDevice device)
    {
        if (playerInputs.Count >= MaxPlayers)
        {
            Debug.LogWarning("Cannot join: max players reached.");
            return null;
        }

        if (IsSchemeAlreadyJoined(controlScheme))
        {
            Debug.LogWarning($"Cannot join: scheme '{controlScheme}' is already in use.");
            return null;
        }

        // Use PlayerInput.Instantiate instead of PlayerInputManager.JoinPlayer
        // because JoinPlayer strips the actions asset from the prefab.
        // Instantiate preserves it and still fires onPlayerJoined on the manager.
        PlayerInput newPlayer = PlayerInput.Instantiate(
            playerInputManager.playerPrefab,
            controlScheme: controlScheme,
            pairWithDevices: new InputDevice[] { device }
        );

        if (newPlayer == null)
        {
            Debug.LogError($"PlayerInput.Instantiate failed for scheme '{controlScheme}'");
            return null;
        }

        if (newPlayer.actions == null)
        {
            Debug.LogError($"Player {newPlayer.playerIndex}: actions is NULL after Instantiate!");
        }
        else
        {
            Debug.Log($"Player {newPlayer.playerIndex}: actions='{newPlayer.actions.name}', " +
                      $"scheme='{newPlayer.currentControlScheme}', " +
                      $"actionMap='{newPlayer.currentActionMap?.name}'");
        }

        return newPlayer;
    }

    /// <summary>
    /// Checks if a given control scheme already has a joined player.
    /// </summary>
    public bool IsSchemeAlreadyJoined(string controlScheme)
    {
        return playerInputs.Any(p =>
            p.currentControlScheme == controlScheme);
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        Debug.Log($"Player {playerInput.playerIndex + 1} joined with scheme: {playerInput.currentControlScheme}");
        playerInput.gameObject.name = $"Player {playerInput.playerIndex + 1}";
        playerInputs.Add(playerInput);
        playerInput.transform.SetParent(gameObject.transform);

        if (GameControlsConfigPanel.instance != null)
            GameControlsConfigPanel.instance.PlayerJoinedUiActivation(playerInput.playerIndex);
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        Debug.Log($"Player {playerInput.playerIndex + 1} left!");
        playerInputs.Remove(playerInput);
        Destroy(playerInput.gameObject);

        if (GameControlsConfigPanel.instance != null)
            GameControlsConfigPanel.instance.PlayerLeftWhileThisPanelIsActive(playerInput.playerIndex);
    }
}

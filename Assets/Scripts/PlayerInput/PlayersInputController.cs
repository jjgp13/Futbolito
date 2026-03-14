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
    /// Returns the created PlayerInput, or null if join was rejected.
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

        return playerInputManager.JoinPlayer(
            controlScheme: controlScheme,
            pairWithDevice: device
        );
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

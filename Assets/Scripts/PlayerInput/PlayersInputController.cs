using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayersInputController : MonoBehaviour
{
    //Singleton
    public static PlayersInputController instance;

    // Reference to the PlayerInputManager component
    public PlayerInputManager playerInputManager;

    public List<PlayerInput> playerInputs;

    private void Awake()
    {
        if (instance == null) instance = this;
        
        // Get the PlayerInputManager component
        playerInputManager = GetComponent<PlayerInputManager>();
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // Subscribe to the PlayerJoinedEvent
        playerInputManager.onPlayerJoined += OnPlayerJoined;
        playerInputManager.onPlayerLeft += OnPlayerLeft;
    }

    private void OnDisable()
    {
        // Unsubscribe when the script is disabled
        playerInputManager.onPlayerJoined -= OnPlayerJoined;
        playerInputManager.onPlayerLeft += OnPlayerLeft;
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        // Handle player joined event
        Debug.Log($"Player {playerInput.playerIndex + 1} joined!");
        playerInput.gameObject.name = $"Player {playerInput.playerIndex + 1}";
        playerInputs.Add(playerInput);
        playerInput.transform.SetParent(gameObject.transform);

        // Check if the GameControlsConfigPanel is active,
        if (GameControlsConfigPanel.instance != null)
            GameControlsConfigPanel.instance.PlayerJoinedUiActivation(playerInput.playerIndex);

        
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        // Handle player left event
        Debug.Log($"Player {playerInput.playerIndex + 1} left!");
        Destroy(playerInput.gameObject);

        // Check if the GameControlsConfigPanel is active,
        if (GameControlsConfigPanel.instance != null)
            GameControlsConfigPanel.instance.PlayerLeftWhileThisPanelIsActive(playerInput.playerIndex);
    }



}

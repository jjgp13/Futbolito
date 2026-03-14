using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;
    private bool initialized;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void LateUpdate()
    {
        // Skip input processing on the frame this object was created
        if (!initialized) { initialized = true; return; }
    }

    public void OnLeftTrigger()
    {
        if (!initialized) return;
        if (GameControlsConfigPanel.instance != null)
            GameControlsConfigPanel.instance.CheckInputToChooseTeamSide(playerInput.playerIndex, "left");
    }

    public void OnRightTrigger()
    {
        if (!initialized) return;
        if (GameControlsConfigPanel.instance != null)
            GameControlsConfigPanel.instance.CheckInputToChooseTeamSide(playerInput.playerIndex, "right");
    }

    public void OnAccept()
    {
        if (!initialized) return;
        if (GameControlsConfigPanel.instance != null)
            GameControlsConfigPanel.instance.OnPlayerAccept(playerInput.playerIndex);
    }
}
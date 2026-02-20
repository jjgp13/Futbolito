using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }
    
    public void OnLeftTrigger()
    {
        if (GameControlsConfigPanel.instance != null)
            GameControlsConfigPanel.instance.CheckInputToChooseTeamSide(playerInput.playerIndex, "left");
    }

    public void OnRightTrigger()
    {
        if (GameControlsConfigPanel.instance != null)
            GameControlsConfigPanel.instance.CheckInputToChooseTeamSide(playerInput.playerIndex, "right");
    }

    public void OnAccept()
    {
        if (GameControlsConfigPanel.instance != null)
            GameControlsConfigPanel.instance.ShowStartButton();
    }
}
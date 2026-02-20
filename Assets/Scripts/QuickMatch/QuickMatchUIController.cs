using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class QuickMatchUIController : MonoBehaviour
{

    public static QuickMatchUIController instance;

    [Header("Input System Reference")]
    public InputActionAsset actions;
    private InputAction startTeamSelection;

    [Header("Reference for panels in the scene")]
    public GameObject gameControlsConfigPanel;
    public GameObject teamSelectionPanel;
    public GameObject settingsPanel;

    [Header("Check which panel is showed in scene")]
    private bool isGameControlsConfigPanelActive;
    private bool isTeamSelectionPanelActive;
    //private bool isSettingsPanelActive;

    [Header("First elements selected")]
    public Button firstTeam;
    public Button uniform;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        isGameControlsConfigPanelActive =  true;
        isTeamSelectionPanelActive = false;
        //isSettingsPanelActive = false;

        gameControlsConfigPanel.SetActive(true);
        teamSelectionPanel.SetActive(false);
        settingsPanel.SetActive(false);

        startTeamSelection = actions.FindActionMap("UI").FindAction("AddPlayer");
    }

    private void OnEnable()
    {
        actions.FindActionMap("UI").Enable();
        startTeamSelection.performed += OnStartTeamSelectionPerformed;
    }

    private void OnDisable()
    {
        startTeamSelection.performed -= OnStartTeamSelectionPerformed;
        actions.FindActionMap("UI").Disable();
    }

    private void OnStartTeamSelectionPerformed(InputAction.CallbackContext context)
    {
        if (isGameControlsConfigPanelActive)
        {
            HandleGameControlsConfigPanel();
        }
        else if (isTeamSelectionPanelActive)
        {
            HandleTeamSelectionPanel();
        }
    }

    private void HandleGameControlsConfigPanel()
    {
        if (GameControlsConfigPanel.instance.AreControlsAssigned())
        {
            isGameControlsConfigPanelActive = false;
            isTeamSelectionPanelActive = true;

            gameControlsConfigPanel.SetActive(false);
            teamSelectionPanel.SetActive(true);
        }
        else
        {
            Debug.Log("No controls assigned");
            // TODO: UI feedback, sounds feedback
        }
    }

    private void HandleTeamSelectionPanel()
    {
        if (MatchInfo.instance.leftTeam != null && MatchInfo.instance.rightTeam != null)
        {
            isTeamSelectionPanelActive = false;
            //isSettingsPanelActive = true;

            teamSelectionPanel.SetActive(false);
            settingsPanel.SetActive(true);

            uniform.Select();
        }
        else
        {
            Debug.Log("Select teams");
        }
    }

    private void HandleSettingsPanel()
    {
        
    }

    public void SwitchUIControls()
    {
        // Implementation for switching UI controls
    }

    public void ChangeToNextPanel(string panel)
    {
        if (panel == "SelectionPanel") firstTeam.Select();
        if (panel == "OptionsPanel") uniform.Select();
    }

    public void ChangeScene(string sceneName)
    {
        if (sceneName == "MainMenu")
        {
            Destroy(GameObject.Find("MatchInfo"));
            Destroy(GameObject.FindGameObjectWithTag("PlayerDataObject"));
        }
        SceneManager.LoadScene(sceneName);
    }
}

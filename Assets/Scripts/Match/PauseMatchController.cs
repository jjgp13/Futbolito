using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMatchController : MonoBehaviour
{

    [Header("Pause panel objects")]
    public GameObject mainPausePanel;
    //Paused, Victory, tie, defeat
    public Text matchStatus;
    public Text matchType;

    public GameObject pauseMatchPanelOptions;
    

    // Start is called before the first frame update
    void Start()
    {
        mainPausePanel.SetActive(true);

        //Hide-show UI panels
        pauseMatchPanelOptions.SetActive(true);
        mainPausePanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// This method will resume the match, if the match is in pause state.
    /// </summary>
    public void Resume()
    {
        mainPausePanel.SetActive(false);
        Time.timeScale = 1f;
        MatchController._matchController.gameIsPaused = false;
    }

    /// <summary>
    /// This method will pause the match, if the match is in playing state.
    /// </summary>
    public void Pause()
    {
        mainPausePanel.SetActive(true);
        Time.timeScale = 0f;
        MatchController._matchController.gameIsPaused = true;
    }
}

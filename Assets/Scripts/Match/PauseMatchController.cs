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

    [Header("Audio Settings (assign in Inspector or created at runtime)")]
    public Slider masterVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider crowdVolumeSlider;

    // Start is called before the first frame update
    void Start()
    {
        mainPausePanel.SetActive(true);

        //Hide-show UI panels
        pauseMatchPanelOptions.SetActive(true);
        mainPausePanel.SetActive(false);

        // Initialize audio sliders
        InitializeAudioSliders();
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
        MatchController.instance.gameIsPaused = false;
    }

    /// <summary>
    /// This method will pause the match, if the match is in playing state.
    /// </summary>
    public void Pause()
    {
        mainPausePanel.SetActive(true);
        Time.timeScale = 0f;
        MatchController.instance.gameIsPaused = true;
    }

    #region Audio Settings

    private void InitializeAudioSliders()
    {
        float master = PlayerPrefs.GetFloat("AudioMasterVolume", 1f);
        float sfx = PlayerPrefs.GetFloat("AudioSFXVolume", 1f);
        float crowd = PlayerPrefs.GetFloat("AudioCrowdVolume", 1f);

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = master;
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfx;
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        if (crowdVolumeSlider != null)
        {
            crowdVolumeSlider.value = crowd;
            crowdVolumeSlider.onValueChanged.AddListener(OnCrowdVolumeChanged);
        }
    }

    private void OnMasterVolumeChanged(float value)
    {
        if (MatchAudioManager.instance != null)
            MatchAudioManager.instance.SetMasterVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (MatchAudioManager.instance != null)
            MatchAudioManager.instance.SetSFXVolume(value);
    }

    private void OnCrowdVolumeChanged(float value)
    {
        if (MatchAudioManager.instance != null)
            MatchAudioManager.instance.SetCrowdVolume(value);
    }

    #endregion
}

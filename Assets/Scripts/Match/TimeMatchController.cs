using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeMatchController : MonoBehaviour
{
    [Header("Time panel")]
    public GameObject timePanel;
    public Text timeText;
    private float timer;
    private bool timerActive;

    void Start()
    {
        // Timer is inert until ResetTimer() is called from MatchController.InitAnimation
        timerActive = false;
    }

    /// <summary>
    /// Initializes (or reinitializes) the countdown timer.
    /// Called by MatchController.InitAnimation right before ball spawn
    /// so the value always reflects the latest MatchInfo / AutoMatchRunner setting.
    /// </summary>
    public void ResetTimer()
    {
        int minutes;
        if (AutoMatchRunner.Instance != null)
            minutes = AutoMatchRunner.Instance.MatchTimeMinutes;
        else if (MatchInfo.instance != null)
            minutes = MatchInfo.instance.matchTime;
        else
            minutes = 5;

        timer = minutes * 60;
        timeText.text = string.Format("{0}:00", minutes);
        timerActive = true;
    }

    void Update()
    {
        if (!timerActive) return;

        if (timer <= 0)
        {
            timerActive = false;
            MatchController.instance.endMatch = true;
            timeText.text = "FINISH";
            var anim = timeText.GetComponent<Animator>();
            if (anim != null) anim.SetBool("Warning", false);
            return;
        }

        timer -= Time.deltaTime;

        string minutes = Mathf.Floor(timer / 60).ToString("00");
        string seconds = (timer % 60).ToString("00");
        timeText.text = string.Format("{0}:{1}", minutes, seconds);

        if (int.Parse(minutes) == 0 && int.Parse(seconds) <= 20)
        {
            var anim = timeText.GetComponent<Animator>();
            if (anim != null) anim.SetBool("Warning", true);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeMatchController : MonoBehaviour
{
    //Variables that manage the time elapsed in the match
    [Header("Time panel")]
    public GameObject timePanel;
    public Text timeText;
    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        //Set time
        timer = MatchInfo._matchInfo.matchTime * 59;
        timeText.text = string.Format("{0}:00", MatchInfo._matchInfo.matchTime.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        CheckMatchTime();
        DecreaseMatchTime();

    }

    private void CheckMatchTime()
    {
        if (timer <= 0)
        {
            MatchController._matchController.endMatch = true;
            timeText.text = "FINISH";
            timeText.GetComponent<Animator>().SetBool("Warning", false);
        }
    }

    private void DecreaseMatchTime()
    {
        if (timer > 0 && MatchController._matchController.ballInGame)
        {
            timer -= Time.deltaTime;

            string minutes = Mathf.Floor(timer / 60).ToString("00");
            string seconds = (timer % 60).ToString("00");
            timeText.text = string.Format("{0}:{1}", minutes, seconds);

            if (int.Parse(minutes) == 0 && int.Parse(seconds) <= 20) timeText.GetComponent<Animator>().SetBool("Warning", true);
        }
    }

}

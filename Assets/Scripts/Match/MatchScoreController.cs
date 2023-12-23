using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchScoreController : MonoBehaviour
{
    public static MatchScoreController instance;

    //Variables that handle the score in the match
    public int LeftTeamScore { get; set; }
    public int RightTeamScore { get; set; }


    public Text matchScore;

    [Header("Goal Animation objects")]
    public GameObject leftTeamScore_UI;
    public GameObject rightTeamScore_UI;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        LeftTeamScore = 0;
        RightTeamScore = 0;
    }
    
    /// <summary>
    /// This method update the score in the current match.
    /// </summary>
    /// <param name="golName">Who score?</param>
    public void AdjustScore(string golName)
    {
        //Play goal sound
        GetComponent<SoundMatchController>().PlayGolSound();
        if (golName == "RightGoalTrigger")
        {
            //Increase score
            LeftTeamScore++;
            //PlayerDataController.playerData.goalsScored++;
            //Change color of Balls in goals UI
            Animator anim = leftTeamScore_UI.transform.GetChild(LeftTeamScore).GetComponent<Animator>();
            anim.SetTrigger("Goal");
        }
        else if (golName == "LeftGoalTrigger")
        {
            RightTeamScore++;
            //PlayerDataController.playerData.goalsAgainst++;
            Animator anim = rightTeamScore_UI.transform.GetChild(RightTeamScore).GetComponent<Animator>();
            anim.SetTrigger("Goal");
        }
        //Update pause panel text
        UpdateUIScore();
        //Check if score is 5
        CheckScore();
    }

    /// <summary>
    /// This method checks if score is equal to 5 for everyone
    /// If not continue spawing balls
    /// </summary>
    public void CheckScore()
    {
        //play end animation with knockout.
        if (LeftTeamScore == 5 || RightTeamScore == 5)
        {
            StartCoroutine(MatchController._matchController.PlayEndMatchAnimation(true));
            return;
        }

        MatchController._matchController.SpawnBall();
    }

    /// <summary>
    /// Update pause panel score.
    /// </summary>
    public void UpdateUIScore()
    {
        matchScore.text = LeftTeamScore.ToString() + "-" + RightTeamScore.ToString();
    }
}

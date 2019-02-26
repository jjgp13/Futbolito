﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// This class is the one that handles all events in GameMatch scene.
/// </summary>
public class MatchController : MonoBehaviour {

    //Singleton
    public static MatchController _matchController;
    
    //Reference to ball in field
    public GameObject ball;
    public bool gameIsPaused;
    public bool ballInGame;

    //Variables to hanlde bullet time
    [Header("Variables for bullet time")]
    public float slowDownTime;
    public float timesSlower;
    public bool bulletTime;
    private float bulletTimeTimer;

    //Reference to camera and animator (also bullet time)
    public GameObject cam;
    private Animator camAnimator;

    [Header("Scriptable objects with teams information")]
    //Reference to teams that are on this match
    public Team playerTeam;
    public Team npcTeam;

    [Header("Initial animation UI objects")]
    //Reference to the object that handle the initial animation.
    public GameObject intialAnimationObject;
    public Text matchTypeText;

    [Header("Pause panel objects")]
    public GameObject mainPausePanel;
    //Paused, Victory, tie, defeat
    public Text matchStatus;
    public Text matchType;
    public Text matchScore;

    public GameObject pauseMatchPanelOptions;
    public Text timeLeftOnPausePanel;
    public GameObject finishQuickMatchPanelOptions;
    public GameObject finishTourMatchPanelOptions;
    

    [Header("Final match animation objects")]
    public GameObject finalMatchPanel;
    public Text finalMatchStatusText;

    [Header("Goal Animation objects")]
    public GameObject golAnimation_UI;
    public GameObject playerScore_UI;
    public GameObject NPCScore_UI;

    [Header("Player UI buttons")]
    public GameObject holding_UI;
    public GameObject shooting_UI;
    public GameObject pauseBtn_UI;

    //Variables that manage the time elapsed in the match
    [Header("Time panel")]
    public GameObject timePanel;
    public Text timeText;
    private float timer;
    private bool endMatch;

    private int roundMatch;
    

    [Header("Restart ball Panel")]
    //Reference to the panel that counts when the ball is inactive and it should be restarted.
    public GameObject timeInactiveBallPanel;
    public Text restartingBallTimeText;
    
    //Variables that handle the score in the match
    private int playerScore;
    public int PlayerScore
    {
        get
        {
            return playerScore;
        }
    }
    private int NPCScore;
    public int NPC_Score
    {
        get
        {
            return NPCScore;
        }
    }

    //Singleton, reference to this script and its info.
    private void Awake()
    {
        _matchController = this;
        //Show action buttons
        SetUIState(true);
    }

    //Initial state of objects.
    private void Start()
    {
        

        //Start initial animation.
        StartCoroutine(InitAnimation());

        //Start score at 0 and game as playing
        playerScore = 0;
        NPCScore = 0;
        gameIsPaused = false;

        //Set time
        timer = MatchInfo._matchInfo.matchTime * 59;
        timeText.text = string.Format("{0}:00", MatchInfo._matchInfo.matchTime.ToString()); 
        ballInGame = false;
        endMatch = false;

        //Reference to player and NPC Game objects to pull info
        playerTeam = MatchInfo._matchInfo.playerTeam;
        npcTeam = MatchInfo._matchInfo.comTeam;

        //Active these panels to assign flags
        golAnimation_UI.SetActive(true);
        mainPausePanel.SetActive(true);
        

        //Set UI (flags and team names)
        SetTeamFlags("PlayerFlags", playerTeam.flag, playerTeam.teamName);
        SetTeamFlags("ComFlags", npcTeam.flag, npcTeam.teamName);

        //Hide-show UI panels
        pauseMatchPanelOptions.SetActive(true);
        finishQuickMatchPanelOptions.SetActive(false);
        mainPausePanel.SetActive(false);
        golAnimation_UI.SetActive(false);

        camAnimator = cam.GetComponent<Animator>();
        bulletTime = false;
        bulletTimeTimer = 0;
    }

    private void Update()
    {
        if(timer > 0 && ballInGame)
        {
            timer -= Time.deltaTime;

            string minutes = Mathf.Floor(timer / 60).ToString("00");
            string seconds = (timer % 60).ToString("00");
            timeText.text = string.Format("{0}:{1}", minutes, seconds);
            
            if (int.Parse(minutes) == 0 && int.Parse(seconds) <= 20) timeText.GetComponent<Animator>().SetBool("Warning", true);
        }

        if (bulletTime)
        {
            bulletTimeTimer += Time.unscaledDeltaTime;
            if(bulletTimeTimer > slowDownTime)
            {
                cam.transform.position = new Vector3(0, 0, -20);
                Time.timeScale = 1f;
                bulletTime = false;
                bulletTimeTimer = 0;
                camAnimator.SetBool("BulletTime", false);
            }
        }

        //This handle the time remaing in match
        if (timer <= 0)
        {
            endMatch = true;
            timeText.text = "FINISH";
            timeText.GetComponent<Animator>().SetBool("Warning", false);
        }

        //if time has finished start end animation 
        if (endMatch)
        {
            timer = 1;
            StartCoroutine(PlayEndMatchAnimation(false));
            ballInGame = false;
            endMatch = false;
        }
    }

    /// <summary>
    /// This method update the score in the current match.
    /// </summary>
    /// <param name="golName">Who score?</param>
    public void AdjustScore(string golName)
    {
        //Play goal sound
        GetComponent<SoundMatchController>().PlayGolSound();
        if (golName == "PlayerGol")
        {
            //Increase score
            playerScore++;
            //Change color of Balls in goals UI
            playerScore_UI.transform.GetChild(playerScore-1).GetComponent<Image>().color = Color.white;
        }
        else if (golName == "NPCGol")
        {
            NPCScore++;
            NPCScore_UI.transform.GetChild(NPCScore-1).GetComponent<Image>().color = Color.white;
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
        if (playerScore == 5 || NPCScore == 5) StartCoroutine(PlayEndMatchAnimation(true));
        else SpawnBall();
    }

    /// <summary>
    /// Instatiate ball in game field
    /// </summary>
    public void SpawnBall()
    {
        Instantiate(ball, Vector2.zero, Quaternion.identity);
    }

    /// <summary>
    /// Play goal animation. Every time a goal is scored.
    /// </summary>
    /// <returns>Animation duration</returns>
    public IEnumerator GolAnimation()
    {
        golAnimation_UI.SetActive(true);
        //Hide Shoot and Hold buttons and pause panel.
        SetUIState(false);
        //Wait 4 seconds.
        yield return new WaitForSeconds(4f);
        //Hide Goal animation
        golAnimation_UI.SetActive(false);
        //If noabady has reached 5 goals show again buttons.
        if(playerScore < 5 && NPCScore < 5) SetUIState(true);
    }

    /// <summary>
    /// Hide or show shooting, hold and pause button.
    /// </summary>
    /// <param name="active">True or false.</param>
    public void SetUIState(bool active)
    {
        holding_UI.SetActive(active);
        shooting_UI.SetActive(active);
        pauseBtn_UI.SetActive(active);
    }

    /// <summary>
    /// Update pause panel score.
    /// </summary>
    public void UpdateUIScore()
    {
        matchScore.text = playerScore.ToString() + "-" + NPCScore.ToString();
    }

    /// <summary>
    /// Play end match animation.
    /// </summary>
    /// <param name="knockout">If a player has reTimeached 5 goals it is consider has knockout</param>
    /// <returns></returns>
    public IEnumerator PlayEndMatchAnimation(bool knockout)
    {
        //If match time has finished and ball is still in gamefield, stop and destroy.
        ball.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        Destroy(GameObject.FindGameObjectWithTag("Ball"));
        
        //Change text depending on game final status.
        if (knockout) finalMatchStatusText.text = "Knockout!";
        else finalMatchStatusText.text = "Time out!";
        
        //Activate final match animation.
        finalMatchPanel.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        finalMatchPanel.gameObject.SetActive(false);

        //Activate final match panel
        mainPausePanel.SetActive(true);
        //Change final match status
        if (playerScore > NPCScore)
        {
            matchStatus.text = "VICTORY!";
            matchStatus.color = Color.yellow;
        }
        else if (playerScore < NPCScore)
        {
            matchStatus.text = "DEFEAT";
            matchStatus.color = Color.red;
        }  
        else
            matchStatus.text = "TIE";

        //Deactivate pause panel shooting button, holding button.
        SetUIState(false);
        //Deactivate pause options
        pauseMatchPanelOptions.SetActive(false);
        //Activate differents options depending of match's type.
        if (MatchInfo._matchInfo.matchType == MatchInfo.MatchType.QuickMatch) finishQuickMatchPanelOptions.SetActive(true);
        if (MatchInfo._matchInfo.matchType == MatchInfo.MatchType.TourMatch) finishTourMatchPanelOptions.SetActive(true);
        
    }

    /// <summary>
    /// This method will search for game objects with the tag given and it will set the flag and the name of the team
    /// </summary>
    /// <param name="tag">Objects to search by tag</param>
    /// <param name="flag">Team flag</param>
    /// <param name="name">Team name</param>
    public void SetTeamFlags(string tag, Sprite flag, string name)
    {
        GameObject[] flags = GameObject.FindGameObjectsWithTag(tag);
        for (int i = 0; i < flags.Length; i++)
        {
            //Get the image and set the flag
            flags[i].GetComponent<Image>().sprite = flag;
            //Get its first child and set its name
            flags[i].transform.GetChild(0).GetComponent<Text>().text = name;
        }
    }

    /// <summary>
    /// This method will resume the match, if the match is in pause state.
    /// </summary>
    public void Resume()
    {
        mainPausePanel.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
        timePanel.SetActive(true);
        holding_UI.SetActive(true);
        shooting_UI.SetActive(true);
    }

    /// <summary>
    /// This method will pause the match, if the match is in playing state.
    /// </summary>
    public void Pause()
    {
        mainPausePanel.SetActive(true);
        timeLeftOnPausePanel.text = "Time left: \n" + timeText.text + " min";
        timePanel.SetActive(false);
        Time.timeScale = 0f;
        gameIsPaused = true;
        holding_UI.SetActive(false);
        shooting_UI.SetActive(false);
    }

    //Activate inital animation.
    IEnumerator InitAnimation()
    {
        //Change initial animation text, depending of the match type
        if (MatchInfo._matchInfo.matchType == MatchInfo.MatchType.QuickMatch)
        {
            matchTypeText.text = "Friendly";
            matchType.text = matchTypeText.text;
        }
        if (MatchInfo._matchInfo.matchType == MatchInfo.MatchType.TourMatch)
        {
            int roundMatch = TournamentController._tourCtlr.matchesRound + 1;
            matchTypeText.text = TournamentController._tourCtlr.tourName + "\n Match " + roundMatch.ToString();
            matchType.text = matchTypeText.text;
        }

        intialAnimationObject.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        intialAnimationObject.SetActive(false);
        timePanel.SetActive(true);
        //Instiatite a ball
        SpawnBall();
    }

    //Animation for bullet time hit.
    //Fix
    public void PlayBulletTimeAnimation(Vector2 pos)
    {
        cam.transform.position = new Vector3(pos.x, pos.y, -20);
        Time.timeScale = 0.02f;
        bulletTime = true;
        camAnimator.SetBool("BulletTime", true);
    }

    public IEnumerator BallHittedEffect()
    {
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(0.1f);
        Time.timeScale = 1;
    }

    /// <summary>
    /// This method changes the scene. If you're quiting the game, it will restart the timescale
    /// and then destroy this game object.
    /// </summary>
    /// <param name="sceneName">Name of the scene</param>
    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1;
        if (sceneName == "MainMenu" || sceneName == "QuickMatchMenu")
        {
            Destroy(GameObject.FindGameObjectWithTag("MatchData"));
            Destroy(GameObject.FindGameObjectWithTag("TourData"));
        }

        if(sceneName == "TourMainMenu")
        {
            //El bueno
            Destroy(GameObject.FindGameObjectWithTag("MatchData"));
            TournamentController._tourCtlr.SimulateRoundOfMatches(TournamentController._tourCtlr.matchesRound);
            TournamentController._tourCtlr.SaveTour();
        }

        SceneManager.LoadScene(sceneName);
    }
}
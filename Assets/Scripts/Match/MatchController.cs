using System.Collections;
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
    public Team leftTeam;
    public Team rightTeam;

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
    public GameObject finishQuickMatchPanelOptions;
    public GameObject finishTourMatchPanelOptions;
    

    [Header("Final match animation objects")]
    public GameObject finalMatchPanel;
    public Text finalMatchStatusText;

    [Header("Goal Animation objects")]
    public GameObject golAnimation_UI;
    public GameObject leftTeamScore_UI;
    public GameObject rightTeamScore_UI;

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
    public int LeftTeamScore { get; set; }
    public int RightTeamScore { get; set; }


    //Singleton, reference to this script and its info.
    private void Awake()
    {
        _matchController = this;
        //Show action buttons
        SetUIState(true);

        //Reference to player and NPC Game objects to pull info
        leftTeam = MatchInfo._matchInfo.leftTeam;
        rightTeam = MatchInfo._matchInfo.rightTeam;
    }


    //Initial state of objects.
    private void Start()
    {
        

        //Start initial animation.
        StartCoroutine(InitAnimation());

        //Start score at 0 and game as playing
        LeftTeamScore = 0;
        RightTeamScore = 0;
        gameIsPaused = false;

        //Set time
        timer = MatchInfo._matchInfo.matchTime * 59;
        timeText.text = string.Format("{0}:00", MatchInfo._matchInfo.matchTime.ToString()); 
        ballInGame = false;
        endMatch = false;


        //Active these panels to assign flags
        golAnimation_UI.SetActive(true);
        mainPausePanel.SetActive(true);
        

        //Set UI (flags and team names)
        SetTeamFlags("LeftTeamFlags", leftTeam.flag, leftTeam.teamName);
        SetTeamFlags("RightTeamFlags", rightTeam.flag, rightTeam.teamName);

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
        if (golName == "RightGoalTrigger")
        {
            //Increase score
            LeftTeamScore++;
            //Change color of Balls in goals UI
            Animator anim = leftTeamScore_UI.transform.GetChild(LeftTeamScore).GetComponent<Animator>();
            anim.SetTrigger("Goal");
        }
        else if (golName == "LeftGoalTrigger")
        {
            RightTeamScore++;
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
        if (LeftTeamScore == 5 || RightTeamScore == 5) StartCoroutine(PlayEndMatchAnimation(true));
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
        if(LeftTeamScore < 5 && RightTeamScore < 5) SetUIState(true);
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
        matchScore.text = LeftTeamScore.ToString() + "-" + RightTeamScore.ToString();
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
        if (knockout)
        {
            finalMatchStatusText.text = "Knockout!";
            yield return new WaitForSeconds(3f);
        }
        else finalMatchStatusText.text = "Time out!";
        
        //Activate final match animation.
        finalMatchPanel.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        finalMatchPanel.gameObject.SetActive(false);

        //Activate final match panel
        mainPausePanel.SetActive(true);
        //Change final match status
        if (LeftTeamScore > RightTeamScore)
        {
            matchStatus.text = "VICTORY!";
            matchStatus.color = Color.yellow;
        }
        else if (LeftTeamScore < RightTeamScore)
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
            SetInitalMatchTypeGivenTournament(TournamentController._tourCtlr.matchesRound, TournamentController._tourCtlr.teamsForKnockoutStage);

        intialAnimationObject.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        intialAnimationObject.SetActive(false);
        timePanel.SetActive(true);
        leftTeamScore_UI.SetActive(true);
        rightTeamScore_UI.SetActive(true);
        //Instiatite a ball
        SpawnBall();
    }

    /// <summary>
    /// Set match type string given the round and the type of tournament
    /// </summary>
    /// <param name="round">Tour roudn which is playing</param>
    /// <param name="finalTeams">Number of teams that qualified to finals</param>
    private void SetInitalMatchTypeGivenTournament(int round, int finalTeams)
    {
        if (round < 3)
        {
            int roundMatch = TournamentController._tourCtlr.matchesRound + 1;
            matchTypeText.text = TournamentController._tourCtlr.tourName + "\n Match " + roundMatch.ToString();
            matchType.text = matchTypeText.text;
        }
        else
        {
            if(finalTeams == 16)
            {
                switch (round)
                {
                    case 3:
                        matchTypeText.text = "Round of 16";
                        matchType.text = matchTypeText.text;
                        break;
                    case 4:
                        matchTypeText.text = "Quarter finals";
                        matchType.text = matchTypeText.text;
                        break;
                    case 5:
                        matchTypeText.text = "Semi finals";
                        matchType.text = matchTypeText.text;
                        break;
                    case 6:
                        matchTypeText.text = "FINAL";
                        matchType.text = matchTypeText.text;
                        break;
                }
            }
            else
            {
                switch (round)
                {
                    case 3:
                        matchTypeText.text = "Quarter finals";
                        matchType.text = matchTypeText.text;
                        break;
                    case 4:
                        matchTypeText.text = "Semi finals";
                        matchType.text = matchTypeText.text;
                        break;
                    case 5:
                        matchTypeText.text = "FINAL";
                        matchType.text = matchTypeText.text;
                        break;
                }
            }
        }
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

            if (TournamentController._tourCtlr.matchesRound < 3)
                TournamentController._tourCtlr.SimulateRoundOfMatches();
            else
                TournamentController._tourCtlr.SimulateRoundOfMatchesInKnockOutStage();

            TournamentController._tourCtlr.SaveTour();
        }

        SceneManager.LoadScene(sceneName);
    }
}
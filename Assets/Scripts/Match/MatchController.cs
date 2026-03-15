using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// This class is the one that handles all events in GameMatch scene.
/// </summary>
public class MatchController : MonoBehaviour {

    // Match lifecycle events for sound/crowd system
    public static event Action OnMatchStart;
    public static event Action OnMatchEnd;
    public static event Action OnBallSpawned;

    //Singleton
    public static MatchController instance;
    
    //Reference to ball in field
    public GameObject ball;
    public bool gameIsPaused;
    public bool ballInGame;

    [Header("Initial animation UI objects")]
    //Reference to the object that handle the initial animation.
    public GameObject intialAnimationObject;
    public Text matchTypeText;

    
    [Header("Final match animation objects")]
    public GameObject finalMatchPanel;
    public Text finalMatchStatusText;

    public GameObject golAnimation_UI;

    public bool endMatch;
    private bool matchEnded;
    private int roundMatch;

    public GameObject finishQuickMatchPanelOptions;
    public GameObject finishTourMatchPanelOptions;

    [Header("Restart ball Panel")]
    //Reference to the panel that counts when the ball is inactive and it should be restarted.
    public GameObject timeInactiveBallPanel;
    public Text restartingBallTimeText;
    
    
    //Singleton, reference to this script and its info.
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void OnDestroy()
    {
        // Clear static event delegates to prevent stale references across scene reloads
        OnMatchStart = null;
        OnMatchEnd = null;
        OnBallSpawned = null;
        if (instance == this) instance = null;
    }


    //Initial state of objects.
    private bool waitingForPlayerSelection;

    private void Start()
    {
        //Start score at 0 and game as playing
        gameIsPaused = false;

        ballInGame = false;
        endMatch = false;
        matchEnded = false;

        //Active these panels to assign flags
        golAnimation_UI.SetActive(true);
        golAnimation_UI.SetActive(false);

        finishQuickMatchPanelOptions.SetActive(false);

        // If the controls config panel is active, wait for the player to choose a side
        // and press Start — GameControlsConfigPanel.StartGame() will re-enable us
        if (GameControlsConfigPanel.instance != null && GameControlsConfigPanel.instance.gameObject.activeInHierarchy)
        {
            waitingForPlayerSelection = true;
            this.enabled = false;
            return;
        }

        //Start initial animation.
        StartCoroutine(InitAnimation());
    }

    private void OnEnable()
    {
        // When re-enabled by GameControlsConfigPanel.StartGame(), start the match
        if (waitingForPlayerSelection)
        {
            waitingForPlayerSelection = false;
            StartCoroutine(InitAnimation());
        }
    }

    private void Update()
    {
        //if time has finished start end animation 
        if (endMatch && !matchEnded)
        {
            StartCoroutine(PlayEndMatchAnimation(false));
            ballInGame = false;
            endMatch = false;
        }
    }

    /// <summary>
    /// Instatiate ball in game field
    /// </summary>
    public void SpawnBall()
    {
        Instantiate(ball, Vector2.zero, Quaternion.identity);
        OnBallSpawned?.Invoke();
    }

    /// <summary>
    /// Play goal animation. Every time a goal is scored.
    /// </summary>
    /// <returns>Animation duration</returns>
    public IEnumerator GolAnimation()
    {
        golAnimation_UI.SetActive(true);
        //Wait 4 seconds.
        yield return new WaitForSeconds(4f);
        //Hide Goal animation
        golAnimation_UI.SetActive(false);
    }

    /// <summary>
    /// Play end match animation.
    /// </summary>
    /// <param name="knockout">If a player has reTimeached 5 goals it is consider has knockout</param>
    /// <returns></returns>
    public IEnumerator PlayEndMatchAnimation(bool knockout)
    {
        // Guard against multiple invocations (knockout + timer can both trigger)
        if (matchEnded) yield break;
        matchEnded = true;

        // Signal match has ended (for sound/crowd system)
        OnMatchEnd?.Invoke();

        //If match time has finished and ball is still in gamefield, stop and destroy.
        ball.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        Destroy(GameObject.FindGameObjectWithTag("Ball"));

        // In auto-test mode, skip all UI panels — AutoMatchRunner handles the flow
        if (AutoMatchRunner.IsAutoMode)
        {
            yield break;
        }

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
        GetComponent<PauseMatchController>().mainPausePanel.SetActive(true);
        
        //Deactivate pause options
        GetComponent<PauseMatchController>().pauseMatchPanelOptions.SetActive(false);


        //Activate differents options depending of match's type.
        if (MatchInfo.instance.matchType == MatchType.QuickMatch) finishQuickMatchPanelOptions.SetActive(true);
        if (MatchInfo.instance.matchType == MatchType.TourMatch) finishTourMatchPanelOptions.SetActive(true);

    }
    
    //Activate inital animation.
    IEnumerator InitAnimation()
    {
        //Change initial animation text, depending of the match type
        if(MatchInfo.instance != null)
        {
            if (MatchInfo.instance.matchType == MatchType.QuickMatch)
            {
                matchTypeText.text = "Friendly";
                GetComponent<PauseMatchController>().matchType.text = matchTypeText.text;
            }
            if (MatchInfo.instance.matchType == MatchType.TourMatch)
                SetInitalMatchTypeGivenTournament(TournamentController._tourCtlr.matchesRound, TournamentController._tourCtlr.teamsForKnockoutStage);
        }
        else
        {
            matchTypeText.text = "Friendly";
            GetComponent<PauseMatchController>().matchType.text = matchTypeText.text;
        }
        

        intialAnimationObject.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        intialAnimationObject.SetActive(false);

        // Initialize timer with correct value from AutoMatchRunner/MatchInfo, then show panel
        var timerController = GetComponent<TimeMatchController>();
        timerController.enabled = true;
        timerController.ResetTimer();
        timerController.timePanel.SetActive(true);

        GetComponent<MatchScoreController>().leftTeamScore_UI.SetActive(true);
        GetComponent<MatchScoreController>().rightTeamScore_UI.SetActive(true);
        
        //Instiatite a ball
        BallBehavior.ResetMatchStats();
        RodBumpEffect.ResetMatchStats();
        SpawnBall();

        // Signal match has started (for sound/crowd system)
        OnMatchStart?.Invoke();
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
            GetComponent<PauseMatchController>().matchType.text = matchTypeText.text;
        }
        else
        {
            if(finalTeams == 16)
            {
                switch (round)
                {
                    case 3:
                        matchTypeText.text = "Round of 16";
                        GetComponent<PauseMatchController>().matchType.text = matchTypeText.text;
                        break;
                    case 4:
                        matchTypeText.text = "Quarter finals";
                        GetComponent<PauseMatchController>().matchType.text = matchTypeText.text;
                        break;
                    case 5:
                        matchTypeText.text = "Semi finals";
                        GetComponent<PauseMatchController>().matchType.text = matchTypeText.text;
                        break;
                    case 6:
                        matchTypeText.text = "FINAL";
                        GetComponent<PauseMatchController>().matchType.text = matchTypeText.text;
                        break;
                }
            }
            else
            {
                switch (round)
                {
                    case 3:
                        matchTypeText.text = "Quarter finals";
                        GetComponent<PauseMatchController>().matchType.text = matchTypeText.text;
                        break;
                    case 4:
                        matchTypeText.text = "Semi finals";
                        GetComponent<PauseMatchController>().matchType.text = matchTypeText.text;
                        break;
                    case 5:
                        matchTypeText.text = "FINAL";
                        GetComponent<PauseMatchController>().matchType.text = matchTypeText.text;
                        break;
                }
            }
        }
    }

    /// <summary>
    /// This method changes the scene. If you're quiting the game, it will restart the timescale
    /// and then destroy this game object.
    /// </summary>
    /// <param name="sceneName">Name of the scene</param>
    public void LoadScene(string sceneName)
    {
        Time.timeScale = AutoMatchRunner.IsAutoMode ? AutoMatchRunner.Instance.TimeScale : 1;
        if (sceneName == "MainMenu" || sceneName == "QuickMatchMenu")
        {
            if(sceneName == "MainMenu") Destroy(GameObject.FindGameObjectWithTag("PlayerDataObject"));
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
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

    [Header("Scriptable objects with teams information")]
    //Reference to teams that are on this match
    public Team leftTeam;
    public Team rightTeam;

    [Header("Initial animation UI objects")]
    //Reference to the object that handle the initial animation.
    public GameObject intialAnimationObject;
    public Text matchTypeText;

    
    [Header("Final match animation objects")]
    public GameObject finalMatchPanel;
    public Text finalMatchStatusText;

    public GameObject golAnimation_UI;

    public bool endMatch;
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
        _matchController = this;

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
        gameIsPaused = false;

        ballInGame = false;
        endMatch = false;

        //Active these panels to assign flags
        golAnimation_UI.SetActive(true);
        

        //Set UI (flags and team names)
        SetTeamFlags("LeftTeamFlags", leftTeam.flag, leftTeam.teamName);
        SetTeamFlags("RightTeamFlags", rightTeam.flag, rightTeam.teamName);

        
        golAnimation_UI.SetActive(false);

        finishQuickMatchPanelOptions.SetActive(false);

    }

    private void Update()
    {
        

        //if time has finished start end animation 
        if (endMatch)
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
        //PlayerDataController.playerData.totalMatches++;
        //switch(MatchInfo._matchInfo.matchLevel)
        //{
        //    case 1:
        //        PlayerDataController.playerData.easyLevelMatches++;
        //        break;
        //    case 2:
        //        PlayerDataController.playerData.normalLevelMatches++;
        //        break;
        //    case 3:
        //        PlayerDataController.playerData.hardLevelMatches++;
        //        break;
        //}


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
        GetComponent<PauseMatchController>().mainPausePanel.SetActive(true);
        
        //Deactivate pause options
        GetComponent<PauseMatchController>().pauseMatchPanelOptions.SetActive(false);


        //Activate differents options depending of match's type.
        if (MatchInfo._matchInfo.matchType == MatchType.QuickMatch) finishQuickMatchPanelOptions.SetActive(true);
        if (MatchInfo._matchInfo.matchType == MatchType.TourMatch) finishTourMatchPanelOptions.SetActive(true);

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

    //Activate inital animation.
    IEnumerator InitAnimation()
    {
        //Change initial animation text, depending of the match type
        if (MatchInfo._matchInfo.matchType == MatchType.QuickMatch)
        {
            matchTypeText.text = "Friendly";
            GetComponent<PauseMatchController>().matchType.text = matchTypeText.text;
        }
        if (MatchInfo._matchInfo.matchType == MatchType.TourMatch)
            SetInitalMatchTypeGivenTournament(TournamentController._tourCtlr.matchesRound, TournamentController._tourCtlr.teamsForKnockoutStage);

        intialAnimationObject.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        intialAnimationObject.SetActive(false);

        GetComponent<TimeMatchController>().timePanel.SetActive(true);

        GetComponent<MatchScoreController>().leftTeamScore_UI.SetActive(true);
        GetComponent<MatchScoreController>().rightTeamScore_UI.SetActive(true);
        
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
        Time.timeScale = 1;
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
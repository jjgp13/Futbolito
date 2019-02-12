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

    //Variables to hanlde bullet time
    public float slowDownTime;
    public float timesSlower;
    public bool bulletTime;
    private float bulletTimeTimer;

    //Reference to camera and animator (also bullet time)
    public GameObject cam;
    private Animator camAnimator;

    //Reference to teams that are on this match
    public Team playerTeam;
    public Team npcTeam;

    //Reference to the object that handle the initial animation.
    public GameObject intialAnimationObject;

    //Reference to the panels in this match scene
    public GameObject Menu_UI;
    public GameObject pausePanel;
    public GameObject finishPanel;
    public Text StatusTitleMenu;
    public Text matchTextScore;
    public Text finalMatchStatus;

    public GameObject golAnimation_UI;
    public GameObject playerScore_UI;
    public GameObject NPCScore_UI;

    public GameObject holding_UI;
    public GameObject shooting_UI;
    public GameObject pauseBtn_UI;

    //Variables that manage the time elapsed in the match
    public Text timeText;
    private float timer;
    private bool endMatch;
    public bool ballInGame;

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
        timer = MatchInfo._matchInfo.matchTime * 60;
        timeText.text = timer.ToString();
        ballInGame = false;
        endMatch = false;

        //Reference to player and NPC Game objects to pull info
        playerTeam = MatchInfo._matchInfo.playerTeam;
        npcTeam = MatchInfo._matchInfo.comTeam;
        //Set UI (flags and team names)
        SetTeamFlags("PlayerFlags", playerTeam.flag, playerTeam.teamName);
        SetTeamFlags("ComFlags", npcTeam.flag, npcTeam.teamName);

        //Hide UI panels
        pausePanel.SetActive(true);
        finishPanel.SetActive(false);
        Menu_UI.SetActive(false);
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

        if (timer <= 0)
        {
            endMatch = true;
            timeText.text = "FINISH";
            timeText.GetComponent<Animator>().SetBool("Warning", false);
        }

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

    public IEnumerator GolAnimation()
    {
        golAnimation_UI.SetActive(true);
        SetUIState(false);
        yield return new WaitForSeconds(4f);
        golAnimation_UI.SetActive(false);
        if(playerScore < 5 && NPCScore < 5) SetUIState(true);
    }

    public void SetUIState(bool active)
    {
        holding_UI.SetActive(active);
        shooting_UI.SetActive(active);
        pauseBtn_UI.SetActive(active);
    }

    public void UpdateUIScore()
    {
        matchTextScore.text = playerScore.ToString() + "-" + NPCScore.ToString();
    }

    public IEnumerator PlayEndMatchAnimation(bool knockout)
    {

        ball.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        Destroy(GameObject.FindGameObjectWithTag("Ball"));

        if (knockout)
        {
            finalMatchStatus.text = "Knockout";
            yield return new WaitForSeconds(4f);
            finalMatchStatus.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
            finalMatchStatus.gameObject.SetActive(false);
        }
        else
        {
            finalMatchStatus.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
            finalMatchStatus.gameObject.SetActive(false);
        }

        Menu_UI.SetActive(true);
        if (playerScore > NPCScore)
            StatusTitleMenu.text = "YOU WIN!";
        else if (playerScore < NPCScore)
            StatusTitleMenu.text = "YOU LOSE!";
        else
            StatusTitleMenu.text = "DRAW!";
        pausePanel.SetActive(false);
        finishPanel.SetActive(true);
        SetUIState(false);
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
        Menu_UI.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
        holding_UI.SetActive(true);
        shooting_UI.SetActive(true);
    }

    /// <summary>
    /// This method will pause the match, if the match is in playing state.
    /// </summary>
    public void Pause()
    {
        Menu_UI.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
        holding_UI.SetActive(false);
        shooting_UI.SetActive(false);
    }

    IEnumerator InitAnimation()
    {
        yield return new WaitForSeconds(1);
        intialAnimationObject.SetActive(true);
        yield return new WaitForSeconds(4);
        intialAnimationObject.SetActive(false);
    }

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
        if (sceneName == "MainMenu")
        {
            Time.timeScale = 1;
            Destroy(GameObject.Find("MatchInfo"));
        }
        SceneManager.LoadScene(sceneName);
    }
}
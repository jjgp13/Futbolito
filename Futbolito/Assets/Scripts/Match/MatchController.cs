using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class MatchController : MonoBehaviour {

    public static MatchController _matchController;
    public GameObject ball;
    public bool gameIsPaused;

    public float slowDownTime;
    public float timesSlower;
    public bool bulletTime;
    private float bulletTimeTimer;
    private Vector2 ballPosition;

    public GameObject cam;
    private Animator camAnimator;

    public Team playerTeam;
    public Team npcTeam;

    public GameObject intialAnimationObject;

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

    public Text timeText;
    private float timer;
    private bool endMatch;
    public bool ballInGame;

    public GameObject timeInactiveBallPanel;
    public Text restartingBallTimeText;
    

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

    private void Awake()
    {
        _matchController = this;
    }

    private void Start()
    {
        StartCoroutine(InitAnimation());
        playerScore = 0;
        NPCScore = 0;
        gameIsPaused = false;

        //Set time
        timer = MatchInfo._matchInfo.matchTime * 60;
        timeText.text = "TIME";
        ballInGame = false;
        endMatch = false;

        playerTeam = GameObject.Find("MatchInfo").GetComponent<MatchInfo>().playerTeam;
        npcTeam = GameObject.Find("MatchInfo").GetComponent<MatchInfo>().comTeam;
        SetTeamFlags("PlayerFlags", playerTeam.flag);
        SetTeamFlags("ComFlags", npcTeam.flag);

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

    public void AdjustScore(string golName)
    {
        GetComponent<SoundMatchController>().PlayGolSound();
        if (golName == "PlayerGol")
        {
            playerScore++;
            playerScore_UI.transform.GetChild(playerScore-1).GetComponent<Image>().color = Color.white;
        }
        else if (golName == "NPCGol")
        {
            NPCScore++;
            NPCScore_UI.transform.GetChild(NPCScore-1).GetComponent<Image>().color = Color.white;
        }
        UpdateUIScore();
        CheckScore();
    }

    public void CheckScore()
    {
        if (playerScore == 5 || NPCScore == 5) StartCoroutine(PlayEndMatchAnimation(true));
        else SpawnBall();
    }

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

    public void SetTeamFlags(string tag, Sprite flag)
    {
        GameObject[] flags = GameObject.FindGameObjectsWithTag(tag);
        for (int i = 0; i < flags.Length; i++) flags[i].GetComponent<Image>().sprite = flag;
    }

    public void Resume()
    {
        Menu_UI.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
        holding_UI.SetActive(true);
        shooting_UI.SetActive(true);
    }

    public void Pause()
    {
        Menu_UI.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
        holding_UI.SetActive(false);
        shooting_UI.SetActive(false);
    }

    public void LoadScene(string sceneName)
    {
        if (sceneName == "MainMenu")
        {
            Time.timeScale = 1;
            Destroy(GameObject.Find("MatchInfo"));
        }
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator InitAnimation()
    {
        yield return new WaitForSeconds(1);
        intialAnimationObject.SetActive(true);
        yield return new WaitForSeconds(4);
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
}
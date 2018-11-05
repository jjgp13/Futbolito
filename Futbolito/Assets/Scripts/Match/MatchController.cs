using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class MatchController : MonoBehaviour {

    public static MatchController _matchController;
    public GameObject ball;
    public bool gameIsPaused;

    public Team playerTeam;
    public Team npcTeam;

    public GameObject intialAnimationObject;

    public GameObject Menu_UI;
    public GameObject pausePanel;
    public GameObject finishPanel;
    public Text StatusTitleMenu;
    public Text matchTextScore;

    public GameObject golAnimation_UI;
    public GameObject playerScore_UI;
    public GameObject NPCScore_UI;

    public GameObject holding_UI;
    public GameObject shooting_UI;
    public GameObject pauseBtn_UI;

    public Text timeText;
    private float timer;

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
        timeText.text = timer + ":00";


        playerTeam = GameObject.Find("MatchInfo").GetComponent<MatchInfo>().playerTeam;
        npcTeam = GameObject.Find("MatchInfo").GetComponent<MatchInfo>().comTeam;
        SetTeamFlags("PlayerFlags", playerTeam.flag);
        SetTeamFlags("ComFlags", npcTeam.flag);


        pausePanel.SetActive(true);
        finishPanel.SetActive(false);
        Menu_UI.SetActive(false);
        golAnimation_UI.SetActive(false);
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        string minutes = Mathf.Floor(timer / 60).ToString("00");
        string seconds = (timer % 60).ToString("00");
        timeText.text = string.Format("{0}:{1}", minutes, seconds);
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
        if(playerScore == 5)
        {

        }
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
        SetUIState(true);
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

    public IEnumerator PlayEndMatchAnimation()
    {
        yield return new WaitForSeconds(4f);
        //gameFinishedMenu_UI.SetActive(true);
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

    public void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }

    IEnumerator InitAnimation()
    {
        yield return new WaitForSeconds(1);
        intialAnimationObject.SetActive(true);
    }
}

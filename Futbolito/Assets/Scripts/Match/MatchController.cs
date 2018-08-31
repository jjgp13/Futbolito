using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class MatchController : MonoBehaviour {

    public static MatchController _matchController;
    public GameObject ball;
    public bool gameIsPaused;

    public Team playerTeam;
    public Team npcTeam;

    public GameObject gameFinishedMenu_UI;
    public GameObject pausedMenu_UI;

    public GameObject golAnimation_UI;
    public GameObject playerScore_UI;
    public GameObject NPCScore_UI;

    public GameObject holding_UI;
    public GameObject shooting_UI;
    public GameObject pauseBtn_UI;

    public Text textPauseScore;

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
        playerScore = 0;
        NPCScore = 0;
        gameIsPaused = false;

        playerTeam = GameObject.Find("PlayerInfo").GetComponent<TeamPickedInfo>().teamPicked;
        npcTeam = GameObject.Find("NpcInfo").GetComponent<TeamPickedInfo>().teamPicked;
        SetTeamFlags("PlayerFlags", playerTeam.flag);
        SetTeamFlags("NpcFlags", npcTeam.flag);

        gameFinishedMenu_UI.SetActive(false);
        pausedMenu_UI.SetActive(false);
        golAnimation_UI.SetActive(false);
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
        textPauseScore.text = playerScore.ToString() + "-" + NPCScore.ToString();
    }

    public IEnumerator PlayEndMatchAnimation()
    {
        yield return new WaitForSeconds(4f);
        gameFinishedMenu_UI.SetActive(true);
        SetUIState(false);
    }

    public void SetTeamFlags(string tag, Sprite flag)
    {
        GameObject[] flags = GameObject.FindGameObjectsWithTag(tag);
        for (int i = 0; i < flags.Length; i++) flags[i].GetComponent<Image>().sprite = flag;
    }

    public void Resume()
    {
        pausedMenu_UI.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
        holding_UI.SetActive(true);
        shooting_UI.SetActive(true);
    }

    public void Pause()
    {
        pausedMenu_UI.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
        holding_UI.SetActive(false);
        shooting_UI.SetActive(false);
    }

    public void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }


}

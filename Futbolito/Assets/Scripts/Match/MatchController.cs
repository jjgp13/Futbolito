using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchController : MonoBehaviour {

    public static MatchController _matchController;
    public GameObject ball;

    public Team playerTeam;
    public Team NpcTeam;

    public Sprite[] scoreSprites;

    public GameObject golAnimation;
    public GameObject playerScoreUI;
    public GameObject NPCScoreUI;

    public GameObject holdingUI;
    public GameObject shootingUI;
    public GameObject pauseBtnUI;

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
        playerTeam = GameObject.Find("PlayerInfo").GetComponent<TeamPickedInfo>().teamPicked;
        NpcTeam = GameObject.Find("NpcInfo").GetComponent<TeamPickedInfo>().teamPicked;

        
    }


    public void AdjustScore(string golName)
    {
        GetComponent<SoundMatchController>().PlayGolSound();
        if (golName == "PlayerGol")
        {
            playerScore++;
            playerScoreUI.transform.GetChild(playerScore-1).GetComponent<Image>().color = Color.white;
        }
        else if (golName == "NPCGol")
        {
            NPCScore++;
            NPCScoreUI.transform.GetChild(NPCScore-1).GetComponent<Image>().color = Color.white;
        }
        UpdateUIScore();
    }

    public void SpawnBall()
    {
        Instantiate(ball, Vector2.zero, Quaternion.identity);
    }

    public IEnumerator GolAnimation()
    {
        golAnimation.SetActive(true);
        GameObject.FindGameObjectWithTag("PlayerFlags").GetComponent<Image>().sprite = playerTeam.flag;
        GameObject.FindGameObjectWithTag("NpcFlags").GetComponent<Image>().sprite = NpcTeam.flag;
        SetUIState(false);
        yield return new WaitForSeconds(4f);
        golAnimation.SetActive(false);
        SetUIState(true);
    }

    public void SetUIState(bool active)
    {
        holdingUI.SetActive(active);
        shootingUI.SetActive(active);
        pauseBtnUI.SetActive(active);
    }

    public void UpdateUIScore()
    {
        textPauseScore.text = playerScore.ToString() + "-" + NPCScore.ToString();
    }

    public void PlayEndMatchAnimation()
    {

    }
}

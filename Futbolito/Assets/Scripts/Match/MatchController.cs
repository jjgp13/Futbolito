using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchController : MonoBehaviour {

    public static MatchController _matchController;
    public GameObject ball;

    public Sprite[] scoreSprites;
    public GameObject playerScoreSprite, NPCScoreSprite;

    public GameObject golAnimation;

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
    }


    public void AdjustScorePlayer()
    {
        playerScore++;
        playerScoreSprite.GetComponent<SpriteRenderer>().sprite = scoreSprites[playerScore];
        UpdatePauseScore();
    }

    public void AdjustScoreNPC()
    {
        NPCScore++;
        NPCScoreSprite.GetComponent<SpriteRenderer>().sprite = scoreSprites[NPCScore];
        UpdatePauseScore();
    }

    public void SpawnBall()
    {
        Instantiate(ball, Vector2.zero, Quaternion.identity);
    }

    public IEnumerator DeactivateGolAnimation()
    {
        yield return new WaitForSeconds(2.5f);
        golAnimation.SetActive(false);
    }

    public void UpdatePauseScore()
    {
        textPauseScore.text = playerScore.ToString() + "-" + NPCScore.ToString();
    }
}

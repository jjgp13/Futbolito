using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchController : MonoBehaviour {

    public static MatchController _matchController;
    public GameObject ball;

    public Sprite[] scoreSprites;
    public GameObject playerScoreSprite, NPCScoreSprite;

    private void Awake()
    {
        _matchController = this;
    }

    private void Start()
    {
        playerScore = 0;
        NPCScore = 0;
    }

    private int playerScore;
    private int NPCScore;

    public void AdjustScorePlayer()
    {
        playerScore++;
        playerScoreSprite.GetComponent<SpriteRenderer>().sprite = scoreSprites[playerScore];
    }

    public void AdjustScoreNPC()
    {
        NPCScore++;
        NPCScoreSprite.GetComponent<SpriteRenderer>().sprite = scoreSprites[NPCScore];
    }

    public void SpawnBall()
    {
        Instantiate(ball, Vector2.zero, Quaternion.identity);
    }

    
}

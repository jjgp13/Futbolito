using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchController : MonoBehaviour {

    public static MatchController _matchController;
    public Text playerGoalsText;
    public Text NPCGoalsText;
    public GameObject ball;

    private void Awake()
    {
        _matchController = this;
    }

    private void Start()
    {
        playerGoals = 0;
        NPC_Goals = 0;
    }

    private int playerGoals;
    private int NPC_Goals;

    public void AdjustScorePlayer()
    {
        playerGoals++;
        playerGoalsText.text = playerGoals.ToString();
    }

    public void AdjustScoreNPC()
    {
        NPC_Goals++;
        NPCGoalsText.text = NPC_Goals.ToString();
    }

    public void SpawnBall()
    {
        Instantiate(ball, Vector2.zero, Quaternion.identity);
    }

    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalNPC : MonoBehaviour {

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "Ball")
        {
            MatchController._matchController.golAnimation.SetActive(true);
            StartCoroutine(MatchController._matchController.DeactivateGolAnimation());
            MatchController._matchController.AdjustScoreNPC();
            if(MatchController._matchController.NPC_Score < 5) MatchController._matchController.SpawnBall();
            Destroy(other.gameObject);
        }
    }

}

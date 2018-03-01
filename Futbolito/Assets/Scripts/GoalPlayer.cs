using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalPlayer : MonoBehaviour {

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Ball")
        {
            MatchController._matchController.AdjustScorePlayer();
            MatchController._matchController.SpawnBall();
            Destroy(other.gameObject);
        }
    }
}

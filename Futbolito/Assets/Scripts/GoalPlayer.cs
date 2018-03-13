﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalPlayer : MonoBehaviour {

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Ball")
        {
            MatchController._matchController.AdjustScorePlayer();
            if(MatchController._matchController.PlayerScore < 5 ) MatchController._matchController.SpawnBall();
            Destroy(other.gameObject);
        }
    }
}

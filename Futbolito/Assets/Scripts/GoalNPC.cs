﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalNPC : MonoBehaviour {

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "Ball")
        {
            MatchController._matchController.AdjustScoreNPC();
            MatchController._matchController.SpawnBall();
            Destroy(other.gameObject);
        }
    }

}
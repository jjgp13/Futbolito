using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStats : MonoBehaviour{

    public bool isShooting;
    public float shootSpeed;

    public float[] shootSpeeds;

    private void Start()
    {
        int d = MatchInfo._matchInfo.difficulty;
        switch (d)
        {
            case 1:
                shootSpeed = shootSpeeds[d - 1];
                break;
            case 2:
                shootSpeed = shootSpeeds[d - 1];
                break;
            case 3:
                shootSpeed = shootSpeeds[d - 1];
                break;
        }
    }

}

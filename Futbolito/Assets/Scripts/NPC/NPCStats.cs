using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStats : MonoBehaviour{

    public bool isShooting;
    public int shootSpeed;

    private void Start()
    {
        int d = MatchInfo._matchInfo.difficulty;
        switch (d)
        {
            case 1:
                shootSpeed = 30;
                break;
            case 2:
                shootSpeed = 50;
                break;
            case 3:
                shootSpeed = 70;
                break;
        }
    }

}

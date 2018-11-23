using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStats : MonoBehaviour{

    public bool isShooting;
    public float shootSpeed;

    private void Start()
    {
        int d = MatchInfo._matchInfo.difficulty;
        switch (d)
        {
            case 1:
                shootSpeed = 0.2f;
                break;
            case 2:
                shootSpeed = 0.5f;
                break;
            case 3:
                shootSpeed = 1f;
                break;
        }
    }

}

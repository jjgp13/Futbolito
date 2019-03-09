using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStats : MonoBehaviour{

    public float shootSpeed;
    public float attractionForce;

    private readonly float[] shootSpeeds =  new float[3] {0.25f, 0.5f, 0.75f };
    private readonly float[] attractionForces = new float[3] { -0.1f, -0.1f, -0.15f };

    private void Start()
    {
        int d = MatchInfo._matchInfo.matchLevel - 1;
        switch (d)
        {
            case 1:
                shootSpeed = shootSpeeds[d];
                attractionForce = attractionForces[d];
                break;
            case 2:
                shootSpeed = shootSpeeds[d];
                attractionForce = attractionForces[d];
                break;
            case 3:
                shootSpeed = shootSpeeds[d];
                attractionForce = attractionForces[d];
                break;
        }
    }

}

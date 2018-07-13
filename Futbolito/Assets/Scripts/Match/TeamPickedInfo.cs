using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamPickedInfo : MonoBehaviour {

    private static bool created = false;
    public Team teamPicked;

    void Awake()
    {
        if (!created)
        {
            DontDestroyOnLoad(gameObject);
            created = true;
        }
    }
}

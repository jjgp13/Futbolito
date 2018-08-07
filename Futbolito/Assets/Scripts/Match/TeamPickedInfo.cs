using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamPickedInfo : MonoBehaviour {

    public Team teamPicked;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootButton : MonoBehaviour {

    public bool isShooting;

	// Use this for initialization
	void Start () {
        isShooting = false;
	}

    public void Holding()
    {
        isShooting = true;
    }

    public void Release()
    {
        isShooting = false;
    }
}

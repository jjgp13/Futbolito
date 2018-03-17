using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesBehavior : MonoBehaviour {

    public ParticleSystem particles;
    private int y, x; 
	
	// Update is called once per frame
	void Update () {
        y = Mathf.Abs((int)GetComponent<Rigidbody2D>().velocity.y);
        x = Mathf.Abs((int)GetComponent<Rigidbody2D>().velocity.x);
        int emission;
        if(y > x) emission = y;
        else emission = x;
        particles.Emit(emission);
	}
}

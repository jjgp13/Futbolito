using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesBehavior : MonoBehaviour {

    public ParticleSystem particles;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        int emission = (int)GetComponent<Rigidbody2D>().velocity.y;
        emission = Mathf.Abs(emission);
        particles.Emit(emission);
	}
}

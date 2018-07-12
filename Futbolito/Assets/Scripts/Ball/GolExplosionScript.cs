using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolExplosionScript : MonoBehaviour {

    public ParticleSystem particles;

	// Use this for initialization
	void Start () {
        Invoke("DestroyExplosion", 2);
	}
	
	void DestroyExplosion()
    {
        Destroy(gameObject);
    }
}

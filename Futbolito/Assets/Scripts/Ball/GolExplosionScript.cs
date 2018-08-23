using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolExplosionScript : MonoBehaviour {

    public ParticleSystem particles;
    public int timeToDestroy;

	// Use this for initialization
	void Start () {
        Invoke("DestroyExplosion", timeToDestroy);
	}
	
	void DestroyExplosion()
    {
        Destroy(gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolController : MonoBehaviour {

    public GameObject GolExplosionAnimation;
    public AudioClip[] golSounds;
    private AudioSource audioS;


	// Use this for initialization
	void Start () {
        audioS = GetComponent<AudioSource>();
	}

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Ball")
        {
            PlaySound();

            if (gameObject.tag == "GoalTrigger")
                MatchController._matchController.AdjustScore(gameObject.name);

            StartCoroutine(MatchController._matchController.GolAnimation());

            Instantiate(GolExplosionAnimation, other.gameObject.transform.position, Quaternion.identity);
            Destroy(other.gameObject);
            MatchController._matchController.ballInGame = false;
            
        }
    }

    private void PlaySound()
    {
        int clipIndex = Random.Range(0, 4);
        audioS.clip = golSounds[clipIndex];
        audioS.Play();
    }
}

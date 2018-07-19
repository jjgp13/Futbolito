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
            MatchController._matchController.golAnimation.SetActive(true);
            StartCoroutine(MatchController._matchController.DeactivateGolAnimation());
            if (gameObject.name == "PlayerGol")
                MatchController._matchController.AdjustScorePlayer();
            else if (gameObject.name == "NPCGol")
                MatchController._matchController.AdjustScoreNPC();

            if (MatchController._matchController.PlayerScore < 5 && MatchController._matchController.NPC_Score < 5) MatchController._matchController.SpawnBall();
            else MatchController._matchController.PlayEndMatchAnimation();
            Instantiate(GolExplosionAnimation, other.gameObject.transform.position, Quaternion.identity);
            Destroy(other.gameObject);
        }
    }

    private void PlaySound()
    {
        int clipIndex = Random.Range(0, 4);
        audioS.clip = golSounds[clipIndex];
        audioS.Play();
    }
}

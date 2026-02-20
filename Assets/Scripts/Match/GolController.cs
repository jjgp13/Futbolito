using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolController : MonoBehaviour {

    /// <summary>
    /// Fired when a goal is scored. Parameter is the scoring side tag ("LeftGoalTrigger" or "RightGoalTrigger").
    /// </summary>
    public static event Action<string> OnGoalScored;

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

            if (gameObject.tag == "LeftGoalTrigger" || gameObject.tag == "RightGoalTrigger")
            {
                // Log goal event for AI analysis
                string scoringSide = gameObject.tag == "LeftGoalTrigger" ? "LEFT_GOAL" : "RIGHT_GOAL";
                int leftScore = MatchScoreController.instance != null ? MatchScoreController.instance.LeftTeamScore : -1;
                int rightScore = MatchScoreController.instance != null ? MatchScoreController.instance.RightTeamScore : -1;
                AIDebugLogger.Log("MATCH", "GOAL_SCORED",
                    $"{scoringSide} — Score: L{leftScore + (scoringSide == "LEFT_GOAL" ? 1 : 0)}-R{rightScore + (scoringSide == "RIGHT_GOAL" ? 1 : 0)}");

                MatchScoreController.instance.AdjustScore(gameObject.name);

                // Fire event for sound/crowd system
                OnGoalScored?.Invoke(gameObject.tag);
            }

            StartCoroutine(MatchController.instance.GolAnimation());

            Instantiate(GolExplosionAnimation, other.gameObject.transform.position, Quaternion.identity);
            Destroy(other.gameObject);
            MatchController.instance.ballInGame = false;
            
        }
    }

    private void PlaySound()
    {
        int clipIndex = UnityEngine.Random.Range(0, golSounds.Length);
        audioS.clip = golSounds[clipIndex];
        audioS.Play();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSoundsController : MonoBehaviour {

    public AudioClip againstPaddle;
    public AudioClip paddleHit;
    public AudioClip againstWall;
    public AudioClip goal;

    private AudioSource audioS;

    private void Start()
    {
        audioS = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip sound)
    {
        audioS.PlayOneShot(sound);
    }
}

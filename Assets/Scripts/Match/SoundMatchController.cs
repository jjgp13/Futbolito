using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundMatchController : MonoBehaviour {

    public AudioClip kickOffSound;
    public AudioClip[] ambientSounds;
    public AudioClip golSound;

    private AudioSource audioS;
    private int clipIndex;

	// Use this for initialization
	void Start () {
        audioS = GetComponent<AudioSource>();
        clipIndex = 0;
	}
	
	// Update is called once per frame
	void Update () {
        if (!audioS.isPlaying)
        {
            clipIndex++;
            audioS.clip = ambientSounds[clipIndex];
            audioS.Play();
            if (clipIndex == ambientSounds.Length-1) clipIndex = 0;
        }
	}

    public IEnumerator PlayKO()
    {
        yield return new WaitForSeconds(4);
        audioS.PlayOneShot(kickOffSound);
    }

    public void PlayGolSound()
    {
        audioS.PlayOneShot(golSound);
    }
}

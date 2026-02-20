using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SoundMatchController — Thin wrapper that delegates to MatchAudioManager.
/// Retained for backward compatibility with existing scene references.
/// </summary>
public class SoundMatchController : MonoBehaviour {

    public AudioClip kickOffSound;
    public AudioClip[] ambientSounds;
    public AudioClip golSound;

    private AudioSource audioS;

	void Start () {
        audioS = GetComponent<AudioSource>();

        // Push clips to MatchAudioManager if it doesn't have them assigned
        PushClipsToManager();
	}

    /// <summary>
    /// If MatchAudioManager exists but has no clips assigned yet,
    /// forward ours so everything works without extra Inspector wiring.
    /// </summary>
    private void PushClipsToManager()
    {
        if (MatchAudioManager.instance == null) return;

        if (MatchAudioManager.instance.ambientCrowdClips == null || MatchAudioManager.instance.ambientCrowdClips.Length == 0)
            MatchAudioManager.instance.ambientCrowdClips = ambientSounds;

        if (MatchAudioManager.instance.kickoffWhistleClip == null && kickOffSound != null)
            MatchAudioManager.instance.kickoffWhistleClip = kickOffSound;
    }

	void Update () {
        // Ambient loop is now handled by MatchAudioManager — do nothing here
	}

    public IEnumerator PlayKO()
    {
        yield return new WaitForSeconds(4);
        // Delegate to MatchAudioManager if available, fallback to local
        if (MatchAudioManager.instance != null && kickOffSound != null)
        {
            // MatchAudioManager handles this via OnMatchStart event
        }
        else if (audioS != null && kickOffSound != null)
        {
            audioS.PlayOneShot(kickOffSound);
        }
    }

    public void PlayGolSound()
    {
        // Goal sound is handled by GolController + MatchAudioManager celebration
        // Keep as fallback
        if (MatchAudioManager.instance == null && audioS != null && golSound != null)
        {
            audioS.PlayOneShot(golSound);
        }
    }
}

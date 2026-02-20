using UnityEngine;

public class BallSoundsController : MonoBehaviour
{
    [Header("Figure Hit Sounds")]
    [Tooltip("Random variations for ball-vs-figure impacts")]
    public AudioClip[] figureHitClips;

    [Header("Wall Hit Sounds")]
    [Tooltip("Random variations for ball-vs-wall impacts")]
    public AudioClip[] wallHitClips;

    [Header("Volume & Pitch Scaling")]
    [Tooltip("Minimum volume for light impacts")]
    [Range(0f, 1f)]
    public float minVolume = 0.3f;
    [Tooltip("Maximum volume for heavy impacts")]
    [Range(0f, 1f)]
    public float maxVolume = 1.0f;

    [Tooltip("Pitch range for random variation")]
    public float minPitch = 0.85f;
    public float maxPitch = 1.25f;

    [Header("Cooldown")]
    [Tooltip("Minimum time between impact sounds to prevent overlap")]
    public float soundCooldown = 0.05f;

    [Header("Velocity Scaling")]
    [Tooltip("Impact velocity that maps to minimum volume")]
    public float lowVelocityRef = 5f;
    [Tooltip("Impact velocity that maps to maximum volume")]
    public float highVelocityRef = 60f;

    private AudioSource audioS;
    private float lastSoundTime;

    // SFX volume multiplier set by MatchAudioManager
    private float sfxVolumeMultiplier = 1f;

    private void Awake()
    {
        audioS = GetComponent<AudioSource>();
        if (audioS == null)
            audioS = gameObject.AddComponent<AudioSource>();

        audioS.playOnAwake = false;
        audioS.spatialBlend = 0f; // 2D sound
    }

    private void OnEnable()
    {
        BallBehavior.OnBallImpact += HandleBallImpact;
    }

    private void OnDisable()
    {
        BallBehavior.OnBallImpact -= HandleBallImpact;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolumeMultiplier = volume;
    }

    private void HandleBallImpact(BallImpactEventArgs impact)
    {
        if (impact.ImpactLevel < 0) return; // Too weak
        if (Time.time - lastSoundTime < soundCooldown) return;

        AudioClip clip = null;

        switch (impact.Type)
        {
            case CollisionType.Figure:
                clip = GetRandomClip(figureHitClips);
                break;
            case CollisionType.Wall:
                clip = GetRandomClip(wallHitClips);
                break;
            default:
                return;
        }

        if (clip == null) return;

        // Scale volume and pitch based on impact velocity
        float velocityNormalized = Mathf.InverseLerp(lowVelocityRef, highVelocityRef, impact.ImpactForce);
        float volume = Mathf.Lerp(minVolume, maxVolume, velocityNormalized) * sfxVolumeMultiplier;
        float pitch = Random.Range(minPitch, maxPitch);

        audioS.pitch = pitch;
        audioS.PlayOneShot(clip, volume);
        lastSoundTime = Time.time;
    }

    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }
}

using UnityEngine;

/// <summary>
/// MatchAudioManager — Singleton that orchestrates all match audio.
/// 
/// LAYERED CROWD SYSTEM:
/// 1. Ambient layer: continuous low crowd murmur (loops CrowdSound clips)
/// 2. Tension layer: fades in when ball is in attacking third near a goal
/// 3. Excitement SFX: one-shot gasps/reactions on near-miss, save, dangerous play
/// 4. Celebration layer: goal roar triggered by OnGoalScored
/// 
/// NEAR-MISS DETECTION:
/// Listens to BallBehavior.OnBallImpact — if ball hits a wall tagged
/// TopGoalWall/BottomGoalWall at high velocity → crowd gasps.
/// 
/// DANGEROUS PLAY:
/// Ball in attacking third + high velocity → plays JugadaPeligrosa clip.
/// 
/// VOLUME CONTROLS:
/// Master, SFX, Crowd volumes — persisted via PlayerPrefs.
/// </summary>
public class MatchAudioManager : MonoBehaviour
{
    public static MatchAudioManager instance;

    #region Audio Clip Configuration

    [Header("=== AMBIENT CROWD ===")]
    [Tooltip("Ambient crowd loop clips (played continuously, cycled)")]
    public AudioClip[] ambientCrowdClips;

    [Tooltip("Base volume for ambient crowd")]
    [Range(0f, 1f)]
    public float ambientBaseVolume = 0.15f;

    [Header("=== TENSION LAYER ===")]
    [Tooltip("Tension crowd clips (faded in during attacking plays)")]
    public AudioClip[] tensionClips;

    [Tooltip("Maximum volume for tension layer")]
    [Range(0f, 1f)]
    public float tensionMaxVolume = 0.4f;

    [Tooltip("How fast tension fades in (per second)")]
    public float tensionFadeInSpeed = 0.8f;

    [Tooltip("How fast tension fades out (per second)")]
    public float tensionFadeOutSpeed = 0.5f;

    [Header("=== EXCITEMENT / REACTIONS ===")]
    [Tooltip("Crowd gasp clips for near-misses")]
    public AudioClip[] crowdGaspClips;

    [Tooltip("Crowd reaction for great saves")]
    public AudioClip[] saveReactionClips;

    [Tooltip("Dangerous play reaction (JugadaPeligrosa)")]
    public AudioClip dangerousPlayClip;

    [Tooltip("Volume for excitement one-shots")]
    [Range(0f, 1f)]
    public float excitementVolume = 0.7f;

    [Tooltip("Cooldown between excitement sounds (prevents spam)")]
    public float excitementCooldown = 1.5f;

    [Header("=== GOAL CELEBRATION ===")]
    [Tooltip("Crowd celebration clips on goal")]
    public AudioClip[] celebrationClips;

    [Tooltip("Volume for goal celebration")]
    [Range(0f, 1f)]
    public float celebrationVolume = 0.9f;

    [Tooltip("Duration of celebration before fading")]
    public float celebrationDuration = 4f;

    [Header("=== WHISTLE ===")]
    [Tooltip("Kickoff whistle clip")]
    public AudioClip kickoffWhistleClip;

    [Tooltip("End match whistle clip (can reuse kickoff with different pitch)")]
    public AudioClip endWhistleClip;

    [Tooltip("Volume for whistle")]
    [Range(0f, 1f)]
    public float whistleVolume = 0.8f;

    [Header("=== NEAR-MISS DETECTION ===")]
    [Tooltip("Minimum impact velocity to trigger near-miss gasp on goal walls")]
    public float nearMissVelocityThreshold = 20f;

    [Header("=== DANGEROUS PLAY DETECTION ===")]
    [Tooltip("Minimum ball velocity to consider a dangerous play")]
    public float dangerousPlayVelocityThreshold = 25f;

    [Tooltip("Ball must be within this X distance of a goal to be 'dangerous'")]
    public float dangerousPlayGoalProximity = 5f;

    [Tooltip("Cooldown for dangerous play sound")]
    public float dangerousPlayCooldown = 3f;

    #endregion

    #region Volume Settings

    [Header("=== VOLUME CONTROLS ===")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;

    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    [Range(0f, 1f)]
    public float crowdVolume = 1f;

    #endregion

    #region Private State

    // AudioSources (created at runtime)
    private AudioSource ambientSource;
    private AudioSource tensionSource;
    private AudioSource excitementSource;
    private AudioSource celebrationSource;
    private AudioSource whistleSource;

    // State tracking
    private float currentTensionTarget = 0f;
    private float lastExcitementTime = -10f;
    private float lastDangerousPlayTime = -10f;
    private bool isCelebrating = false;
    private float celebrationTimer = 0f;
    private int ambientClipIndex = 0;

    // Field boundaries (cached)
    private float leftGoalX;
    private float rightGoalX;
    private bool fieldBoundsInitialized = false;

    #endregion

    #region Lifecycle

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Destroy(this);
            return;
        }

        CreateAudioSources();
        LoadVolumeSettings();
    }

    private void OnEnable()
    {
        BallBehavior.OnBallImpact += HandleBallImpact;
        GolController.OnGoalScored += HandleGoalScored;
        MatchController.OnMatchStart += HandleMatchStart;
        MatchController.OnMatchEnd += HandleMatchEnd;
    }

    private void OnDisable()
    {
        BallBehavior.OnBallImpact -= HandleBallImpact;
        GolController.OnGoalScored -= HandleGoalScored;
        MatchController.OnMatchStart -= HandleMatchStart;
        MatchController.OnMatchEnd -= HandleMatchEnd;
    }

    private void Update()
    {
        if (!fieldBoundsInitialized)
            InitializeFieldBounds();

        UpdateTensionLayer();
        UpdateCelebrationFade();
        UpdateAmbientLoop();
    }

    #endregion

    #region Initialization

    private void CreateAudioSources()
    {
        ambientSource = CreateSource("AmbientCrowd", true);
        tensionSource = CreateSource("TensionLayer", true);
        excitementSource = CreateSource("ExcitementSFX", false);
        celebrationSource = CreateSource("Celebration", false);
        whistleSource = CreateSource("Whistle", false);
    }

    private AudioSource CreateSource(string name, bool loop)
    {
        GameObject child = new GameObject($"AudioSource_{name}");
        child.transform.SetParent(transform);
        AudioSource source = child.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f; // 2D
        return source;
    }

    private void InitializeFieldBounds()
    {
        // Find goal triggers to determine field X extents
        GameObject leftGoal = GameObject.FindWithTag("LeftGoalTrigger");
        GameObject rightGoal = GameObject.FindWithTag("RightGoalTrigger");

        if (leftGoal != null && rightGoal != null)
        {
            leftGoalX = leftGoal.transform.position.x;
            rightGoalX = rightGoal.transform.position.x;
            fieldBoundsInitialized = true;
        }
        else
        {
            // Fallback: use screen bounds
            leftGoalX = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
            rightGoalX = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;
            fieldBoundsInitialized = true;
        }
    }

    #endregion

    #region Ambient Layer

    private void UpdateAmbientLoop()
    {
        if (ambientSource == null) return;
        if (ambientCrowdClips == null || ambientCrowdClips.Length == 0) return;

        if (!ambientSource.isPlaying)
        {
            ambientClipIndex = (ambientClipIndex + 1) % ambientCrowdClips.Length;
            ambientSource.clip = ambientCrowdClips[ambientClipIndex];
            ambientSource.volume = ambientBaseVolume * crowdVolume * masterVolume;
            ambientSource.Play();
        }
        else
        {
            ambientSource.volume = ambientBaseVolume * crowdVolume * masterVolume;
        }
    }

    #endregion

    #region Tension Layer

    private void UpdateTensionLayer()
    {
        if (tensionSource == null) return;

        // Determine tension target based on ball position
        UpdateTensionTarget();

        // Smoothly fade tension volume
        float currentVol = tensionSource.volume / Mathf.Max(0.001f, tensionMaxVolume * crowdVolume * masterVolume);
        float speed = (currentTensionTarget > currentVol) ? tensionFadeInSpeed : tensionFadeOutSpeed;
        float newNormalized = Mathf.MoveTowards(currentVol, currentTensionTarget, speed * Time.deltaTime);
        tensionSource.volume = newNormalized * tensionMaxVolume * crowdVolume * masterVolume;

        // Start/stop tension clip
        if (newNormalized > 0.01f && !tensionSource.isPlaying)
        {
            if (tensionClips != null && tensionClips.Length > 0)
            {
                tensionSource.clip = tensionClips[Random.Range(0, tensionClips.Length)];
                tensionSource.Play();
            }
            else if (ambientCrowdClips != null && ambientCrowdClips.Length > 0)
            {
                // Fallback: reuse ambient at higher pitch for tension
                tensionSource.clip = ambientCrowdClips[Random.Range(0, ambientCrowdClips.Length)];
                tensionSource.pitch = 1.2f;
                tensionSource.Play();
            }
        }
        else if (newNormalized <= 0.01f && tensionSource.isPlaying)
        {
            tensionSource.Stop();
            tensionSource.pitch = 1f;
        }
    }

    private void UpdateTensionTarget()
    {
        if (!fieldBoundsInitialized || !MatchController.instance.ballInGame)
        {
            currentTensionTarget = 0f;
            return;
        }

        GameObject ball = GameObject.FindWithTag("Ball");
        if (ball == null)
        {
            currentTensionTarget = 0f;
            return;
        }

        float ballX = ball.transform.position.x;
        float fieldWidth = Mathf.Abs(rightGoalX - leftGoalX);
        if (fieldWidth < 0.1f) fieldWidth = 20f; // safety

        // Normalize ball position: 0 = leftGoal, 1 = rightGoal
        float normalizedX = Mathf.InverseLerp(leftGoalX, rightGoalX, ballX);

        // Tension rises when ball is in either attacking third (near either goal)
        float distFromNearestGoal = Mathf.Min(normalizedX, 1f - normalizedX);

        // 0 = at goal, 0.5 = center
        // Tension activates when ball is in the outer 30% of the field
        if (distFromNearestGoal < 0.3f)
        {
            currentTensionTarget = Mathf.InverseLerp(0.3f, 0f, distFromNearestGoal);

            // Boost tension if ball is moving fast
            Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
            if (ballRb != null && ballRb.linearVelocity.magnitude > 15f)
            {
                currentTensionTarget = Mathf.Min(1f, currentTensionTarget * 1.3f);
            }
        }
        else
        {
            currentTensionTarget = 0f;
        }
    }

    #endregion

    #region Celebration

    private void HandleGoalScored(string goalTag)
    {
        isCelebrating = true;
        celebrationTimer = celebrationDuration;

        // Play celebration clip
        if (celebrationClips != null && celebrationClips.Length > 0)
        {
            AudioClip clip = celebrationClips[Random.Range(0, celebrationClips.Length)];
            celebrationSource.PlayOneShot(clip, celebrationVolume * crowdVolume * masterVolume);
        }

        // Boost ambient volume during celebration
        if (ambientSource != null)
        {
            ambientSource.volume = ambientBaseVolume * 2.5f * crowdVolume * masterVolume;
        }

        // Stop tension (celebration overrides)
        currentTensionTarget = 0f;
    }

    private void UpdateCelebrationFade()
    {
        if (!isCelebrating) return;

        celebrationTimer -= Time.deltaTime;
        if (celebrationTimer <= 0f)
        {
            isCelebrating = false;
            // Ambient will restore to normal volume on next UpdateAmbientLoop
        }
    }

    #endregion

    #region Near-Miss & Dangerous Play Detection

    private void HandleBallImpact(BallImpactEventArgs impact)
    {
        if (impact.ImpactLevel < 0) return;

        // Near-miss: ball hits goal wall at high velocity
        if (impact.Type == CollisionType.Wall && impact.ImpactSource != null)
        {
            string tag = impact.ImpactSource.tag;
            if ((tag == "TopGoalWall" || tag == "BottomGoalWall") 
                && impact.ImpactForce >= nearMissVelocityThreshold)
            {
                PlayExcitementSound(crowdGaspClips);
            }
        }

        // GK save detection: high-velocity ball deflected by a figure on GoalKepperRod
        if (impact.Type == CollisionType.Figure && impact.ImpactForce >= nearMissVelocityThreshold
            && impact.ImpactSource != null)
        {
            Transform parent = impact.ImpactSource.transform.parent;
            if (parent != null && parent.name == "GoalKepperRod")
            {
                PlayExcitementSound(saveReactionClips);
            }
        }

        // Dangerous play: high-velocity figure hit near a goal
        if (impact.Type == CollisionType.Figure && impact.ImpactForce >= dangerousPlayVelocityThreshold)
        {
            CheckDangerousPlay(impact.ContactPoint);
        }
    }

    private void CheckDangerousPlay(Vector2 ballPos)
    {
        if (!fieldBoundsInitialized) return;
        if (Time.time - lastDangerousPlayTime < dangerousPlayCooldown) return;

        float distToLeft = Mathf.Abs(ballPos.x - leftGoalX);
        float distToRight = Mathf.Abs(ballPos.x - rightGoalX);
        float distToNearestGoal = Mathf.Min(distToLeft, distToRight);

        if (distToNearestGoal <= dangerousPlayGoalProximity)
        {
            if (dangerousPlayClip != null)
            {
                excitementSource.PlayOneShot(dangerousPlayClip, excitementVolume * crowdVolume * masterVolume);
                lastDangerousPlayTime = Time.time;
            }
        }
    }

    private void PlayExcitementSound(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;
        if (Time.time - lastExcitementTime < excitementCooldown) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        excitementSource.PlayOneShot(clip, excitementVolume * crowdVolume * masterVolume);
        lastExcitementTime = Time.time;
    }

    /// <summary>
    /// Call from external systems to trigger a save reaction (e.g., GK intercept at high velocity)
    /// </summary>
    public void PlaySaveReaction()
    {
        PlayExcitementSound(saveReactionClips);
    }

    #endregion

    #region Match Lifecycle

    private void HandleMatchStart()
    {
        // Play kickoff whistle
        if (kickoffWhistleClip != null)
        {
            whistleSource.PlayOneShot(kickoffWhistleClip, whistleVolume * sfxVolume * masterVolume);
        }

        // Start ambient crowd
        if (ambientCrowdClips != null && ambientCrowdClips.Length > 0)
        {
            ambientClipIndex = 0;
            ambientSource.clip = ambientCrowdClips[0];
            ambientSource.volume = ambientBaseVolume * crowdVolume * masterVolume;
            ambientSource.Play();
        }
    }

    private void HandleMatchEnd()
    {
        // Play end whistle
        AudioClip whistle = endWhistleClip != null ? endWhistleClip : kickoffWhistleClip;
        if (whistle != null)
        {
            whistleSource.pitch = endWhistleClip != null ? 1f : 0.85f; // lower pitch if reusing kickoff
            whistleSource.PlayOneShot(whistle, whistleVolume * sfxVolume * masterVolume);
        }

        // Fade out crowd
        currentTensionTarget = 0f;
        if (ambientSource != null) ambientSource.Stop();
        if (tensionSource != null) tensionSource.Stop();
    }

    #endregion

    #region Volume Controls

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("AudioMasterVolume", masterVolume);
        PlayerPrefs.Save();
        PropagateVolumeToSFX();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("AudioSFXVolume", sfxVolume);
        PlayerPrefs.Save();
        PropagateVolumeToSFX();
    }

    public void SetCrowdVolume(float volume)
    {
        crowdVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("AudioCrowdVolume", crowdVolume);
        PlayerPrefs.Save();
    }

    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("AudioMasterVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("AudioSFXVolume", 1f);
        crowdVolume = PlayerPrefs.GetFloat("AudioCrowdVolume", 1f);
        PropagateVolumeToSFX();
    }

    /// <summary>
    /// Push SFX volume to ball sound controllers
    /// </summary>
    private void PropagateVolumeToSFX()
    {
        BallSoundsController[] ballSounds = FindObjectsByType<BallSoundsController>(FindObjectsSortMode.None);
        foreach (var bs in ballSounds)
        {
            bs.SetSFXVolume(sfxVolume * masterVolume);
        }
    }

    #endregion
}

using System.Collections;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Collider2D))]
public class FoosballFigureShootAction : MonoBehaviour
{
    [Header("Shot Configuration")]
    [SerializeField] private float shotActiveWindow = 0.3f;
    [SerializeField] private float shotCooldown = 0.5f;

    [Header("Shot Force Levels")]
    [Tooltip("Shot force for quick taps (less than 1 second charge)")]
    [SerializeField] private float lightShotForce = 30f;
    [Tooltip("Shot force for medium charge (between 1-2 seconds)")]
    [SerializeField] private float mediumShotForce = 60f;
    [Tooltip("Shot force for full charge (over 2 seconds)")]
    [SerializeField] private float heavyShotForce = 100f;

    [Header("Charge Thresholds")]
    [Tooltip("Charge time threshold to start playing particles (heavy shot level)")]
    [SerializeField] private float heavyShotChargeThreshold = 3.0f;

    [Header("Impact Physics")]
    [Tooltip("Multiplier for vertical force based on impact point")]
    [SerializeField] private float verticalForceMultiplier = 1.5f;
    [Tooltip("Maximum vertical angle deviation in degrees")]
    [SerializeField] private float maxVerticalAngle = 45f;

    [Header("Figure Side Effects")]
    [Tooltip("Particle effect while charging the shot")]
    [SerializeField] private ParticleSystem chargingParticles;
    [Tooltip("Particle effect from figure's kick motion (not ball impact)")]
    [SerializeField] private ParticleSystem figureKickEffect;
    [SerializeField] private AudioSource shotSound;
    [SerializeField] private AudioClip[] shotSoundVariations;

    [Header("Arcade Feel")]
    [Tooltip("Camera shake intensity based on shot power")]
    [SerializeField] private float lightShakeMagnitude = 2f;
    [SerializeField] private float mediumShakeMagnitude = 5f;
    [SerializeField] private float heavyShakeMagnitude = 8f;
    [SerializeField] private float shakeDuration = 0.5f;

    [Tooltip("Time slow effect on powerful shots")]
    [SerializeField] private bool enableTimeSlowOnHeavyShot = true;
    [SerializeField] private float timeSlowFactor = 0.3f;
    [SerializeField] private float timeSlowDuration = 0.1f;

    // Internal state
    private bool isShotActive = false;
    private bool isOnCooldown = false;
    private bool isCharging = false;
    private bool areParticlesPlaying = false;
    private float currentChargeTime = 0f;
    private float shotPower = 0f;
    private Vector2 shotDirection = Vector2.zero;
    private int shotLevel = 0; // 0 = light, 1 = medium, 2 = heavy

    // References
    private FoosballFigureAnimationController figureController;
    private CinemachineImpulseSource impulseSource;
    private Camera mainCamera;

    private void Awake()
    {
        figureController = GetComponent<FoosballFigureAnimationController>();
        
        // Setup camera shake
        impulseSource = GetComponent<CinemachineImpulseSource>();
        if (impulseSource == null)
        {
            impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
            impulseSource.m_ImpulseDefinition.m_ImpulseDuration = shakeDuration;
            impulseSource.m_DefaultVelocity = new Vector3(1f, 1f, 0);
            impulseSource.m_ImpulseDefinition.m_ImpulseChannel = 0;
            impulseSource.m_ImpulseDefinition.m_CustomImpulseShape = AnimationCurve.EaseInOut(0, 1, 1, 0);
            impulseSource.m_ImpulseDefinition.m_DissipationDistance = 100;
        }

        mainCamera = Camera.main;
    }

    /// <summary>
    /// Start charging the shot - called from rod controller
    /// Particles will only start when heavy shot threshold is reached
    /// </summary>
    public void StartCharging()
    {
        if (isOnCooldown) return;

        isCharging = true;
        currentChargeTime = 0f;
        areParticlesPlaying = false;
        
        // Do NOT start particles here - wait for UpdateChargeTime to reach threshold
    }

    /// <summary>
    /// Update the charge time - called from rod controller during charging
    /// Starts particles when heavy shot threshold is reached
    /// </summary>
    public void UpdateChargeTime(float chargeTime)
    {
        if (!isCharging) return;
        
        currentChargeTime = chargeTime;
        
        // Start particles only when heavy shot threshold is reached
        if (chargeTime >= heavyShotChargeThreshold && !areParticlesPlaying)
        {
            if (chargingParticles != null)
            {
                chargingParticles.Play();
                areParticlesPlaying = true;
            }
        }
    }

    /// <summary>
    /// Stop charging and release the shot
    /// </summary>
    public void StopCharging()
    {
        isCharging = false;
        currentChargeTime = 0f;

        // Stop charging particles
        StopChargingParticles();
    }

    /// <summary>
    /// Force stop all particles - called when rod becomes inactive
    /// </summary>
    public void ForceStopAllParticles()
    {
        StopChargingParticles();
        
        // Also stop kick effect if playing
        if (figureKickEffect != null && figureKickEffect.isPlaying)
        {
            figureKickEffect.Stop();
            figureKickEffect.Clear();
        }
    }

    private void StopChargingParticles()
    {
        areParticlesPlaying = false;
        
        if (chargingParticles != null && chargingParticles.isPlaying)
        {
            chargingParticles.Stop();
            chargingParticles.Clear();
        }
    }

    /// <summary>
    /// Call this method when a shot should be prepared, typically after a kick animation is triggered
    /// </summary>
    /// <param name="power">Shot force</param>
    /// <param name="teamSide">Team side to determine direction</param>
    /// <param name="chargeTime">Total time the shot was charged</param>
    public void PrepareShot(float power, TeamSide teamSide, float chargeTime = 0f)
    {
        if (isOnCooldown) return;

        // Stop charging particles if still playing
        StopCharging();

        // Determine shot level based on charge time
        DetermineShotLevel(chargeTime);

        // Apply the corresponding force based on level
        SetShotPowerByLevel();

        // Calculate shot direction
        CalculateShotDirection(teamSide);

        // Activate the shot for a short window
        StartCoroutine(ActivateShotWindow());
    }

    private void DetermineShotLevel(float chargeTime)
    {
        if (chargeTime < 1.0f)
        {
            shotLevel = 0; // Light shot
        }
        else if (chargeTime < 2.0f)
        {
            shotLevel = 1; // Medium shot
        }
        else
        {
            shotLevel = 2; // Heavy shot
        }
    }

    private void SetShotPowerByLevel()
    {
        switch (shotLevel)
        {
            case 0:
                shotPower = lightShotForce;
                break;
            case 1:
                shotPower = mediumShotForce;
                break;
            case 2:
                shotPower = heavyShotForce;
                break;
        }
    }

    private IEnumerator ActivateShotWindow()
    {
        // Activate shot capability
        isShotActive = true;

        // Track attempt for telemetry
        PhysicsTelemetryLogger.LogShotAttempt(shotLevel);

        // Wait for the shot window duration
        yield return new WaitForSeconds(shotActiveWindow);

        // Log whether the shot actually connected
        if (isShotActive)
        {
            // Window expired without hitting ball
            AIDebugLogger.Log(transform.parent != null ? transform.parent.name : name,
                "SHOT_MISSED", $"Shot window expired ({shotActiveWindow:F2}s) — ball not contacted, level:{shotLevel}");
        }

        // Deactivate shot if it wasn't used
        isShotActive = false;

        // Apply cooldown
        isOnCooldown = true;
        yield return new WaitForSeconds(shotCooldown);
        isOnCooldown = false;
    }

    private void CalculateShotDirection(TeamSide teamSide)
    {
        // Base direction depends on team side
        Vector2 direction = teamSide == TeamSide.LeftTeam ? Vector2.right : Vector2.left;

        // Add some vertical randomness based on shot level (less randomness for higher power shots)
        float randomness = Mathf.Lerp(0.3f, 0.05f, (float)shotLevel / 2f);
        direction += new Vector2(0, Random.Range(-randomness, randomness));
        direction.Normalize();

        shotDirection = direction;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        AttemptShot(collision);
    }

    // Catches balls already in contact when shot window activates
    private void OnCollisionStay2D(Collision2D collision)
    {
        AttemptShot(collision);
    }

    private void AttemptShot(Collision2D collision)
    {
        if (!isShotActive) return;

        GameObject collidedObject = collision.gameObject;
        if (!collidedObject.CompareTag("Ball")) return;

        // Get the ball's rigidbody
        Rigidbody2D ballRb = collidedObject.GetComponent<Rigidbody2D>();
        if (ballRb == null) return;

        // Get the contact point to determine impact location
        ContactPoint2D contact = collision.GetContact(0);

        // Apply force to ball with position-based adjustments
        ApplyForceToBall(ballRb, contact);
        PlayFigureEffects(contact.point);

        // Log successful ball contact
        string rodName = transform.parent != null ? transform.parent.name : name;
        AIDebugLogger.Log(rodName, "BALL_HIT",
            $"Shot connected! level:{shotLevel} power:{shotPower:F0} contact:({contact.point.x:F1},{contact.point.y:F1})");

        PhysicsTelemetryLogger.LogShotConnected();

        // Apply camera shake based on shot level
        ApplyCameraShake(shotLevel);

        // Apply time slow for heavy shots
        if (enableTimeSlowOnHeavyShot && shotLevel == 2)
        {
            StartCoroutine(TimeSlowEffect());
        }

        // Deactivate shooting to prevent multiple shots
        isShotActive = false;
    }

    private void ApplyForceToBall(Rigidbody2D ballRb, ContactPoint2D contact)
    {
        // Calculate impact position relative to figure center
        Vector2 impactPoint = contact.point;
        Vector2 figureCenter = transform.position;
        float verticalOffset = impactPoint.y - figureCenter.y;

        // Normalize the offset based on figure height (approximate the collider height)
        Collider2D collider = GetComponent<Collider2D>();
        float figureHeight = collider.bounds.size.y;
        float normalizedOffset = figureHeight > 0 ? verticalOffset / (figureHeight * 0.5f) : 0;

        // Clamp the normalized offset between -1 and 1
        normalizedOffset = Mathf.Clamp(normalizedOffset, -1f, 1f);

        // Adjust shot direction based on impact point
        Vector2 adjustedDirection = shotDirection;

        // Convert normalized offset to angle (in degrees)
        // Invert the angle for right team to maintain correct ball trajectory
        float angleMultiplier = shotDirection.x > 0 ? 1f : -1f; // 1 for right (left team), -1 for left (right team)
        float verticalAngle = normalizedOffset * maxVerticalAngle * angleMultiplier;

        // Apply the rotation to our direction
        adjustedDirection = Quaternion.Euler(0, 0, verticalAngle) * adjustedDirection;

        // Apply force with the adjusted direction
        ballRb.linearVelocity = adjustedDirection * shotPower;
    }

    private void PlayFigureEffects(Vector2 contactPoint)
    {
        // Play figure-side kick effect (foot motion, etc.)
        if (figureKickEffect != null)
        {
            figureKickEffect.transform.position = contactPoint;
            figureKickEffect.Play();
        }
        
        // Play sound
        if (shotSound != null && !shotSound.isPlaying)
        {
            if (shotSoundVariations != null && shotSoundVariations.Length > shotLevel && shotSoundVariations[shotLevel] != null)
            {
                shotSound.clip = shotSoundVariations[shotLevel];
            }

            shotSound.pitch = Random.Range(0.9f + (shotLevel * 0.1f), 1.1f + (shotLevel * 0.1f));
            shotSound.volume = 0.6f + (shotLevel * 0.2f);
            shotSound.Play();
        }
    }

    private void ApplyCameraShake(int impactLevel)
    {
        float shakeMagnitude = impactLevel switch
        {
            0 => lightShakeMagnitude,
            1 => mediumShakeMagnitude,
            2 => heavyShakeMagnitude,
            _ => 0
        };

        if (impulseSource != null && shakeMagnitude > 0)
        {
            impulseSource.m_ImpulseDefinition.m_AmplitudeGain = shakeMagnitude;
            impulseSource.GenerateImpulse();
        }
    }

    private IEnumerator TimeSlowEffect()
    {
        float originalTimeScale = Time.timeScale;
        float originalFixedDeltaTime = Time.fixedDeltaTime;

        Time.timeScale = timeSlowFactor;
        Time.fixedDeltaTime = Time.fixedDeltaTime * timeSlowFactor;

        yield return new WaitForSecondsRealtime(timeSlowDuration);

        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime;
    }

    // Call this from parent controller to cancel an active shot
    public void CancelShot()
    {
        isShotActive = false;
        StopCharging();
    }

    /// <summary>
    /// Apply values from a physics preset at runtime.
    /// </summary>
    public void ApplyPreset(PhysicsPreset preset)
    {
        lightShotForce = preset.lightShotForce;
        mediumShotForce = preset.mediumShotForce;
        heavyShotForce = preset.heavyShotForce;
        maxVerticalAngle = preset.maxVerticalAngle;
        shotActiveWindow = preset.shotActiveWindow;
        shotCooldown = preset.shotCooldown;
        lightShakeMagnitude = preset.cameraShakeMultiplier * 2f;
        mediumShakeMagnitude = preset.cameraShakeMultiplier * 5f;
        heavyShakeMagnitude = preset.cameraShakeMultiplier * 8f;
    }

    // Getters for debugging and analytics
    public int GetShotLevel() => shotLevel;
    public float GetShotPower() => shotPower;
    public bool IsCharging() => isCharging;
}

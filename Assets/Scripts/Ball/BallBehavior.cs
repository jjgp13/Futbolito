using System;
using System.Collections;
using UnityEngine;

public class BallImpactEventArgs
{
    public Vector2 ContactPoint { get; set; }
    public float ImpactForce { get; set; }
    public int ImpactLevel { get; set; } // 0=light, 1=medium, 2=heavy
    public GameObject ImpactSource { get; set; }
    public CollisionType Type { get; set; }
}

public enum CollisionType
{
    Figure,
    Wall,
    Other
}

public class BallBehavior : MonoBehaviour
{
    Rigidbody2D rb;
    
    [Header("Particle Effects")]
    public ParticleSystem ballExplosion;
    public ParticleSystem kickImpactParticles;
    public ParticleSystem wallImpactParticles;
    public ParticleSystem stopBallParticles;

    [Header("Impact Thresholds")]
    [Tooltip("Velocity thresholds for impact levels")]
    public float lightImpactThreshold = 10f;
    public float mediumImpactThreshold = 30f;
    public float heavyImpactThreshold = 50f;

    [Header("Trail Effect")]
    public TrailRenderer ballTrail;
    [Tooltip("Minimum velocity to show trail")]
    public float minVelocityForTrail = 5f;
    [Tooltip("Maximum trail width at high velocity")]
    public float maxTrailWidth = 0.5f;
    [Tooltip("Velocity at which trail reaches maximum width")]
    public float maxTrailVelocity = 50f;

    public float timeToWaitToStart;
    private float inactiveBallTime;
    private bool kickOff;

    [Header("Speed Limit")]
    [Tooltip("Maximum ball speed (0 = unlimited)")]
    public float maxBallSpeed = 0f;

    [Header("Restarting ball values")]
    [Tooltip("Enable/disable automatic ball respawn when inactive")]
    public bool autoRespawnWhenInactive = true;

    [Tooltip("Time in seconds before respawning an inactive ball")]
    public int timeInactiveToRespawn;

    [Tooltip("Minimum velocity threshold for considering the ball inactive")]
    public float minVelocityLimit;

    [Tooltip("Initial force range for ball movement")]
    public float iniMinForce, iniMaxForce;

    // Event for external systems to listen to
    public static event Action<BallImpactEventArgs> OnBallImpact;

    void Start()
    {
        //Set sprite of the ball selected
        if (MatchInfo.instance != null)
        {
            GetComponent<SpriteRenderer>().sprite = MatchInfo.instance.ballSelected;
        }

        //Get Rigibody
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        Invoke("AddInitialVelocity", timeToWaitToStart);

        inactiveBallTime = 0;
        kickOff = false;
        
        // Setup trail
        InitializeTrail();
    }

    private void InitializeTrail()
    {
        if (ballTrail == null)
        {
            ballTrail = GetComponent<TrailRenderer>();
            if (ballTrail == null)
            {
                ballTrail = gameObject.AddComponent<TrailRenderer>();
                ConfigureTrailRenderer();
            }
        }
        
        // Start with trail disabled
        ballTrail.emitting = false;
    }

    private void ConfigureTrailRenderer()
    {
        ballTrail.time = 0.3f;
        ballTrail.startWidth = 0.2f;
        ballTrail.endWidth = 0.05f;
        ballTrail.material = new Material(Shader.Find("Sprites/Default"));
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.white, 0.0f), 
                new GradientColorKey(new Color(1f, 0.8f, 0.2f), 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.8f, 0.0f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        ballTrail.colorGradient = gradient;
    }

    private void FixedUpdate()
    {
        // Check if ball is inactive (very low velocity)
        bool ballIsInactive = rb.linearVelocity.x > -minVelocityLimit &&
                              rb.linearVelocity.x < minVelocityLimit &&
                              rb.linearVelocity.y > -minVelocityLimit &&
                              rb.linearVelocity.y < minVelocityLimit;

        if (ballIsInactive && kickOff)
        {
            if (autoRespawnWhenInactive)
            {
                inactiveBallTime += Time.deltaTime;

                if (timeInactiveToRespawn - inactiveBallTime <= 3)
                {
                    MatchController.instance.timeInactiveBallPanel.SetActive(true);
                    MatchController.instance.restartingBallTimeText.text = "RESTARTING BALL IN: " + (timeInactiveToRespawn - inactiveBallTime).ToString("0");
                }
            }
            else
            {
                if (stopBallParticles != null && !stopBallParticles.isPlaying)
                {
                    stopBallParticles.Play();
                }
            }
        }
        else
        {
            MatchController.instance.timeInactiveBallPanel.SetActive(false);
            inactiveBallTime = 0;

            if (stopBallParticles != null && stopBallParticles.isPlaying)
            {
                stopBallParticles.Stop();
            }
        }

        if (autoRespawnWhenInactive && inactiveBallTime >= timeInactiveToRespawn)
        {
            RespawnBall();
        }
        
        // Update trail effect
        UpdateTrailEffect();

        // Clamp ball speed if limit is set
        if (maxBallSpeed > 0f && rb.linearVelocity.magnitude > maxBallSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxBallSpeed;
            PhysicsTelemetryLogger.LogSpeedClamped();
        }
    }

    private void UpdateTrailEffect()
    {
        if (ballTrail == null) return;
        
        float currentVelocity = rb.linearVelocity.magnitude;
        
        if (currentVelocity > minVelocityForTrail)
        {
            ballTrail.emitting = true;
            
            // Scale trail width based on velocity
            float velocityRatio = Mathf.Clamp01(currentVelocity / maxTrailVelocity);
            float trailWidth = Mathf.Lerp(0.1f, maxTrailWidth, velocityRatio);
            
            ballTrail.startWidth = trailWidth;
            ballTrail.endWidth = trailWidth * 0.3f;
            ballTrail.time = Mathf.Lerp(0.2f, 0.5f, velocityRatio);
            
            // Change color based on velocity
            Gradient gradient = new Gradient();
            if (currentVelocity > 40f)
            {
                // Red trail for very fast
                gradient.SetKeys(
                    new GradientColorKey[] { 
                        new GradientColorKey(new Color(1f, 0.3f, 0.3f), 0.0f), 
                        new GradientColorKey(new Color(1f, 0.5f, 0.2f), 1.0f) 
                    },
                    new GradientAlphaKey[] { 
                        new GradientAlphaKey(0.9f, 0.0f), 
                        new GradientAlphaKey(0.0f, 1.0f) 
                    }
                );
            }
            else if (currentVelocity > 20f)
            {
                // Yellow trail for medium speed
                gradient.SetKeys(
                    new GradientColorKey[] { 
                        new GradientColorKey(new Color(1f, 1f, 0.3f), 0.0f), 
                        new GradientColorKey(new Color(1f, 0.8f, 0.2f), 1.0f) 
                    },
                    new GradientAlphaKey[] { 
                        new GradientAlphaKey(0.8f, 0.0f), 
                        new GradientAlphaKey(0.0f, 1.0f) 
                    }
                );
            }
            else
            {
                // Blue/white trail for slower
                gradient.SetKeys(
                    new GradientColorKey[] { 
                        new GradientColorKey(Color.white, 0.0f), 
                        new GradientColorKey(new Color(0.5f, 0.8f, 1f), 1.0f) 
                    },
                    new GradientAlphaKey[] { 
                        new GradientAlphaKey(0.7f, 0.0f), 
                        new GradientAlphaKey(0.0f, 1.0f) 
                    }
                );
            }
            ballTrail.colorGradient = gradient;
        }
        else
        {
            ballTrail.emitting = false;
        }
    }

    public void RespawnBall()
    {
        MatchController.instance.timeInactiveBallPanel.SetActive(false);
        MatchController.instance.SpawnBall();
        Destroy(gameObject);
    }

    void AddInitialVelocity()
    {
        float xVel = UnityEngine.Random.Range(iniMinForce, iniMaxForce);
        float yVel = UnityEngine.Random.Range(iniMinForce, iniMaxForce);
        switch (UnityEngine.Random.Range(0, 4))
        {
            case 0:
                rb.AddForce(new Vector2(xVel, yVel), ForceMode2D.Impulse);
                break;
            case 1:
                rb.AddForce(new Vector2(-xVel, yVel), ForceMode2D.Impulse);
                break;
            case 2:
                rb.AddForce(new Vector2(-xVel, -yVel), ForceMode2D.Impulse);
                break;
            case 3:
                rb.AddForce(new Vector2(xVel, -yVel), ForceMode2D.Impulse);
                break;
        }
        kickOff = true;
        MatchController.instance.ballInGame = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Get impact information
        Vector2 contactPoint = collision.contacts[0].point;
        float impactVelocity = collision.relativeVelocity.magnitude;
        
        // Determine collision type
        CollisionType collisionType = DetermineCollisionType(collision.gameObject);
        
        // Calculate impact level
        int impactLevel = CalculateImpactLevel(impactVelocity);
        
        // Create event args
        BallImpactEventArgs eventArgs = new BallImpactEventArgs
        {
            ContactPoint = contactPoint,
            ImpactForce = impactVelocity,
            ImpactLevel = impactLevel,
            ImpactSource = collision.gameObject,
            Type = collisionType
        };
        
        // Trigger event for external systems (camera shake, sound, etc.)
        OnBallImpact?.Invoke(eventArgs);
        
        // Play particle effects based on collision type
        PlayImpactParticles(eventArgs);
    }

    private CollisionType DetermineCollisionType(GameObject collidedObject)
    {
        if (collidedObject.CompareTag("PlayerPaddle") || collidedObject.CompareTag("NPCPaddle"))
        {
            return CollisionType.Figure;
        }
        else if (collidedObject.CompareTag("Wall") || 
                 collidedObject.CompareTag("TopWall") || 
                 collidedObject.CompareTag("BottomWall"))
        {
            return CollisionType.Wall;
        }
        
        return CollisionType.Other;
    }

    private int CalculateImpactLevel(float velocity)
    {
        if (velocity >= heavyImpactThreshold)
            return 2; // Heavy
        else if (velocity >= mediumImpactThreshold)
            return 1; // Medium
        else if (velocity >= lightImpactThreshold)
            return 0; // Light
        
        return -1; // Too weak, no effect
    }

    private void PlayImpactParticles(BallImpactEventArgs impact)
    {
        if (impact.ImpactLevel < 0) return; // Too weak
        
        ParticleSystem particleSystem = null;
        
        // Select appropriate particle system
        switch (impact.Type)
        {
            case CollisionType.Figure:
                particleSystem = kickImpactParticles;
                break;
            case CollisionType.Wall:
                particleSystem = wallImpactParticles;
                break;
            default:
                return; // No particles for other collisions
        }
        
        if (particleSystem == null) return;
        
        // Position particles at contact point
        particleSystem.transform.position = impact.ContactPoint;
        
        // Configure particle system based on impact
        ConfigureParticleSystem(particleSystem, impact);
        
        // Play particles
        particleSystem.Play();
        
        // Temporarily boost trail after strong impact
        if (impact.ImpactLevel >= 1)
        {
            StartCoroutine(BoostTrailAfterImpact(impact.ImpactLevel));
        }
    }

    private void ConfigureParticleSystem(ParticleSystem particles, BallImpactEventArgs impact)
    {
        var main = particles.main;
        var emission = particles.emission;
        
        // Scale effect based on impact force
        float forceRatio = Mathf.Clamp01(impact.ImpactForce / heavyImpactThreshold);
        
        // Adjust particle speed
        main.startSpeed = Mathf.Lerp(3f, 15f, forceRatio);
        
        // Adjust particle size
        main.startSize = Mathf.Lerp(0.2f, 0.8f, forceRatio);
        
        // Adjust particle count
        int particleCount = impact.Type == CollisionType.Figure 
            ? 10 + (impact.ImpactLevel * 20)  // More particles for figure kicks
            : 5 + (impact.ImpactLevel * 10);   // Fewer for walls
        
        ParticleSystem.Burst burst = emission.GetBurst(0);
        burst.count = (short)particleCount;
        emission.SetBurst(0, burst);
        
        // Set color based on impact level and type
        Color particleColor = GetImpactColor(impact);
        main.startColor = particleColor;
    }

    private Color GetImpactColor(BallImpactEventArgs impact)
    {
        // Different colors for different collision types
        if (impact.Type == CollisionType.Figure)
        {
            // Intensity-based colors for figure kicks
            switch (impact.ImpactLevel)
            {
                case 0: return new Color(0.3f, 0.8f, 1f);   // Light blue
                case 1: return new Color(1f, 1f, 0.3f);      // Yellow
                case 2: return new Color(1f, 0.3f, 0.3f);    // Red
                default: return Color.white;
            }
        }
        else // Wall collisions
        {
            // Cooler colors for wall impacts
            switch (impact.ImpactLevel)
            {
                case 0: return new Color(0.7f, 0.7f, 0.9f);  // Light gray-blue
                case 1: return new Color(0.9f, 0.9f, 1f);    // Bright white
                case 2: return new Color(1f, 0.8f, 0.3f);    // Orange
                default: return Color.white;
            }
        }
    }

    private IEnumerator BoostTrailAfterImpact(int impactLevel)
    {
        if (ballTrail == null) yield break;
        
        float originalMaxWidth = maxTrailWidth;
        maxTrailWidth *= (1f + impactLevel * 0.5f); // 50% boost per level
        
        yield return new WaitForSeconds(0.3f);
        
        maxTrailWidth = originalMaxWidth;
    }

    public void KickBall(Vector2 direction, float force)
    {
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * force;
        }
    }
}

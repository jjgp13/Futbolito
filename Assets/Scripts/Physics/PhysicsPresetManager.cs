using UnityEngine;

/// <summary>
/// Manages physics presets for the game.
/// Add this component to the MatchController GameObject in your match scene.
/// Use the Inspector buttons to switch presets at edit-time or runtime.
/// Automatically applies ball settings to newly spawned balls.
/// </summary>
public class PhysicsPresetManager : MonoBehaviour
{
    [Header("Preset Selection")]
    [Tooltip("The currently active physics preset")]
    public PhysicsPreset activePreset;

    [Header("Physics Material References")]
    [Tooltip("Drag the Ball.physicsMaterial2D here")]
    public PhysicsMaterial2D ballMaterial;
    [Tooltip("Drag the Figures.physicsMaterial2D here")]
    public PhysicsMaterial2D figureMaterial;
    [Tooltip("Drag the Walls.physicsMaterial2D here")]
    public PhysicsMaterial2D wallMaterial;

    [Header("Auto-Apply")]
    [Tooltip("Automatically apply the preset when the game starts")]
    public bool applyOnStart = true;

    private void OnEnable()
    {
        // Listen for new ball spawns so we can apply physics to them
        MatchController.OnBallSpawned += OnBallSpawned;
    }

    private void OnDisable()
    {
        MatchController.OnBallSpawned -= OnBallSpawned;
    }

    private void Start()
    {
        if (applyOnStart && activePreset != null)
        {
            ApplyPreset();
        }
    }

    private void OnBallSpawned()
    {
        if (activePreset != null)
        {
            // Ball is instantiated this frame — apply settings next frame
            Invoke(nameof(ApplyBallSettings), 0f);
        }
    }

    /// <summary>
    /// Applies the currently selected preset to all physics objects in the scene.
    /// Can be called from Inspector button or at runtime.
    /// </summary>
    public void ApplyPreset()
    {
        if (activePreset == null)
        {
            Debug.LogWarning("[PhysicsPresetManager] No preset assigned!");
            return;
        }

        // Notify telemetry logger to close current session before switching
        var telemetry = GetComponent<PhysicsTelemetryLogger>();
        if (telemetry != null)
        {
            telemetry.OnPresetChanged();
        }

        if (!AutoMatchRunner.IsAutoMode) Debug.Log($"[PhysicsPresetManager] Applying preset: {activePreset.presetName}");

        ApplyPhysicsMaterials();
        ApplyBallSettings();
        ApplyShotSettings();
    }

    private void ApplyPhysicsMaterials()
    {
        if (ballMaterial != null)
        {
            ballMaterial.friction = activePreset.surfaceFriction;
            ballMaterial.bounciness = activePreset.ballBounciness;
        }

        if (figureMaterial != null)
        {
            figureMaterial.friction = activePreset.surfaceFriction;
            figureMaterial.bounciness = activePreset.figureBounciness;
        }

        if (wallMaterial != null)
        {
            wallMaterial.friction = activePreset.surfaceFriction;
            wallMaterial.bounciness = activePreset.wallBounciness;
        }
    }

    private void ApplyBallSettings()
    {
        var ball = FindBall();
        if (ball == null) return;

        Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.useAutoMass = false;
            rb.mass = activePreset.ballMass;
            rb.linearDamping = activePreset.ballLinearDrag;
            rb.gravityScale = 0f;
        }

        BallBehavior behavior = ball.GetComponent<BallBehavior>();
        if (behavior != null)
        {
            behavior.maxBallSpeed = activePreset.maxBallSpeed;
        }
    }

    private void ApplyShotSettings()
    {
        var shootActions = FindObjectsByType<FoosballFigureShootAction>(FindObjectsSortMode.None);
        foreach (var action in shootActions)
        {
            action.ApplyPreset(activePreset);
        }
    }

    private GameObject FindBall()
    {
        GameObject ball = GameObject.FindGameObjectWithTag("Ball");
        return ball;
    }
}

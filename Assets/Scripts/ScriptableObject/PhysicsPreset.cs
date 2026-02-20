using UnityEngine;

[CreateAssetMenu(fileName = "New Physics Preset", menuName = "Futbolito/Physics Preset")]
public class PhysicsPreset : ScriptableObject
{
    [Header("Preset Info")]
    public string presetName;
    [TextArea(2, 4)]
    public string description;

    [Header("Ball Weight & Drag")]
    [Tooltip("Ball mass — heavier = harder to move, more momentum")]
    public float ballMass = 1f;
    [Tooltip("How fast the ball decelerates (HIGHEST IMPACT property)")]
    [Range(0f, 3f)]
    public float ballLinearDrag = 0.5f;

    [Header("Bounciness")]
    [Range(0f, 1f)]
    [Tooltip("Ball bounce off all surfaces")]
    public float ballBounciness = 0.9f;
    [Range(0f, 1f)]
    [Tooltip("How much energy the ball keeps when hitting figures")]
    public float figureBounciness = 0.6f;
    [Range(0f, 1f)]
    [Tooltip("How much energy the ball keeps when hitting walls")]
    public float wallBounciness = 0.9f;

    [Header("Surface Friction")]
    [Range(0f, 1f)]
    [Tooltip("Friction for all surfaces (ball, figures, walls). Higher = ball grips more on contact.")]
    public float surfaceFriction = 0f;

    [Header("Shot Forces")]
    [Tooltip("Quick tap force (used ~90% of the time by players)")]
    public float lightShotForce = 30f;
    [Tooltip("Medium charge force (1-2 sec)")]
    public float mediumShotForce = 60f;
    [Tooltip("Full charge force (2+ sec)")]
    public float heavyShotForce = 100f;

    [Header("Shot Feel")]
    [Tooltip("Max vertical angle deviation on shots (degrees)")]
    public float maxVerticalAngle = 45f;
    [Tooltip("How long the shot hitbox stays active — higher = more forgiving")]
    [Range(0.1f, 0.6f)]
    public float shotActiveWindow = 0.3f;
    [Tooltip("Cooldown between shots")]
    [Range(0.2f, 1f)]
    public float shotCooldown = 0.5f;

    [Header("Ball Speed Limit")]
    [Tooltip("Maximum ball speed (0 = unlimited)")]
    public float maxBallSpeed = 0f;

    [Header("Camera Shake")]
    [Tooltip("Camera shake intensity multiplier (1 = default)")]
    public float cameraShakeMultiplier = 1f;
}

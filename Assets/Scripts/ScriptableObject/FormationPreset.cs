using UnityEngine;

/// <summary>
/// Defines a formation layout and per-figure-count rod speed curve.
/// Used by RodConfiguration to set figure counts and by rod movement scripts to set base speed.
/// Multiple presets can be rotated via AutoMatchRunner for A/B testing.
/// </summary>
[CreateAssetMenu(fileName = "New Formation Preset", menuName = "Futbolito/Formation Preset")]
public class FormationPreset : ScriptableObject
{
    [Header("Preset Info")]
    public string presetName;
    [TextArea(2, 4)]
    public string description;

    [Header("Formation (figures per rod, GK is always 1)")]
    [Range(1, 5)] public int defense = 2;
    [Range(1, 5)] public int midfield = 5;
    [Range(1, 5)] public int attack = 3;

    [Header("Player Rod Base Speed (by figure count on rod)")]
    [Tooltip("Speed for rod with 1 figure (GK)")]
    public float playerSpeed1Fig = 10f;
    [Tooltip("Speed for rod with 2 figures")]
    public float playerSpeed2Fig = 8f;
    [Tooltip("Speed for rod with 3 figures")]
    public float playerSpeed3Fig = 6f;
    [Tooltip("Speed for rod with 4 figures")]
    public float playerSpeed4Fig = 4f;
    [Tooltip("Speed for rod with 5 figures")]
    public float playerSpeed5Fig = 2f;

    [Header("AI Rod Base Speed (by figure count on rod)")]
    [Tooltip("Speed for rod with 1 figure (GK)")]
    public float aiSpeed1Fig = 3f;
    [Tooltip("Speed for rod with 2 figures")]
    public float aiSpeed2Fig = 2.5f;
    [Tooltip("Speed for rod with 3 figures")]
    public float aiSpeed3Fig = 2f;
    [Tooltip("Speed for rod with 4 figures")]
    public float aiSpeed4Fig = 1.5f;
    [Tooltip("Speed for rod with 5 figures")]
    public float aiSpeed5Fig = 1f;

    [Header("Role Speed Multipliers (optional)")]
    [Tooltip("Multiplies the base speed for GK (both player+AI have separate sets)")]
    public float playerGoalkeeperMultiplier = 1f;
    public float playerDefenseMultiplier = 1f;
    public float playerMidfieldMultiplier = 1f;
    public float playerAttackMultiplier = 1f;

    public float aiGoalkeeperMultiplier = 1f;
    public float aiDefenseMultiplier = 1f;
    public float aiMidfieldMultiplier = 1f;
    public float aiAttackMultiplier = 1f;

    /// <summary>
    /// Returns a Formation object matching this preset's figure counts.
    /// </summary>
    public Formation ToFormation()
    {
        return new Formation { defense = this.defense, mid = this.midfield, attack = this.attack };
    }

    #region Active Preset (static, set by AutoMatchRunner or scene setup)

    private static FormationPreset _active;

    /// <summary>
    /// The currently active formation preset. When set, overrides team formations and rod speeds.
    /// Set by AutoMatchRunner during test rotation, or manually in scene.
    /// Null means use default team formations and hardcoded speeds.
    /// </summary>
    public static FormationPreset Active
    {
        get => _active;
        set
        {
            _active = value;
            if (value != null)
                Debug.Log($"[FormationPreset] Active formation set to: {value.presetName} ({value.defense}-{value.midfield}-{value.attack})");
            else
                Debug.Log("[FormationPreset] Active formation cleared (using team defaults)");
        }
    }

    #endregion

    /// <summary>
    /// Gets the player rod base speed for a given figure count.
    /// If a role is provided, the corresponding role multiplier is applied.
    /// </summary>
    public float GetPlayerSpeed(int figureCount, RodRole role = RodRole.Midfield)
    {
        float baseSpeed = figureCount switch
        {
            1 => playerSpeed1Fig,
            2 => playerSpeed2Fig,
            3 => playerSpeed3Fig,
            4 => playerSpeed4Fig,
            5 => playerSpeed5Fig,
            _ => playerSpeed3Fig
        };

        return baseSpeed * GetPlayerRoleMultiplier(role);
    }

    private float GetPlayerRoleMultiplier(RodRole role)
    {
        return role switch
        {
            RodRole.Goalkeeper => playerGoalkeeperMultiplier,
            RodRole.Defense => playerDefenseMultiplier,
            RodRole.Midfield => playerMidfieldMultiplier,
            RodRole.Attack => playerAttackMultiplier,
            _ => 1f
        };
    }

    /// <summary>
    /// Gets the AI rod base speed for a given figure count.
    /// If a role is provided, the corresponding role multiplier is applied.
    /// </summary>
    public float GetAISpeed(int figureCount, RodRole role = RodRole.Midfield)
    {
        float baseSpeed = figureCount switch
        {
            1 => aiSpeed1Fig,
            2 => aiSpeed2Fig,
            3 => aiSpeed3Fig,
            4 => aiSpeed4Fig,
            5 => aiSpeed5Fig,
            _ => aiSpeed3Fig
        };

        return baseSpeed * GetAIRoleMultiplier(role);
    }

    private float GetAIRoleMultiplier(RodRole role)
    {
        return role switch
        {
            RodRole.Goalkeeper => aiGoalkeeperMultiplier,
            RodRole.Defense => aiDefenseMultiplier,
            RodRole.Midfield => aiMidfieldMultiplier,
            RodRole.Attack => aiAttackMultiplier,
            _ => 1f
        };
    }
}

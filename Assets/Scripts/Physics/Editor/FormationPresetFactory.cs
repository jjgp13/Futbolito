using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Creates default formation presets via the Unity menu.
/// Use: Assets → Create → Futbolito → Generate Default Formation Presets
/// </summary>
public static class FormationPresetFactory
{
    private const string PRESETS_FOLDER = "Assets/Formation Presets";

    [MenuItem("Assets/Create/Futbolito/Generate Default Formation Presets")]
    public static void GenerateDefaultFormationPresets()
    {
        if (!Directory.Exists(PRESETS_FOLDER))
        {
            Directory.CreateDirectory(PRESETS_FOLDER);
            AssetDatabase.Refresh();
        }

        CreateClassic442();
        CreateWide352();
        CreateDefensive532();
        CreateAttacking235();
        CreateBalanced343();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[FormationPresetFactory] Created 5 formation presets in '{PRESETS_FOLDER}/'");
    }

    private static void CreateClassic442()
    {
        var preset = ScriptableObject.CreateInstance<FormationPreset>();
        preset.presetName = "Classic 4-4-2";
        preset.description = "Standard foosball layout. Even coverage across all zones. The baseline formation.";
        preset.defense = 4;
        preset.midfield = 4;
        preset.attack = 2;
        // Player speeds (current defaults)
        preset.playerSpeed1Fig = 10f;
        preset.playerSpeed2Fig = 8f;
        preset.playerSpeed3Fig = 6f;
        preset.playerSpeed4Fig = 4f;
        preset.playerSpeed5Fig = 2f;
        // AI speeds (current defaults)
        preset.aiSpeed1Fig = 3f;
        preset.aiSpeed2Fig = 2.5f;
        preset.aiSpeed3Fig = 2f;
        preset.aiSpeed4Fig = 1.5f;
        preset.aiSpeed5Fig = 1f;
        SavePreset(preset, "Classic 4-4-2");
    }

    private static void CreateWide352()
    {
        var preset = ScriptableObject.CreateInstance<FormationPreset>();
        preset.presetName = "Wide 3-5-2";
        preset.description = "Strong midfield control with 5 figures. Fewer defenders but fast defense rod compensates.";
        preset.defense = 3;
        preset.midfield = 5;
        preset.attack = 2;
        preset.playerSpeed1Fig = 10f;
        preset.playerSpeed2Fig = 8f;
        preset.playerSpeed3Fig = 6f;
        preset.playerSpeed4Fig = 4f;
        preset.playerSpeed5Fig = 2f;
        preset.aiSpeed1Fig = 3f;
        preset.aiSpeed2Fig = 2.5f;
        preset.aiSpeed3Fig = 2f;
        preset.aiSpeed4Fig = 1.5f;
        preset.aiSpeed5Fig = 1f;
        SavePreset(preset, "Wide 3-5-2");
    }

    private static void CreateDefensive532()
    {
        var preset = ScriptableObject.CreateInstance<FormationPreset>();
        preset.presetName = "Defensive 5-3-2";
        preset.description = "Packed defense with 5 figures. Hard to score against but slower defensive rod.";
        preset.defense = 5;
        preset.midfield = 3;
        preset.attack = 2;
        preset.playerSpeed1Fig = 10f;
        preset.playerSpeed2Fig = 8f;
        preset.playerSpeed3Fig = 6f;
        preset.playerSpeed4Fig = 4f;
        preset.playerSpeed5Fig = 2f;
        preset.aiSpeed1Fig = 3f;
        preset.aiSpeed2Fig = 2.5f;
        preset.aiSpeed3Fig = 2f;
        preset.aiSpeed4Fig = 1.5f;
        preset.aiSpeed5Fig = 1f;
        SavePreset(preset, "Defensive 5-3-2");
    }

    private static void CreateAttacking235()
    {
        var preset = ScriptableObject.CreateInstance<FormationPreset>();
        preset.presetName = "Attacking 2-3-5";
        preset.description = "All-out attack with 5 forward figures. Vulnerable at the back but overwhelming upfront.";
        preset.defense = 2;
        preset.midfield = 3;
        preset.attack = 5;
        preset.playerSpeed1Fig = 10f;
        preset.playerSpeed2Fig = 8f;
        preset.playerSpeed3Fig = 6f;
        preset.playerSpeed4Fig = 4f;
        preset.playerSpeed5Fig = 2f;
        preset.aiSpeed1Fig = 3f;
        preset.aiSpeed2Fig = 2.5f;
        preset.aiSpeed3Fig = 2f;
        preset.aiSpeed4Fig = 1.5f;
        preset.aiSpeed5Fig = 1f;
        SavePreset(preset, "Attacking 2-3-5");
    }

    private static void CreateBalanced343()
    {
        var preset = ScriptableObject.CreateInstance<FormationPreset>();
        preset.presetName = "Balanced 3-4-3";
        preset.description = "Even spread across defense and attack with midfield control. Good all-around.";
        preset.defense = 3;
        preset.midfield = 4;
        preset.attack = 3;
        preset.playerSpeed1Fig = 10f;
        preset.playerSpeed2Fig = 8f;
        preset.playerSpeed3Fig = 6f;
        preset.playerSpeed4Fig = 4f;
        preset.playerSpeed5Fig = 2f;
        preset.aiSpeed1Fig = 3f;
        preset.aiSpeed2Fig = 2.5f;
        preset.aiSpeed3Fig = 2f;
        preset.aiSpeed4Fig = 1.5f;
        preset.aiSpeed5Fig = 1f;
        SavePreset(preset, "Balanced 3-4-3");
    }

    private static void SavePreset(FormationPreset preset, string name)
    {
        string path = $"{PRESETS_FOLDER}/{name}.asset";

        if (AssetDatabase.LoadAssetAtPath<FormationPreset>(path) != null)
        {
            Debug.Log($"[FormationPresetFactory] '{name}' already exists, skipping.");
            Object.DestroyImmediate(preset);
            return;
        }

        AssetDatabase.CreateAsset(preset, path);
        Debug.Log($"[FormationPresetFactory] Created formation preset: {path}");
    }
}

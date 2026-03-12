using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Creates default physics presets via the Unity menu.
/// Use: Assets → Create → Futbolito → Generate Default Presets
/// </summary>
public static class PhysicsPresetFactory
{
    private const string PRESETS_FOLDER = "Assets/Physics Presets";

    [MenuItem("Assets/Create/Futbolito/Generate Default Presets")]
    public static void GenerateDefaultPresets()
    {
        if (!Directory.Exists(PRESETS_FOLDER))
        {
            Directory.CreateDirectory(PRESETS_FOLDER);
            AssetDatabase.Refresh();
        }

        CreateCurrentDefault();
        CreateArcadePreset();
        CreatePinballChaosPreset();
        CreateAirHockeyPreset();
        CreateSpeedDemonPreset();
        CreateCompetitivePreset();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[PhysicsPresetFactory] Created 6 presets in 'Assets/Physics Presets/'");
    }

    private static void CreateCurrentDefault()
    {
        var preset = ScriptableObject.CreateInstance<PhysicsPreset>();
        preset.presetName = "Current Default";
        preset.description = "The original game physics. Preserves all existing values as a baseline.";
        preset.ballMass = 1f;
        preset.ballLinearDrag = 0.5f;
        preset.ballBounciness = 0.9f;
        preset.figureBounciness = 0.6f;
        preset.wallBounciness = 0.9f;
        preset.surfaceFriction = 0f;
        preset.lightShotForce = 30f;
        preset.heavyShotForce = 60f;
        preset.maxVerticalAngle = 45f;
        preset.shotActiveWindow = 0.3f;
        preset.shotCooldown = 0.5f;
        preset.maxBallSpeed = 0f;
        preset.cameraShakeMultiplier = 1f;
        preset.bumpStrength = 3f;
        preset.maxBumpRange = 3.5f;
        preset.magnetPullForce = 15f;
        preset.magnetMaxBallSpeed = 8f;
        SavePreset(preset, "Current Default");
    }

    private static void CreateAirHockeyPreset()
    {
        var preset = ScriptableObject.CreateInstance<PhysicsPreset>();
        preset.presetName = "Arcade";
        preset.description = "Fast, bouncy, and explosive. Low friction, high bounciness, powerful shots with screen shake.";
        preset.ballMass = 0.8f;
        preset.ballLinearDrag = 0.3f;
        preset.ballBounciness = 0.95f;
        preset.figureBounciness = 0.7f;
        preset.wallBounciness = 0.95f;
        preset.surfaceFriction = 0f;
        preset.lightShotForce = 35f;
        preset.heavyShotForce = 70f;
        preset.maxVerticalAngle = 50f;
        preset.shotActiveWindow = 0.35f;
        preset.shotCooldown = 0.4f;
        preset.maxBallSpeed = 0f;
        preset.cameraShakeMultiplier = 1.5f;
        preset.bumpStrength = 5f;
        preset.maxBumpRange = 4.5f;
        preset.magnetPullForce = 12f;
        preset.magnetMaxBallSpeed = 10f;
        SavePreset(preset, "Arcade");
    }

    private static void CreatePinballChaosPreset()
    {
        var preset = ScriptableObject.CreateInstance<PhysicsPreset>();
        preset.presetName = "Pinball Chaos";
        preset.description = "Maximum bounciness everywhere. Ball ricochets wildly off walls and figures. Pure chaos and fun.";
        preset.ballMass = 0.6f;
        preset.ballLinearDrag = 0.1f;
        preset.ballBounciness = 1f;
        preset.figureBounciness = 0.9f;
        preset.wallBounciness = 1f;
        preset.surfaceFriction = 0f;
        preset.lightShotForce = 25f;
        preset.heavyShotForce = 50f;
        preset.maxVerticalAngle = 60f;
        preset.shotActiveWindow = 0.4f;
        preset.shotCooldown = 0.3f;
        preset.maxBallSpeed = 80f;
        preset.cameraShakeMultiplier = 1.8f;
        preset.bumpStrength = 6f;
        preset.maxBumpRange = 5f;
        preset.magnetPullForce = 10f;
        preset.magnetMaxBallSpeed = 12f;
        SavePreset(preset, "Pinball Chaos");
    }

    private static void CreateAirHockeyPreset()
    {
        var preset = ScriptableObject.CreateInstance<PhysicsPreset>();
        preset.presetName = "Air Hockey";
        preset.description = "Frictionless surface — ball glides forever. Clean rebounds, minimal energy loss.";
        preset.ballMass = 0.9f;
        preset.ballLinearDrag = 0.05f;
        preset.ballBounciness = 0.85f;
        preset.figureBounciness = 0.5f;
        preset.wallBounciness = 0.9f;
        preset.surfaceFriction = 0f;
        preset.lightShotForce = 18f;
        preset.heavyShotForce = 38f;
        preset.maxVerticalAngle = 40f;
        preset.shotActiveWindow = 0.3f;
        preset.shotCooldown = 0.45f;
        preset.maxBallSpeed = 55f;
        preset.cameraShakeMultiplier = 0.7f;
        preset.bumpStrength = 2.5f;
        preset.maxBumpRange = 3f;
        preset.magnetPullForce = 18f;
        preset.magnetMaxBallSpeed = 7f;
        SavePreset(preset, "Air Hockey");
    }

    private static void CreateSpeedDemonPreset()
    {
        var preset = ScriptableObject.CreateInstance<PhysicsPreset>();
        preset.presetName = "Speed Demon";
        preset.description = "Ultra light ball, minimal drag. Everything is fast and twitchy. Blink and you miss.";
        preset.ballMass = 0.5f;
        preset.ballLinearDrag = 0.15f;
        preset.ballBounciness = 0.88f;
        preset.figureBounciness = 0.65f;
        preset.wallBounciness = 0.92f;
        preset.surfaceFriction = 0f;
        preset.lightShotForce = 45f;
        preset.heavyShotForce = 85f;
        preset.maxVerticalAngle = 55f;
        preset.shotActiveWindow = 0.3f;
        preset.shotCooldown = 0.25f;
        preset.maxBallSpeed = 0f;
        preset.cameraShakeMultiplier = 1.2f;
        preset.bumpStrength = 4f;
        preset.maxBumpRange = 4f;
        preset.magnetPullForce = 8f;
        preset.magnetMaxBallSpeed = 15f;
        SavePreset(preset, "Speed Demon");
    }

    private static void CreateCompetitivePreset()
    {
        var preset = ScriptableObject.CreateInstance<PhysicsPreset>();
        preset.presetName = "Competitive";
        preset.description = "Balanced and fair. Moderate friction gives ball control, predictable bounces. Rewards skill.";
        preset.ballMass = 1f;
        preset.ballLinearDrag = 0.8f;
        preset.ballBounciness = 0.75f;
        preset.figureBounciness = 0.5f;
        preset.wallBounciness = 0.8f;
        preset.surfaceFriction = 0.1f;
        preset.lightShotForce = 25f;
        preset.heavyShotForce = 55f;
        preset.maxVerticalAngle = 35f;
        preset.shotActiveWindow = 0.3f;
        preset.shotCooldown = 0.5f;
        preset.maxBallSpeed = 70f;
        preset.cameraShakeMultiplier = 0.8f;
        preset.bumpStrength = 3f;
        preset.maxBumpRange = 3f;
        preset.magnetPullForce = 15f;
        preset.magnetMaxBallSpeed = 8f;
        SavePreset(preset, "Competitive");
    }

    private static void SavePreset(PhysicsPreset preset, string name)
    {
        string path = $"{PRESETS_FOLDER}/{name}.asset";

        // Don't overwrite existing presets
        if (AssetDatabase.LoadAssetAtPath<PhysicsPreset>(path) != null)
        {
            Debug.Log($"[PhysicsPresetFactory] '{name}' already exists, skipping.");
            Object.DestroyImmediate(preset);
            return;
        }

        AssetDatabase.CreateAsset(preset, path);
        Debug.Log($"[PhysicsPresetFactory] Created preset: {path}");
    }
}

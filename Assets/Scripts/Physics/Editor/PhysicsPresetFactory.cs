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
        CreateRealisticPreset();
        CreateArcadePreset();
        CreatePinballChaosPreset();
        CreateAirHockeyPreset();
        CreateHeavyMetalPreset();
        CreateSpeedDemonPreset();
        CreateCompetitivePreset();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[PhysicsPresetFactory] Created 8 presets in 'Assets/Physics Presets/'");
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
        preset.mediumShotForce = 60f;
        preset.heavyShotForce = 100f;
        preset.maxVerticalAngle = 45f;
        preset.shotActiveWindow = 0.3f;
        preset.shotCooldown = 0.5f;
        preset.maxBallSpeed = 0f;
        preset.cameraShakeMultiplier = 1f;
        SavePreset(preset, "Current Default");
    }

    private static void CreateRealisticPreset()
    {
        var preset = ScriptableObject.CreateInstance<PhysicsPreset>();
        preset.presetName = "Realistic Foosball";
        preset.description = "Simulates a real foosball table. Higher friction, realistic bounce, heavier ball with natural deceleration.";
        preset.ballMass = 1.2f;
        preset.ballLinearDrag = 1.5f;
        preset.ballBounciness = 0.6f;
        preset.figureBounciness = 0.4f;
        preset.wallBounciness = 0.7f;
        preset.surfaceFriction = 0.25f;
        preset.lightShotForce = 20f;
        preset.mediumShotForce = 40f;
        preset.heavyShotForce = 70f;
        preset.maxVerticalAngle = 30f;
        preset.shotActiveWindow = 0.25f;
        preset.shotCooldown = 0.6f;
        preset.maxBallSpeed = 60f;
        preset.cameraShakeMultiplier = 0.5f;
        SavePreset(preset, "Realistic Foosball");
    }

    private static void CreateArcadePreset()
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
        preset.mediumShotForce = 70f;
        preset.heavyShotForce = 120f;
        preset.maxVerticalAngle = 50f;
        preset.shotActiveWindow = 0.35f;
        preset.shotCooldown = 0.4f;
        preset.maxBallSpeed = 0f;
        preset.cameraShakeMultiplier = 1.5f;
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
        preset.mediumShotForce = 50f;
        preset.heavyShotForce = 85f;
        preset.maxVerticalAngle = 60f;
        preset.shotActiveWindow = 0.4f;
        preset.shotCooldown = 0.3f;
        preset.maxBallSpeed = 80f;
        preset.cameraShakeMultiplier = 1.8f;
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
        preset.mediumShotForce = 38f;
        preset.heavyShotForce = 65f;
        preset.maxVerticalAngle = 40f;
        preset.shotActiveWindow = 0.3f;
        preset.shotCooldown = 0.45f;
        preset.maxBallSpeed = 55f;
        preset.cameraShakeMultiplier = 0.7f;
        SavePreset(preset, "Air Hockey");
    }

    private static void CreateHeavyMetalPreset()
    {
        var preset = ScriptableObject.CreateInstance<PhysicsPreset>();
        preset.presetName = "Heavy Metal";
        preset.description = "Heavy ball, massive impact. Slow but satisfying — every shot feels like a cannonball.";
        preset.ballMass = 2f;
        preset.ballLinearDrag = 2f;
        preset.ballBounciness = 0.35f;
        preset.figureBounciness = 0.25f;
        preset.wallBounciness = 0.4f;
        preset.surfaceFriction = 0.35f;
        preset.lightShotForce = 40f;
        preset.mediumShotForce = 80f;
        preset.heavyShotForce = 150f;
        preset.maxVerticalAngle = 25f;
        preset.shotActiveWindow = 0.35f;
        preset.shotCooldown = 0.7f;
        preset.maxBallSpeed = 50f;
        preset.cameraShakeMultiplier = 2f;
        SavePreset(preset, "Heavy Metal");
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
        preset.mediumShotForce = 85f;
        preset.heavyShotForce = 140f;
        preset.maxVerticalAngle = 55f;
        preset.shotActiveWindow = 0.3f;
        preset.shotCooldown = 0.25f;
        preset.maxBallSpeed = 0f;
        preset.cameraShakeMultiplier = 1.2f;
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
        preset.mediumShotForce = 55f;
        preset.heavyShotForce = 95f;
        preset.maxVerticalAngle = 35f;
        preset.shotActiveWindow = 0.3f;
        preset.shotCooldown = 0.5f;
        preset.maxBallSpeed = 70f;
        preset.cameraShakeMultiplier = 0.8f;
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

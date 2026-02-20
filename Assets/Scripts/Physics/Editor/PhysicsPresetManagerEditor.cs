using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PhysicsPresetManager))]
public class PhysicsPresetManagerEditor : Editor
{
    private SerializedProperty activePreset;
    private SerializedProperty ballMaterial;
    private SerializedProperty figureMaterial;
    private SerializedProperty wallMaterial;
    private SerializedProperty applyOnStart;

    private PhysicsPreset[] availablePresets;

    private void OnEnable()
    {
        activePreset = serializedObject.FindProperty("activePreset");
        ballMaterial = serializedObject.FindProperty("ballMaterial");
        figureMaterial = serializedObject.FindProperty("figureMaterial");
        wallMaterial = serializedObject.FindProperty("wallMaterial");
        applyOnStart = serializedObject.FindProperty("applyOnStart");

        RefreshPresetList();
    }

    private void RefreshPresetList()
    {
        string[] guids = AssetDatabase.FindAssets("t:PhysicsPreset");
        availablePresets = new PhysicsPreset[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            availablePresets[i] = AssetDatabase.LoadAssetAtPath<PhysicsPreset>(path);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var manager = (PhysicsPresetManager)target;

        // Header
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("⚽ Physics Preset Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // Quick preset buttons
        EditorGUILayout.LabelField("Quick Apply Presets", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Click a button to assign and apply a preset instantly.", MessageType.Info);

        if (availablePresets != null && availablePresets.Length > 0)
        {
            EditorGUILayout.BeginVertical("box");
            foreach (var preset in availablePresets)
            {
                if (preset == null) continue;

                bool isActive = manager.activePreset == preset;
                GUI.backgroundColor = isActive ? Color.green : Color.white;

                EditorGUILayout.BeginHorizontal();
                string label = isActive ? $"✅ {preset.presetName}" : $"▶ {preset.presetName}";
                if (GUILayout.Button(label, GUILayout.Height(30)))
                {
                    Undo.RecordObject(manager, "Change Physics Preset");
                    manager.activePreset = preset;
                    EditorUtility.SetDirty(manager);

                    if (Application.isPlaying)
                    {
                        manager.ApplyPreset();
                    }
                    else
                    {
                        ApplyPresetInEditor(manager);
                    }
                }
                EditorGUILayout.EndHorizontal();

                // Show description if available
                if (!string.IsNullOrEmpty(preset.description))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField(preset.description, EditorStyles.miniLabel);
                    EditorGUI.indentLevel--;
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox(
                "No PhysicsPreset assets found. Right-click in Project → Create → Futbolito → Physics Preset",
                MessageType.Warning);
        }

        if (GUILayout.Button("🔄 Refresh Preset List"))
        {
            RefreshPresetList();
        }

        EditorGUILayout.Space(10);

        // Standard properties
        EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(activePreset);
        EditorGUILayout.PropertyField(applyOnStart);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Physics Materials", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(ballMaterial);
        EditorGUILayout.PropertyField(figureMaterial);
        EditorGUILayout.PropertyField(wallMaterial);

        EditorGUILayout.Space(10);

        // Manual apply button
        GUI.backgroundColor = new Color(0.3f, 0.8f, 1f);
        if (GUILayout.Button("🎯 Apply Active Preset Now", GUILayout.Height(35)))
        {
            if (Application.isPlaying)
            {
                manager.ApplyPreset();
            }
            else
            {
                ApplyPresetInEditor(manager);
            }
        }
        GUI.backgroundColor = Color.white;

        // Show current preset summary
        if (manager.activePreset != null)
        {
            EditorGUILayout.Space(10);
            DrawPresetSummary(manager.activePreset);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void ApplyPresetInEditor(PhysicsPresetManager manager)
    {
        if (manager.activePreset == null) return;

        if (manager.ballMaterial != null)
        {
            Undo.RecordObject(manager.ballMaterial, "Apply Physics Preset");
            manager.ballMaterial.friction = manager.activePreset.surfaceFriction;
            manager.ballMaterial.bounciness = manager.activePreset.ballBounciness;
            EditorUtility.SetDirty(manager.ballMaterial);
        }

        if (manager.figureMaterial != null)
        {
            Undo.RecordObject(manager.figureMaterial, "Apply Physics Preset");
            manager.figureMaterial.friction = manager.activePreset.surfaceFriction;
            manager.figureMaterial.bounciness = manager.activePreset.figureBounciness;
            EditorUtility.SetDirty(manager.figureMaterial);
        }

        if (manager.wallMaterial != null)
        {
            Undo.RecordObject(manager.wallMaterial, "Apply Physics Preset");
            manager.wallMaterial.friction = manager.activePreset.surfaceFriction;
            manager.wallMaterial.bounciness = manager.activePreset.wallBounciness;
            EditorUtility.SetDirty(manager.wallMaterial);
        }

        Debug.Log($"[PhysicsPresetManager] Editor: Applied '{manager.activePreset.presetName}' to physics materials.");
    }

    private void DrawPresetSummary(PhysicsPreset preset)
    {
        EditorGUILayout.LabelField("Active Preset Summary", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField($"Ball: mass={preset.ballMass} drag={preset.ballLinearDrag} bounce={preset.ballBounciness}");
        EditorGUILayout.LabelField($"Surfaces: friction={preset.surfaceFriction} fig_bounce={preset.figureBounciness} wall_bounce={preset.wallBounciness}");
        EditorGUILayout.LabelField($"Shots: light={preset.lightShotForce} med={preset.mediumShotForce} heavy={preset.heavyShotForce}");
        EditorGUILayout.LabelField($"Feel: window={preset.shotActiveWindow}s cooldown={preset.shotCooldown}s shake×{preset.cameraShakeMultiplier}");

        EditorGUILayout.EndVertical();
    }
}

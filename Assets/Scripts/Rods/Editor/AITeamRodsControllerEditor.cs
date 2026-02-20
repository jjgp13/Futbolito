using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom Inspector for AITeamRodsController
/// Adds preset buttons for easy difficulty testing
/// </summary>
[CustomEditor(typeof(AITeamRodsController))]
public class AITeamRodsControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();

        AITeamRodsController controller = (AITeamRodsController)target;

        // Add space before preset buttons
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Quick Difficulty Presets", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
    "Click a button to instantly load preset difficulty configuration. " +
            "Changes apply immediately in Play mode.",
 MessageType.Info);

        EditorGUILayout.Space(5);

        // Horizontal layout for preset buttons
        EditorGUILayout.BeginHorizontal();

        // Easy preset button
        if (GUILayout.Button("Load EASY Preset", GUILayout.Height(30)))
        {
            Undo.RecordObject(controller, "Load Easy Preset");
            controller.ApplyDifficultyPreset(1);
            EditorUtility.SetDirty(controller);
            Debug.Log("[Editor] Applied EASY difficulty preset");
        }

        // Medium preset button
        if (GUILayout.Button("Load MEDIUM Preset", GUILayout.Height(30)))
        {
            Undo.RecordObject(controller, "Load Medium Preset");
            controller.ApplyDifficultyPreset(2);
            EditorUtility.SetDirty(controller);
            Debug.Log("[Editor] Applied MEDIUM difficulty preset");
        }

        // Hard preset button
        if (GUILayout.Button("Load HARD Preset", GUILayout.Height(30)))
        {
            Undo.RecordObject(controller, "Load Hard Preset");
            controller.ApplyDifficultyPreset(3);
            EditorUtility.SetDirty(controller);
            Debug.Log("[Editor] Applied HARD difficulty preset");
        }

        EditorGUILayout.EndHorizontal();

        // Show current configuration summary
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Current Configuration Summary", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        SerializedObject serializedController = new SerializedObject(controller);

        // Action Probabilities
        EditorGUILayout.LabelField("Action Probabilities:", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField($"  Shoot: {serializedController.FindProperty("shootProbability").floatValue:P0}");
        EditorGUILayout.LabelField($"  Wall Pass: {serializedController.FindProperty("wallPassProbability").floatValue:P0}");
        EditorGUILayout.LabelField($"  Magnet: {serializedController.FindProperty("magnetProbability").floatValue:P0}");

        EditorGUILayout.Space(3);

        // Timing
        EditorGUILayout.LabelField("Reaction & Timing:", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField($"  Reaction Delay: {serializedController.FindProperty("reactionDelay").floatValue:F2}s");
        EditorGUILayout.LabelField($"  Charge Multiplier: {serializedController.FindProperty("chargeTimeMultiplier").floatValue:F2}x");
        EditorGUILayout.LabelField($"  Decision Interval: {serializedController.FindProperty("decisionInterval").floatValue:F2}s");

        EditorGUILayout.Space(3);

        // Defensive Strategy
        EditorGUILayout.LabelField("Defensive Strategy:", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField($"  Zone Defense: {serializedController.FindProperty("zoneDefenseWeight").floatValue:P0}");
        EditorGUILayout.LabelField($"  Man Marking: {serializedController.FindProperty("manMarkingWeight").floatValue:P0}");
        EditorGUILayout.LabelField($"  Anticipation: {serializedController.FindProperty("anticipationWeight").floatValue:P0}");

        EditorGUILayout.Space(3);

        // Behavior Quality
        EditorGUILayout.LabelField("Behavior Quality:", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField($"  Whiff Chance: {serializedController.FindProperty("whiffChance").floatValue:P0}");
        EditorGUILayout.LabelField($"  Positioning Accuracy: {serializedController.FindProperty("positioningAccuracy").floatValue:P0}");
        EditorGUILayout.LabelField($"  Movement Speed: {serializedController.FindProperty("movementSpeedMultiplier").floatValue:P0}");

        EditorGUILayout.Space(3);

        // Shot Aggressiveness
        EditorGUILayout.LabelField("Shot Aggressiveness:", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField($"  Charge Target: {serializedController.FindProperty("shotChargeTarget").floatValue:F2} ({serializedController.FindProperty("shotChargeTarget").floatValue * 1.0f:F2}s)");
        EditorGUILayout.LabelField($"  Cooldown Duration: {serializedController.FindProperty("chargeCooldownDuration").floatValue:F2}s");

        EditorGUILayout.Space(3);

        // Magnet Behavior
        EditorGUILayout.LabelField("Magnet Behavior:", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField($"  Min Hold Time: {serializedController.FindProperty("minimumMagnetHoldTime").floatValue:F2}s");
        EditorGUILayout.LabelField($"  Max Ball Velocity: {serializedController.FindProperty("maxBallVelocityForMagnet").floatValue:F1}");

        EditorGUILayout.EndVertical();

        // Apply changes during Play mode
        if (Application.isPlaying && GUI.changed)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                  "? Changes made during Play mode will be lost when you stop. " +
          "Use preset buttons for permanent changes.",
                 MessageType.Warning);
        }

        // Add testing tips
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Testing Tips", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
   "� Load a preset and enter Play mode to test\n" +
          "� Adjust individual values for fine-tuning\n" +
  "� Check Console for debug logs\n" +
            "� Enable 'Show Debug Info' on FSM components for detailed logging",
            MessageType.Info);
    }
}

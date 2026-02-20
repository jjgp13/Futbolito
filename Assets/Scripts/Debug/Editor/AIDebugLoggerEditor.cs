using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AIDebugLogger))]
public class AIDebugLoggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AIDebugLogger logger = (AIDebugLogger)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Logger Controls", EditorStyles.boldLabel);

        if (Application.isPlaying)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Start Logging", GUILayout.Height(30)))
            {
                logger.StartLogging();
            }

            if (GUILayout.Button("Stop Logging", GUILayout.Height(30)))
            {
                logger.StopLogging();
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Run Analysis Now", GUILayout.Height(25)))
            {
                logger.RunAnalysis();
            }
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Enter Play mode to control logging.\n" +
                "With 'Auto Log On Match' enabled, logging starts/stops automatically with each match.",
                MessageType.Info);
        }

        EditorGUILayout.Space(5);
        if (GUILayout.Button("Open Log Directory", GUILayout.Height(25)))
        {
            logger.OpenLogDirectory();
        }
    }
}

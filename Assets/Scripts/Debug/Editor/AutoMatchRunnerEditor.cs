using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom Inspector for AutoMatchRunner — provides Start/Stop buttons, profile info, and live progress.
/// </summary>
[CustomEditor(typeof(AutoMatchRunner))]
public class AutoMatchRunnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var runner = (AutoMatchRunner)target;

        // Draw default inspector fields
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        // Profile info box
        DrawProfileInfo(runner);

        EditorGUILayout.Space(10);

        // Status section
        EditorGUILayout.LabelField("Test Suite Status", EditorStyles.boldLabel);

        if (runner.IsRunning)
        {
            // Progress bar
            float progress = (float)runner.CurrentMatch / runner.TotalMatches;
            EditorGUI.ProgressBar(
                EditorGUILayout.GetControlRect(false, 20),
                progress,
                $"Match {runner.CurrentMatch} / {runner.TotalMatches}"
            );

            // Time estimate
            float estMinutes = (runner.TotalMatches * runner.MatchTimeMinutes * 60f / runner.TimeScale) / 60f;
            float elapsed = Time.realtimeSinceStartup;
            EditorGUILayout.LabelField($"  Estimated total: ~{estMinutes:F0} min");

            EditorGUILayout.Space(5);

            // Stop button
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("■ Stop Test Suite", GUILayout.Height(30)))
            {
                runner.StopTestSuite();
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            if (Application.isPlaying)
            {
                // Start button
                GUI.backgroundColor = new Color(0.4f, 1f, 0.4f);
                if (GUILayout.Button("▶ Run AI Test Suite", GUILayout.Height(35)))
                {
                    runner.StartTestSuite();
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Enter Play Mode to run the AI test suite.\n" +
                    "If 'Auto Start On Play' is enabled, tests begin automatically.",
                    MessageType.Info
                );
            }
        }

        // Results section
        if (runner.Results != null && runner.Results.Count > 0)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);

            int leftWins = 0, rightWins = 0, draws = 0;
            int totalLeftGoals = 0, totalRightGoals = 0;

            foreach (var r in runner.Results)
            {
                if (r.winner == "Left") leftWins++;
                else if (r.winner == "Right") rightWins++;
                else draws++;
                totalLeftGoals += r.leftScore;
                totalRightGoals += r.rightScore;
            }

            EditorGUILayout.LabelField($"  Record: Left {leftWins}W — {draws}D — {rightWins}W Right");
            EditorGUILayout.LabelField($"  Goals:  Left {totalLeftGoals} — {totalRightGoals} Right");

            // Per-match results
            EditorGUILayout.Space(5);
            foreach (var r in runner.Results)
            {
                string ko = r.knockout ? " KO" : "";
                string diff = $"{DiffName(r.leftDifficulty)}v{DiffName(r.rightDifficulty)}";
                string preset = (r.physicsPreset ?? "").Length > 10
                    ? (r.physicsPreset ?? "").Substring(0, 10)
                    : r.physicsPreset ?? "";
                EditorGUILayout.LabelField($"  #{r.matchNumber}: L{r.leftScore}-R{r.rightScore} {r.winner}{ko} [{diff}] {preset} rsp:{r.ballRestarts}");
            }

            // Open report button
            if (!string.IsNullOrEmpty(runner.ReportOutputPath) && System.IO.File.Exists(runner.ReportOutputPath))
            {
                EditorGUILayout.Space(5);
                if (GUILayout.Button("Open Aggregate Report"))
                {
                    EditorUtility.RevealInFinder(runner.ReportOutputPath);
                }
            }
        }

        // Force repaint while running
        if (runner.IsRunning)
        {
            Repaint();
        }
    }

    private void DrawProfileInfo(AutoMatchRunner runner)
    {
        var profile = runner.ActiveProfile;
        int combos = runner.UniqueCombos;
        int matches = AutoMatchRunner.MatchCountForProfile(profile, combos);

        float estMinutes = (matches * runner.MatchTimeMinutes * 60f / runner.TimeScale) / 60f;

        string runsPerCombo = profile switch
        {
            AutoMatchRunner.TestProfile.Quick => "1 run per combo — fast iteration after code changes",
            AutoMatchRunner.TestProfile.Standard => "2 runs per combo — spot consistency and patterns",
            AutoMatchRunner.TestProfile.Full => "3 runs per combo — statistical confidence for releases",
            _ => ""
        };

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Profile Summary", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"  {runsPerCombo}");
        EditorGUILayout.LabelField($"  {combos} unique combos → {matches} matches → ~{estMinutes:F0} min real time");
        EditorGUILayout.EndVertical();
    }

    private static string DiffName(int level) => level switch
    {
        1 => "Easy",
        2 => "Med",
        3 => "Hard",
        _ => $"L{level}"
    };
}

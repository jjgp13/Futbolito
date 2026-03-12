using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Automated AI vs AI match runner for testing.
/// Runs multiple matches in sequence at accelerated speed with varying
/// difficulty combos and physics presets. Generates aggregate reports.
/// 
/// USAGE:
/// 1. Wire a UI Button to GameControlsConfigPanel.StartAutoTest()
/// 2. Or add to scene and enable autoStartOnPlay
/// 3. Matches cycle through difficulty combos × physics presets automatically
/// 4. Reports saved to Application.persistentDataPath/ai_logs/auto_test_*/
/// </summary>
[DefaultExecutionOrder(-100)]
public class AutoMatchRunner : MonoBehaviour
{
    #region Test Profiles

    public enum TestProfile
    {
        Quick,     // 1 run per unique combo
        Standard,  // 2 runs per unique combo
        Full       // 3 runs per unique combo
    }

    /// <summary>Number of unique combos = difficulty rotations × physics presets × formation presets (or 1 if disabled).</summary>
    public int UniqueCombos
    {
        get
        {
            int diffCombos = rotateDifficulty ? DifficultyRotation.GetLength(0) : 1;
            int presetCombos = (rotatePhysicsPresets && physicsPresets.Count > 0) ? physicsPresets.Count : 1;
            int formationCombos = (rotateFormations && formationPresets.Count > 0) ? formationPresets.Count : 1;
            return diffCombos * presetCombos * formationCombos;
        }
    }

    /// <summary>Computes total match count from profile and unique combos.</summary>
    public static int MatchCountForProfile(TestProfile profile, int uniqueCombos)
    {
        return profile switch
        {
            TestProfile.Quick => uniqueCombos * 1,
            TestProfile.Standard => uniqueCombos * 2,
            TestProfile.Full => uniqueCombos * 3,
            _ => uniqueCombos
        };
    }

    #endregion

    #region Configuration

    [Header("Test Profile")]
    [Tooltip("Quick = 1 run per combo, Standard = 2 runs per combo, Full = 3 runs per combo")]
    [SerializeField] private TestProfile testProfile = TestProfile.Quick;

    [Header("Match Settings")]
    [Tooltip("Match duration in minutes (game time)")]
    [SerializeField, Range(2, 5)] private int matchTimeMinutes = 3;

    [Tooltip("Game speed multiplier (5x = 3min match in ~36s real time)")]
    [SerializeField, Range(1f, 10f)] private float timeScale = 5f;

    [Header("Difficulty Rotation")]
    [Tooltip("Cycle through different left/right difficulty combos each match")]
    [SerializeField] private bool rotateDifficulty = true;

    [Header("Physics Preset Rotation")]
    [Tooltip("Cycle through different physics presets each match")]
    [SerializeField] private bool rotatePhysicsPresets = true;

    [Tooltip("Physics preset assets to cycle through (auto-loaded if empty)")]
    [SerializeField] private List<PhysicsPreset> physicsPresets = new List<PhysicsPreset>();

    [Header("Formation Preset Rotation")]
    [Tooltip("Cycle through different formation presets each match")]
    [SerializeField] private bool rotateFormations = true;

    [Tooltip("Formation preset assets to cycle through (auto-loaded if empty)")]
    [SerializeField] private List<FormationPreset> formationPresets = new List<FormationPreset>();

    [Header("Scene")]
    [Tooltip("Name of the match scene to load")]
    [SerializeField] private string matchSceneName = "GameMatchTesting_Scene";

    [Header("Automation")]
    [Tooltip("Automatically start the test suite when Play is pressed")]
    [SerializeField] private bool autoStartOnPlay = false;

    #endregion

    #region Singleton

    public static AutoMatchRunner Instance { get; private set; }

    public static bool IsAutoMode => Instance != null && Instance.isRunning;

    #endregion

    #region State

    private bool isRunning;
    private int currentMatch;
    private int matchCount; // Computed from profile × unique combos
    private string testRunId;
    private float testStartRealTime;
    private float matchStartRealTime;
    private List<MatchResult> results = new List<MatchResult>();
    private string reportOutputPath;

    // Current match configuration
    private int currentLeftDifficulty;
    private int currentRightDifficulty;
    private string currentPhysicsPresetName;
    private string currentFormationPresetName;

    // Difficulty combos to cycle through
    private static readonly int[,] DifficultyRotation = {
        { 1, 1 }, // Easy vs Easy
        { 1, 2 }, // Easy vs Medium
        { 1, 3 }, // Easy vs Hard
        { 2, 2 }, // Medium vs Medium
        { 2, 3 }, // Medium vs Hard
        { 3, 3 }, // Hard vs Hard
    };

    // Public read-only accessors for Editor UI
    public bool IsRunning => isRunning;
    public int CurrentMatch => currentMatch;
    public int TotalMatches => matchCount;
    public IReadOnlyList<MatchResult> Results => results;
    public string TestRunId => testRunId;
    public string ReportOutputPath => reportOutputPath;
    public TestProfile ActiveProfile => testProfile;
    public float TimeScale => timeScale;
    public int MatchTimeMinutes => matchTimeMinutes;

    #endregion

    #region Data Structures

    [Serializable]
    public struct MatchResult
    {
        public int matchNumber;
        public int leftScore;
        public int rightScore;
        public int leftDifficulty;
        public int rightDifficulty;
        public string physicsPreset;
        public string formationPreset;
        public float gameDurationSeconds;
        public float realDurationSeconds;
        public bool knockout;
        public string winner;
        public int ballRestarts;
        public float deadBallTime;
        public string logFilePath;
    }

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (autoStartOnPlay && !isRunning)
        {
            InitTestSuite();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeFromMatchEvents();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-subscribe after scene load since static events are cleared in OnDestroy
        SubscribeToMatchEvents();
        
        // Restore timeScale and fixedDeltaTime (other scripts may have reset them)
        if (isRunning)
        {
            Time.timeScale = timeScale;
            Time.fixedDeltaTime = 0.02f * timeScale;
        }
    }

    private void SubscribeToMatchEvents()
    {
        // Unsubscribe first to prevent duplicates
        MatchController.OnMatchEnd -= OnMatchEnded;
        MatchController.OnMatchStart -= OnMatchStarted;
        MatchController.OnMatchEnd += OnMatchEnded;
        MatchController.OnMatchStart += OnMatchStarted;
    }

    private void UnsubscribeFromMatchEvents()
    {
        MatchController.OnMatchEnd -= OnMatchEnded;
        MatchController.OnMatchStart -= OnMatchStarted;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            FormationPreset.Active = null;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
    }

    #endregion

    #region Test Suite Control

    private void InitTestSuite()
    {
        SyncMatchCountFromProfile();
        isRunning = true;
        currentMatch = 1;
        results.Clear();
        testRunId = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        testStartRealTime = Time.realtimeSinceStartup;
        reportOutputPath = null;

        LoadPhysicsPresetsIfNeeded();
        LogSuiteStart();

        EnsureMatchInfo();
        Time.timeScale = timeScale;
        // Scale physics step with timeScale to prevent spiral of death.
        // Default fixedDeltaTime is 0.02 → at 5x, use 0.1 (same real-world rate).
        Time.fixedDeltaTime = 0.02f * timeScale;

        // Apply configuration to the first match so difficulty/formation/preset are set
        ApplyMatchConfiguration(currentMatch);
    }

    public void StartTestSuite()
    {
        if (isRunning) return;

        // Clean previous logs for a fresh run
        AIDebugLogger.CleanLogDirectory();

        SyncMatchCountFromProfile();
        isRunning = true;
        currentMatch = 0;
        results.Clear();
        testRunId = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        testStartRealTime = Time.realtimeSinceStartup;
        reportOutputPath = null;

        LoadPhysicsPresetsIfNeeded();
        LogSuiteStart();

        EnsureMatchInfo();
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = 0.02f * timeScale;

        LoadNextMatch();
    }

    public void StartTestSuiteInCurrentScene()
    {
        if (isRunning) return;

        // Clean previous logs for a fresh run
        AIDebugLogger.CleanLogDirectory();

        SyncMatchCountFromProfile();
        isRunning = true;
        currentMatch = 1;
        results.Clear();
        testRunId = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        testStartRealTime = Time.realtimeSinceStartup;
        reportOutputPath = null;

        LoadPhysicsPresetsIfNeeded();
        LogSuiteStart();

        EnsureMatchInfo();
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = 0.02f * timeScale;
        ApplyMatchConfiguration(currentMatch);
    }

    public void StopTestSuite()
    {
        if (!isRunning) return;

        isRunning = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        FormationPreset.Active = null;
        {
            GenerateReport();
        }

        Debug.Log("[AutoMatchRunner] Test suite stopped.");
    }

    private void SyncMatchCountFromProfile()
    {
        matchCount = MatchCountForProfile(testProfile, UniqueCombos);
    }

    private void LogSuiteStart()
    {
        Debug.Log($"[AutoMatchRunner] === STARTING TEST SUITE ===");
        Debug.Log($"[AutoMatchRunner] Profile: {testProfile} | Matches: {matchCount} ({UniqueCombos} unique combos) | Time: {matchTimeMinutes}min | Speed: {timeScale}x");
        Debug.Log($"[AutoMatchRunner] Difficulty rotation: {(rotateDifficulty ? "ON" : "OFF")} | Physics rotation: {(rotatePhysicsPresets ? "ON" : "OFF")} ({physicsPresets.Count} presets) | Formation rotation: {(rotateFormations ? "ON" : "OFF")} ({formationPresets.Count} presets)");
        float estMinutes = (matchCount * matchTimeMinutes * 60f / timeScale) / 60f;
        Debug.Log($"[AutoMatchRunner] Estimated real time: ~{estMinutes:F0} minutes");
    }

    #endregion

    #region Match Configuration

    private void LoadPhysicsPresetsIfNeeded()
    {
        if (rotatePhysicsPresets && physicsPresets.Count == 0)
        {
            // Auto-load all physics presets from Resources or known path
            var loaded = Resources.LoadAll<PhysicsPreset>("");
            if (loaded.Length > 0)
            {
                physicsPresets.AddRange(loaded);
            }
            else
            {
                Debug.LogWarning("[AutoMatchRunner] No physics presets found via Resources.LoadAll. Assign them manually in Inspector.");
                rotatePhysicsPresets = false;
            }
        }

        if (rotateFormations && formationPresets.Count == 0)
        {
            var loaded = Resources.LoadAll<FormationPreset>("");
            if (loaded.Length > 0)
            {
                formationPresets.AddRange(loaded);
            }
            else
            {
                Debug.LogWarning("[AutoMatchRunner] No formation presets found via Resources.LoadAll. Assign them manually in Inspector.");
                rotateFormations = false;
            }
        }
    }

    /// <summary>
    /// Selects difficulty combo and physics preset for a given match number.
    /// </summary>
    private void ApplyMatchConfiguration(int matchNum)
    {
        // Difficulty rotation
        if (rotateDifficulty)
        {
            int comboIndex = (matchNum - 1) % (DifficultyRotation.GetLength(0));
            currentLeftDifficulty = DifficultyRotation[comboIndex, 0];
            currentRightDifficulty = DifficultyRotation[comboIndex, 1];
        }
        else
        {
            currentLeftDifficulty = 2;
            currentRightDifficulty = 2;
        }

        // Physics preset rotation
        if (rotatePhysicsPresets && physicsPresets.Count > 0)
        {
            int presetIndex = (matchNum - 1) % physicsPresets.Count;
            currentPhysicsPresetName = physicsPresets[presetIndex] != null
                ? physicsPresets[presetIndex].presetName
                : "Unknown";
        }
        else
        {
            currentPhysicsPresetName = "Default";
        }

        // Formation preset rotation
        if (rotateFormations && formationPresets.Count > 0)
        {
            int formationIndex = (matchNum - 1) % formationPresets.Count;
            var formation = formationPresets[formationIndex];
            currentFormationPresetName = formation != null ? formation.presetName : "Unknown";
            FormationPreset.Active = formation;
        }
        else
        {
            currentFormationPresetName = "Default";
            FormationPreset.Active = null;
        }

        string leftDiffName = DifficultyName(currentLeftDifficulty);
        string rightDiffName = DifficultyName(currentRightDifficulty);
        Debug.Log($"[AutoMatchRunner] Match {matchNum} config: {leftDiffName} vs {rightDiffName} | Physics: {currentPhysicsPresetName} | Formation: {currentFormationPresetName}");
    }

    /// <summary>
    /// Called after scene loads to apply per-team difficulty and physics preset.
    /// </summary>
    private void ApplyMatchConfigToScene()
    {
        // Apply per-team difficulty
        var aiControllers = FindObjectsByType<AITeamRodsController>(FindObjectsSortMode.None);
        foreach (var controller in aiControllers)
        {
            if (controller.teamSide == TeamSide.LeftTeam)
            {
                controller.ApplyDifficultyPreset(currentLeftDifficulty);
                Debug.Log($"[AutoMatchRunner] Left team → {DifficultyName(currentLeftDifficulty)}");
            }
            else if (controller.teamSide == TeamSide.RightTeam)
            {
                controller.ApplyDifficultyPreset(currentRightDifficulty);
                Debug.Log($"[AutoMatchRunner] Right team → {DifficultyName(currentRightDifficulty)}");
            }
        }

        // Apply physics preset
        if (rotatePhysicsPresets && physicsPresets.Count > 0)
        {
            int presetIndex = (currentMatch - 1) % physicsPresets.Count;
            var presetManager = FindAnyObjectByType<PhysicsPresetManager>();
            if (presetManager != null && physicsPresets[presetIndex] != null)
            {
                presetManager.activePreset = physicsPresets[presetIndex];
                presetManager.ApplyPreset();
                Debug.Log($"[AutoMatchRunner] Physics preset → {physicsPresets[presetIndex].presetName}");
            }
        }
    }

    private static string DifficultyName(int level) => level switch
    {
        1 => "Easy",
        2 => "Medium",
        3 => "Hard",
        _ => $"L{level}"
    };

    #endregion

    #region Match Lifecycle

    private void EnsureMatchInfo()
    {
        if (MatchInfo.instance == null)
        {
            var go = new GameObject("MatchInfo_AutoTest");
            go.tag = "MatchData";
            go.AddComponent<MatchInfo>();
        }

        MatchInfo.instance.leftControllers.Clear();
        MatchInfo.instance.rightControllers.Clear();
        MatchInfo.instance.matchTime = matchTimeMinutes;
        MatchInfo.instance.matchLevel = 2; // Default, overridden per-team
        MatchInfo.instance.matchType = MatchType.QuickMatch;
    }

    private void LoadNextMatch()
    {
        currentMatch++;
        ApplyMatchConfiguration(currentMatch);
        Debug.Log($"[AutoMatchRunner] --- Loading match {currentMatch}/{matchCount} ---");
        SceneManager.LoadScene(matchSceneName);
    }

    private void OnMatchStarted()
    {
        if (!isRunning) return;
        matchStartRealTime = Time.realtimeSinceStartup;

        // Apply per-team difficulty and physics preset to the loaded scene
        ApplyMatchConfigToScene();

        Debug.Log($"[AutoMatchRunner] Match {currentMatch}/{matchCount} started");
    }

    private void OnMatchEnded()
    {
        if (!isRunning) return;

        // Read ball stats from BallBehavior
        int ballRestarts = BallBehavior.BallSpawnCount;
        float deadBallTime = BallBehavior.TotalDeadBallTime;

        var result = new MatchResult
        {
            matchNumber = currentMatch,
            leftScore = MatchScoreController.instance != null ? MatchScoreController.instance.LeftTeamScore : 0,
            rightScore = MatchScoreController.instance != null ? MatchScoreController.instance.RightTeamScore : 0,
            leftDifficulty = currentLeftDifficulty,
            rightDifficulty = currentRightDifficulty,
            physicsPreset = currentPhysicsPresetName,
            formationPreset = currentFormationPresetName,
            realDurationSeconds = Time.realtimeSinceStartup - matchStartRealTime,
            gameDurationSeconds = matchTimeMinutes * 60f,
            ballRestarts = ballRestarts,
            deadBallTime = deadBallTime
        };

        result.knockout = (result.leftScore >= 5 || result.rightScore >= 5);

        if (result.leftScore > result.rightScore) result.winner = "Left";
        else if (result.rightScore > result.leftScore) result.winner = "Right";
        else result.winner = "Draw";

        if (AIDebugLogger.Instance != null)
        {
            result.logFilePath = AIDebugLogger.Instance.CurrentLogFilePath;
        }

        results.Add(result);

        string leftDiff = DifficultyName(result.leftDifficulty);
        string rightDiff = DifficultyName(result.rightDifficulty);
        Debug.Log($"[AutoMatchRunner] Match {currentMatch}: L{result.leftScore}-R{result.rightScore} ({result.winner}) [{leftDiff} vs {rightDiff}] [{result.physicsPreset}] [{result.formationPreset}] restarts:{result.ballRestarts} deadBall:{result.deadBallTime:F0}s [{result.realDurationSeconds:F1}s]");

        if (currentMatch < matchCount)
        {
            StartCoroutine(LoadNextMatchDelayed());
        }
        else
        {
            StartCoroutine(FinishTestSuiteDelayed());
        }
    }

    private IEnumerator LoadNextMatchDelayed()
    {
        // Clean up resources between matches to prevent memory accumulation
        yield return Resources.UnloadUnusedAssets();
        System.GC.Collect();
        yield return new WaitForSecondsRealtime(0.5f);
        
        // Ensure timeScale and fixedDeltaTime are correct after scene cleanup
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = 0.02f * timeScale;
        
        LoadNextMatch();
    }

    private IEnumerator FinishTestSuiteDelayed()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        FinishTestSuite();
    }

    private void FinishTestSuite()
    {
        isRunning = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        FormationPreset.Active = null;

        float totalRealTime = Time.realtimeSinceStartup - testStartRealTime;
        Debug.Log($"[AutoMatchRunner] === TEST SUITE COMPLETE ===");
        Debug.Log($"[AutoMatchRunner] {matchCount} matches in {totalRealTime:F1}s real time");

        GenerateReport();
    }

    #endregion

    #region Report Generation

    private void GenerateReport()
    {
        string report = MultiMatchAnalyzer.GenerateReport(results, testRunId, matchTimeMinutes, timeScale);

        string dir = Path.Combine(Application.persistentDataPath, "ai_logs", $"auto_test_{testRunId}");
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        reportOutputPath = Path.Combine(dir, "aggregate_report.txt");
        File.WriteAllText(reportOutputPath, report);

        Debug.Log($"[AutoMatchRunner] Report saved: {reportOutputPath}");
        // Skip console dump of full report to reduce Unity Console bloat
    }

    #endregion
}

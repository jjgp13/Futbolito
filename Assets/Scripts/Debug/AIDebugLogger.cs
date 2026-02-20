using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// AI Debug Logger — Captures structured AI decision logs and writes them to file.
/// 
/// USAGE:
/// 1. Add this component to any GameObject in the scene (e.g., MatchController)
/// 2. Enable "Enable Logging" in Inspector
/// 3. Play the match — logs are written to Application.persistentDataPath/ai_logs/
/// 4. Right-click component → "Dump AI Log Summary" for analysis
/// 
/// LOG FORMAT:
/// [Frame] [Time] [Rod] [Action] Message
/// 
/// The logger automatically enables showDebugInfo on all AI components when active.
/// </summary>
public class AIDebugLogger : MonoBehaviour
{
    #region Configuration

    [Header("Logger Settings")]
    [SerializeField] private bool enableLogging = true;

    [Tooltip("Automatically start/stop logging when a match begins/ends")]
    [SerializeField] private bool autoLogOnMatch = true;

    [Tooltip("Also write to Unity Console (may impact performance)")]
    [SerializeField] private bool mirrorToConsole = false;

    [Tooltip("Maximum log entries before auto-flush to disk")]
    [SerializeField] private int flushInterval = 100;

    #endregion

    #region Singleton

    public static AIDebugLogger Instance { get; private set; }

    #endregion

    #region State

    private StreamWriter logWriter;
    private string logFilePath;
    private string logDirectory;
    private int entryCount = 0;
    private float matchStartTime;
    private bool isLogging = false;

    // Track original debug flag states for restoration
    private Dictionary<MonoBehaviour, bool> originalDebugFlags = new Dictionary<MonoBehaviour, bool>();

    // In-memory buffer for analysis
    private List<AILogEntry> logEntries = new List<AILogEntry>();

    #endregion

    #region Data Structures

    public struct AILogEntry
    {
        public int frame;
        public float timestamp;
        public string rodName;
        public string actionType;
        public string message;

        public override string ToString()
        {
            return $"[{frame}] [{timestamp:F3}s] [{rodName}] [{actionType}] {message}";
        }
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
    }

    private void OnEnable()
    {
        MatchController.OnMatchStart += OnMatchStarted;
        MatchController.OnMatchEnd += OnMatchEnded;
    }

    private void OnDisable()
    {
        MatchController.OnMatchStart -= OnMatchStarted;
        MatchController.OnMatchEnd -= OnMatchEnded;
    }

    private void Start()
    {
        // Only auto-start here if NOT using auto-match mode (legacy behavior)
        if (enableLogging && !autoLogOnMatch)
        {
            StartLogging();
            Invoke(nameof(ReEnableDebugFlags), 3f);
        }
    }

    private void OnMatchStarted()
    {
        if (!enableLogging || !autoLogOnMatch) return;

        // Stop previous session if still running
        if (isLogging)
        {
            RunAnalysis();
            StopLogging();
        }

        StartLogging();
        Log("SYSTEM", "MATCH_START", "Match started — auto-logging enabled");
        Invoke(nameof(ReEnableDebugFlags), 1f);
    }

    private void OnMatchEnded()
    {
        if (!isLogging || !autoLogOnMatch) return;

        Log("SYSTEM", "MATCH_END", "Match ended — finalizing log");
        RunAnalysis();
        StopLogging();
    }

    /// <summary>
    /// Re-scans for AI components and enables debug flags.
    /// Called with delay to catch components initialized after Start().
    /// </summary>
    private void ReEnableDebugFlags()
    {
        if (isLogging)
        {
            EnableAllDebugFlags();
        }
    }

    private void OnDestroy()
    {
        StopLogging();
        if (Instance == this) Instance = null;
    }

    private void OnApplicationQuit()
    {
        if (isLogging)
        {
            Log("SYSTEM", "SHUTDOWN", "Application quit — finalizing log");
            RunAnalysis();
            StopLogging();
        }
    }

    #endregion

    #region Logging Control

    public void StartLogging()
    {
        if (isLogging) return;

        matchStartTime = Time.time;
        entryCount = 0;
        logEntries.Clear();

        // Create log directory
        logDirectory = Path.Combine(Application.persistentDataPath, "ai_logs");
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        // Create timestamped log file
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        logFilePath = Path.Combine(logDirectory, $"ai_log_{timestamp}.txt");

        try
        {
            logWriter = new StreamWriter(logFilePath, false);
            logWriter.AutoFlush = false;
            isLogging = true;

            // Write header
            logWriter.WriteLine($"=== AI Debug Log — {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
            logWriter.WriteLine($"Log Path: {logFilePath}");
            logWriter.WriteLine($"Format: [Frame] [Time] [Rod] [Action] Message");
            logWriter.WriteLine(new string('=', 80));
            logWriter.WriteLine();

            // Subscribe to Unity log messages
            Application.logMessageReceived += OnUnityLogMessage;

            // Auto-enable debug flags
            EnableAllDebugFlags();

            Debug.Log($"[AIDebugLogger] Logging started → {logFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[AIDebugLogger] Failed to start logging: {e.Message}");
            isLogging = false;
        }
    }

    public void StopLogging()
    {
        if (!isLogging) return;

        Application.logMessageReceived -= OnUnityLogMessage;
        RestoreDebugFlags();

        if (logWriter != null)
        {
            logWriter.Flush();
            logWriter.Close();
            logWriter = null;
        }

        isLogging = false;
        Debug.Log($"[AIDebugLogger] Logging stopped. {entryCount} entries written to {logFilePath}");
    }

    #endregion

    #region Public API — Structured Logging

    /// <summary>
    /// Log a structured AI action event. Called directly by AI components.
    /// </summary>
    public static void Log(string rodName, string actionType, string message)
    {
        if (Instance == null || !Instance.isLogging) return;
        Instance.WriteEntry(rodName, actionType, message);
    }

    /// <summary>
    /// Log a possession change event.
    /// </summary>
    public static void LogPossession(string previousState, string newState)
    {
        Log("TEAM", "POSSESSION", $"{previousState} → {newState}");
    }

    /// <summary>
    /// Log a rod configuration change.
    /// </summary>
    public static void LogRodConfig(string configName, string reason)
    {
        Log("TEAM", "ROD_CONFIG", $"Config: {configName} — {reason}");
    }

    /// <summary>
    /// Log a shoot evaluation result.
    /// </summary>
    public static void LogShootEval(string rodName, bool result, string reason)
    {
        string status = result ? "WILL_SHOOT" : "NO_SHOOT";
        Log(rodName, "SHOOT_EVAL", $"{status} — {reason}");
    }

    /// <summary>
    /// Log magnet state change.
    /// </summary>
    public static void LogMagnet(string rodName, bool activated, string reason)
    {
        string action = activated ? "MAGNET_ON" : "MAGNET_OFF";
        Log(rodName, action, reason);
    }

    /// <summary>
    /// Log wall pass event.
    /// </summary>
    public static void LogWallPass(string rodName, bool executed, string reason)
    {
        string action = executed ? "WALLPASS_EXEC" : "WALLPASS_SKIP";
        Log(rodName, action, reason);
    }

    /// <summary>
    /// Log positioning/movement context.
    /// </summary>
    public static void LogPositioning(string rodName, string context, string movementMode)
    {
        Log(rodName, "POSITIONING", $"Context: {context}, Mode: {movementMode}");
    }

    /// <summary>
    /// Log GK-specific intercept event.
    /// </summary>
    public static void LogGKIntercept(string rodName, float threatLevel, float predictedY, float targetY)
    {
        Log(rodName, "GK_INTERCEPT", $"threat:{threatLevel:F2} predicted_y:{predictedY:F2} target_y:{targetY:F2}");
    }

    /// <summary>
    /// Log GK clearing decision.
    /// </summary>
    public static void LogGKClear(string rodName, float targetY, string reason)
    {
        Log(rodName, "GK_CLEAR", $"Clear toward Y={targetY:F2} — {reason}");
    }

    /// <summary>
    /// Log ball hit confirmation (shot connected with ball).
    /// </summary>
    public static void LogBallHit(string rodName, int shotLevel, float shotPower, Vector2 contactPoint)
    {
        Log(rodName, "BALL_HIT", $"Shot connected! level:{shotLevel} power:{shotPower:F0} contact:({contactPoint.x:F1},{contactPoint.y:F1})");
    }

    /// <summary>
    /// Log shot miss (shot window expired without ball contact).
    /// </summary>
    public static void LogShotMissed(string rodName, int shotLevel)
    {
        Log(rodName, "SHOT_MISSED", $"Shot window expired — ball not contacted, level:{shotLevel}");
    }

    /// <summary>
    /// Log goal scored event.
    /// </summary>
    public static void LogGoal(string goalSide, int leftScore, int rightScore)
    {
        Log("MATCH", "GOAL_SCORED", $"{goalSide} — Score: L{leftScore}-R{rightScore}");
    }

    /// <summary>
    /// Log magnet→shoot chain (magnet deactivated to start charging).
    /// </summary>
    public static void LogMagnetToShoot(string rodName)
    {
        Log(rodName, "MAGNET_TO_SHOOT", "Magnet deactivated → charge started (magnet→shoot chain)");
    }

    #endregion

    #region Internal

    private void WriteEntry(string rodName, string actionType, string message)
    {
        int frame = Time.frameCount;
        float elapsed = Time.time - matchStartTime;

        var entry = new AILogEntry
        {
            frame = frame,
            timestamp = elapsed,
            rodName = rodName,
            actionType = actionType,
            message = message
        };

        logEntries.Add(entry);

        string line = entry.ToString();

        if (logWriter != null)
        {
            logWriter.WriteLine(line);
            entryCount++;

            if (entryCount % flushInterval == 0)
            {
                logWriter.Flush();
            }
        }

        if (mirrorToConsole)
        {
            Debug.Log($"[AILog] {line}");
        }
    }

    /// <summary>
    /// Captures Unity Debug.Log messages that match AI prefixes
    /// </summary>
    private void OnUnityLogMessage(string logString, string stackTrace, LogType type)
    {
        if (!isLogging || logWriter == null) return;

        // Filter for AI-related messages
        if (logString.StartsWith("[AIRod") ||
            logString.StartsWith("[AITeam") ||
            logString.StartsWith("[AIDebug") ||
            logString.Contains("] Context:") ||
            logString.Contains("] GK Context:") ||
            logString.Contains("] AI Decision:") ||
            logString.Contains("] CLEARING LANE") ||
            logString.Contains("GK_INTERCEPT") ||
            logString.Contains("GK_CLEAR") ||
            logString.Contains("BALL_HIT") ||
            logString.Contains("SHOT_MISSED") ||
            logString.Contains("GOAL_SCORED") ||
            logString.Contains("MAGNET_TO_SHOOT"))
        {
            // Parse rod name from message if possible
            string rodName = ExtractRodName(logString);
            string actionType = "UNITY_LOG";

            WriteEntry(rodName, actionType, logString);
        }
    }

    private string ExtractRodName(string message)
    {
        // Try to extract rod name from patterns like [AIRodShootAction] RodName: ...
        int bracketEnd = message.IndexOf(']');
        if (bracketEnd < 0) return "UNKNOWN";

        string afterBracket = message.Substring(bracketEnd + 1).TrimStart();

        // Look for rod name before the next colon or dash
        int colonIdx = afterBracket.IndexOf(':');
        int dashIdx = afterBracket.IndexOf('-');
        int endIdx = -1;

        if (colonIdx >= 0 && dashIdx >= 0)
            endIdx = Math.Min(colonIdx, dashIdx);
        else if (colonIdx >= 0)
            endIdx = colonIdx;
        else if (dashIdx >= 0)
            endIdx = dashIdx;

        if (endIdx > 0)
        {
            return afterBracket.Substring(0, endIdx).Trim();
        }

        return "UNKNOWN";
    }

    #endregion

    #region Debug Flag Management

    private void EnableAllDebugFlags()
    {
        originalDebugFlags.Clear();

        // Find all AI components and enable their debug flags
        EnableDebugOnComponents<AIRodShootAction>("showDebugInfo");
        EnableDebugOnComponents<AIRodMagnetAction>("showDebugInfo");
        EnableDebugOnComponents<AIRodWallPassAction>("showDebugInfo");
        EnableDebugOnComponents<AIRodStateMachine>("showDebugInfo");

        Log("SYSTEM", "CONFIG", $"Enabled showDebugInfo on {originalDebugFlags.Count} AI components");
    }

    private void EnableDebugOnComponents<T>(string fieldName) where T : MonoBehaviour
    {
        T[] components = FindObjectsOfType<T>();
        foreach (T component in components)
        {
            var field = typeof(T).GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            if (field != null && field.FieldType == typeof(bool))
            {
                bool original = (bool)field.GetValue(component);
                originalDebugFlags[component] = original;
                field.SetValue(component, true);
            }
        }
    }

    private void RestoreDebugFlags()
    {
        foreach (var kvp in originalDebugFlags)
        {
            if (kvp.Key == null) continue;

            var field = kvp.Key.GetType().GetField("showDebugInfo",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(kvp.Key, kvp.Value);
            }
        }

        originalDebugFlags.Clear();
    }

    #endregion

    #region Analysis

    [ContextMenu("Dump AI Log Summary")]
    public void RunAnalysis()
    {
        if (logEntries.Count == 0)
        {
            Debug.Log("[AIDebugLogger] No log entries to analyze.");
            return;
        }

        string summary = AILogAnalyzer.Analyze(logEntries);

        // Write analysis file
        string analysisPath = logFilePath != null
            ? logFilePath.Replace(".txt", "_analysis.txt")
            : Path.Combine(logDirectory ?? Application.persistentDataPath, "ai_analysis.txt");

        try
        {
            File.WriteAllText(analysisPath, summary);
            Debug.Log($"[AIDebugLogger] Analysis written to {analysisPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[AIDebugLogger] Failed to write analysis: {e.Message}");
        }

        // Also log to console
        Debug.Log($"[AIDebugLogger] === ANALYSIS SUMMARY ===\n{summary}");
    }

    [ContextMenu("Open Log Directory")]
    public void OpenLogDirectory()
    {
        string dir = Path.Combine(Application.persistentDataPath, "ai_logs");
        if (Directory.Exists(dir))
        {
            Application.OpenURL(dir);
        }
        else
        {
            Debug.Log($"[AIDebugLogger] Log directory does not exist yet: {dir}");
        }
    }

    #endregion
}

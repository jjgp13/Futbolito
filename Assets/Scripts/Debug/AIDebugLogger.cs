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
/// AI components call AIDebugLogger.Log() directly — all output goes to file only.
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

    /// <summary>Path to the current match log file (null if not logging).</summary>
    public string CurrentLogFilePath => logFilePath;

    /// <summary>
    /// Deletes all files in the ai_logs directory. Call before starting a new test suite.
    /// </summary>
    public static void CleanLogDirectory()
    {
        string dir = Path.Combine(Application.persistentDataPath, "ai_logs");
        if (Directory.Exists(dir))
        {
            try
            {
                Directory.Delete(dir, true);
                if (!AutoMatchRunner.IsAutoMode) Debug.Log("[AIDebugLogger] Log directory cleaned");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[AIDebugLogger] Failed to clean log directory: {e.Message}");
            }
        }
    }

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
        if (enableLogging && !autoLogOnMatch)
        {
            StartLogging();
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
    }

    private void OnMatchEnded()
    {
        if (!isLogging || !autoLogOnMatch) return;

        Log("SYSTEM", "MATCH_END", "Match ended — finalizing log");
        RunAnalysis();
        StopLogging();
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

            isLogging = true;

            if (!AutoMatchRunner.IsAutoMode)
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

        if (logWriter != null)
        {
            logWriter.Flush();
            logWriter.Close();
            logWriter = null;
        }

        isLogging = false;
        if (!AutoMatchRunner.IsAutoMode)
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

        if (mirrorToConsole && !AutoMatchRunner.IsAutoMode)
        {
            Debug.Log($"[AILog] {line}");
        }
    }

    #endregion

    #region Analysis

    [ContextMenu("Dump AI Log Summary")]
    public void RunAnalysis()
    {
        if (logEntries.Count == 0)
        {
            if (!AutoMatchRunner.IsAutoMode) Debug.Log("[AIDebugLogger] No log entries to analyze.");
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
            if (!AutoMatchRunner.IsAutoMode) Debug.Log($"[AIDebugLogger] Analysis written to {analysisPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[AIDebugLogger] Failed to write analysis: {e.Message}");
        }
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

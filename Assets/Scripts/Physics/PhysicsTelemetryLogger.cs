using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// Logs physics telemetry per-preset session.
/// Every time you switch presets mid-match, the current session is closed
/// and a new one begins. All sessions are written to a single file on quit.
/// Add to the same GameObject as PhysicsPresetManager.
/// </summary>
public class PhysicsTelemetryLogger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool enableTelemetry = true;

    // Per-session tracking
    private float sessionStartTime;
    private string activePresetName = "Unknown";
    private float maxBallSpeed;
    private float totalBallSpeed;
    private int speedSamples;
    private int speedClampCount;
    private int wallBounces;
    private int figureBounces;
    private int totalCollisions;
    private int shotsAttempted;
    private int shotsConnected;
    private int lightShots;
    private int mediumShots;
    private int heavyShots;

    // Accumulated sessions
    private List<string> completedSessions = new List<string>();
    private float matchStartTime;

    private Rigidbody2D ballRb;
    private static PhysicsTelemetryLogger cachedInstance;

    private void OnEnable()
    {
        cachedInstance = this;
        MatchController.OnBallSpawned += OnBallSpawned;
        BallBehavior.OnBallImpact += OnBallImpact;
    }

    private void OnDisable()
    {
        cachedInstance = null;
        MatchController.OnBallSpawned -= OnBallSpawned;
        BallBehavior.OnBallImpact -= OnBallImpact;
    }

    private void Start()
    {
        matchStartTime = Time.time;
        StartNewSession();
    }

    private void StartNewSession()
    {
        sessionStartTime = Time.time;
        maxBallSpeed = 0f;
        totalBallSpeed = 0f;
        speedSamples = 0;
        speedClampCount = 0;
        wallBounces = 0;
        figureBounces = 0;
        totalCollisions = 0;
        shotsAttempted = 0;
        shotsConnected = 0;
        lightShots = 0;
        mediumShots = 0;
        heavyShots = 0;

        var manager = GetComponent<PhysicsPresetManager>();
        if (manager != null && manager.activePreset != null)
        {
            activePresetName = manager.activePreset.presetName;
        }

        Debug.Log($"[PhysicsTelemetry] Session started: {activePresetName}");
    }

    /// <summary>
    /// Call this when the preset changes to flush the current session and start a new one.
    /// </summary>
    public void OnPresetChanged()
    {
        if (!enableTelemetry) return;

        // Save current session
        string session = BuildSessionSummary();
        if (session != null)
        {
            completedSessions.Add(session);
            Debug.Log($"[PhysicsTelemetry] Session closed: {activePresetName}\n{session}");
        }

        // Start fresh
        StartNewSession();
    }

    private void OnBallSpawned()
    {
        var ball = GameObject.FindGameObjectWithTag("Ball");
        if (ball != null)
        {
            ballRb = ball.GetComponent<Rigidbody2D>();
        }
    }

    private void FixedUpdate()
    {
        if (!enableTelemetry || ballRb == null) return;

        float speed = ballRb.linearVelocity.magnitude;
        totalBallSpeed += speed;
        speedSamples++;

        if (speed > maxBallSpeed)
            maxBallSpeed = speed;
    }

    private void OnBallImpact(BallImpactEventArgs impact)
    {
        if (!enableTelemetry) return;

        totalCollisions++;

        switch (impact.Type)
        {
            case CollisionType.Wall:
                wallBounces++;
                break;
            case CollisionType.Figure:
                figureBounces++;
                break;
        }
    }

    public static void LogShotAttempt(int shotLevel)
    {
        if (cachedInstance == null || !cachedInstance.enableTelemetry) return;

        cachedInstance.shotsAttempted++;
        switch (shotLevel)
        {
            case 0: cachedInstance.lightShots++; break;
            case 1: cachedInstance.mediumShots++; break;
            case 2: cachedInstance.heavyShots++; break;
        }
    }

    public static void LogShotConnected()
    {
        if (cachedInstance == null || !cachedInstance.enableTelemetry) return;
        cachedInstance.shotsConnected++;
    }

    public static void LogSpeedClamped()
    {
        if (cachedInstance == null || !cachedInstance.enableTelemetry) return;
        cachedInstance.speedClampCount++;
    }

    private string BuildSessionSummary()
    {
        float duration = Time.time - sessionStartTime;
        if (duration < 1f) return null; // Skip very short sessions

        float avgSpeed = speedSamples > 0 ? totalBallSpeed / speedSamples : 0f;
        float hitRate = shotsAttempted > 0 ? (float)shotsConnected / shotsAttempted * 100f : 0f;
        float bouncesPerMin = duration > 0f ? totalCollisions / (duration / 60f) : 0f;

        return $@"--- PRESET: {activePresetName} ({duration:F1}s) ---
  Ball Speed    → avg: {avgSpeed:F1}  max: {maxBallSpeed:F1}  clamped: {speedClampCount}x
  Collisions    → total: {totalCollisions}  walls: {wallBounces}  figures: {figureBounces}  ({bouncesPerMin:F0}/min)
  Shots         → {shotsConnected}/{shotsAttempted} hit ({hitRate:F1}%)  light: {lightShots}  med: {mediumShots}  heavy: {heavyShots}
";
    }

    private void OnApplicationQuit()
    {
        WriteFullReport();
    }

    private void OnDestroy()
    {
        WriteFullReport();
    }

    private bool reportWritten = false;

    private void WriteFullReport()
    {
        if (!enableTelemetry || reportWritten) return;
        reportWritten = true;

        // Close the current active session
        string lastSession = BuildSessionSummary();
        if (lastSession != null)
        {
            completedSessions.Add(lastSession);
        }

        if (completedSessions.Count == 0) return;

        float totalDuration = Time.time - matchStartTime;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== PHYSICS TELEMETRY REPORT ===");
        sb.AppendLine($"Total duration: {totalDuration:F1}s");
        sb.AppendLine($"Presets tested: {completedSessions.Count}");
        sb.AppendLine();

        for (int i = 0; i < completedSessions.Count; i++)
        {
            sb.AppendLine($"[{i + 1}] {completedSessions[i]}");
        }

        string report = sb.ToString();

        // Write to log directory
        string logDir = Path.Combine(Application.persistentDataPath, "ai_logs");
        if (!Directory.Exists(logDir))
            Directory.CreateDirectory(logDir);

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filePath = Path.Combine(logDir, $"physics_telemetry_{timestamp}.txt");

        File.WriteAllText(filePath, report);
        Debug.Log($"[PhysicsTelemetry] Report written to: {filePath}\n{report}");
    }
}

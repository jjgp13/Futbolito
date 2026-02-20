using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// AI Log Analyzer — Parses structured AI log entries and produces diagnostic summaries.
/// 
/// Detects:
/// - Action frequency per rod (histogram)
/// - Stuck loops (repeated action patterns)
/// - Shoot failure reasons
/// - Possession timeline
/// </summary>
public static class AILogAnalyzer
{
    private const int STUCK_LOOP_THRESHOLD = 5;
    private const int PATTERN_WINDOW_SIZE = 4;

    public static string Analyze(List<AIDebugLogger.AILogEntry> entries)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== AI DEBUG LOG ANALYSIS ===");
        sb.AppendLine($"Total entries: {entries.Count}");
        sb.AppendLine($"Duration: {(entries.Count > 0 ? entries[entries.Count - 1].timestamp : 0):F1}s");
        sb.AppendLine();

        AnalyzeActionFrequency(entries, sb);
        AnalyzeShootFailures(entries, sb);
        AnalyzeShotEffectiveness(entries, sb);
        AnalyzePlayerVsAI(entries, sb);
        AnalyzeStuckLoops(entries, sb);
        AnalyzePossessionTimeline(entries, sb);
        AnalyzeMagnetWallPassPattern(entries, sb);
        AnalyzeWallPassFollowUp(entries, sb);

        return sb.ToString();
    }

    #region Action Frequency

    private static void AnalyzeActionFrequency(List<AIDebugLogger.AILogEntry> entries, StringBuilder sb)
    {
        sb.AppendLine("--- ACTION FREQUENCY PER ROD ---");

        var rodActions = new Dictionary<string, Dictionary<string, int>>();

        foreach (var entry in entries)
        {
            if (entry.rodName == "SYSTEM" || entry.rodName == "UNKNOWN") continue;

            if (!rodActions.ContainsKey(entry.rodName))
                rodActions[entry.rodName] = new Dictionary<string, int>();

            var actions = rodActions[entry.rodName];
            if (!actions.ContainsKey(entry.actionType))
                actions[entry.actionType] = 0;
            actions[entry.actionType]++;
        }

        foreach (var rod in rodActions.OrderBy(r => r.Key))
        {
            sb.AppendLine($"\n  {rod.Key}:");
            foreach (var action in rod.Value.OrderByDescending(a => a.Value))
            {
                string bar = new string('█', System.Math.Min(action.Value, 50));
                sb.AppendLine($"    {action.Key,-20} {action.Value,5}x  {bar}");
            }
        }
        sb.AppendLine();
    }

    #endregion

    #region Shoot Failures

    private static void AnalyzeShootFailures(List<AIDebugLogger.AILogEntry> entries, StringBuilder sb)
    {
        sb.AppendLine("--- SHOOT FAILURE REASONS ---");

        var shootEntries = entries.Where(e => e.actionType == "SHOOT_EVAL").ToList();
        int totalEvals = shootEntries.Count;
        int willShoot = shootEntries.Count(e => e.message.Contains("WILL_SHOOT"));
        int noShoot = shootEntries.Count(e => e.message.Contains("NO_SHOOT"));

        sb.AppendLine($"  Total evaluations: {totalEvals}");
        sb.AppendLine($"  Will shoot: {willShoot} ({(totalEvals > 0 ? (float)willShoot / totalEvals * 100 : 0):F1}%)");
        sb.AppendLine($"  No shoot:   {noShoot} ({(totalEvals > 0 ? (float)noShoot / totalEvals * 100 : 0):F1}%)");

        // Group failure reasons
        var failureReasons = new Dictionary<string, int>();
        foreach (var entry in shootEntries.Where(e => e.message.Contains("NO_SHOOT")))
        {
            string reason = ExtractReason(entry.message);
            if (!failureReasons.ContainsKey(reason))
                failureReasons[reason] = 0;
            failureReasons[reason]++;
        }

        if (failureReasons.Count > 0)
        {
            sb.AppendLine("\n  Failure breakdown:");
            foreach (var reason in failureReasons.OrderByDescending(r => r.Value))
            {
                float pct = totalEvals > 0 ? (float)reason.Value / noShoot * 100 : 0;
                sb.AppendLine($"    {reason.Key,-40} {reason.Value,4}x ({pct:F1}%)");
            }
        }
        sb.AppendLine();
    }

    private static string ExtractReason(string message)
    {
        int dashIdx = message.IndexOf('—');
        if (dashIdx >= 0 && dashIdx + 2 < message.Length)
        {
            return message.Substring(dashIdx + 2).Trim();
        }
        return message;
    }

    #endregion

    #region Player vs AI Comparison

    private static void AnalyzePlayerVsAI(List<AIDebugLogger.AILogEntry> entries, StringBuilder sb)
    {
        sb.AppendLine("--- PLAYER vs AI ACTION COMPARISON ---");

        // Player actions
        int playerShoots = entries.Count(e => e.actionType == "PLAYER_SHOOT");
        int playerCharges = entries.Count(e => e.actionType == "PLAYER_CHARGE_START");
        int playerMagnetOn = entries.Count(e => e.actionType == "PLAYER_MAGNET_ON");
        int playerMagnetOff = entries.Count(e => e.actionType == "PLAYER_MAGNET_OFF");
        int playerWallPass = entries.Count(e => e.actionType == "PLAYER_WALLPASS");

        // AI actions
        int aiShoots = entries.Count(e => e.actionType == "SHOOT_EXEC");
        int aiWhiffs = entries.Count(e => e.actionType == "SHOOT_WHIFF");
        int aiMagnetOn = entries.Count(e => e.actionType == "MAGNET_ON");
        int aiMagnetOff = entries.Count(e => e.actionType == "MAGNET_OFF");
        int aiWallPass = entries.Count(e => e.actionType == "WALLPASS_EXEC" && e.message.Contains("Executed"));

        bool hasPlayerData = playerShoots + playerMagnetOn + playerWallPass > 0;

        if (!hasPlayerData)
        {
            sb.AppendLine("  No player actions logged (player logging not active in this match)");
            sb.AppendLine();
            return;
        }

        sb.AppendLine($"  {"Action",-20} {"Player",8} {"AI",8}  {"Ratio",8}");
        sb.AppendLine($"  {"------",-20} {"------",8} {"--",8}  {"-----",8}");

        AppendComparisonRow(sb, "Shoots", playerShoots, aiShoots + aiWhiffs);
        AppendComparisonRow(sb, "Magnet ON", playerMagnetOn, aiMagnetOn);
        AppendComparisonRow(sb, "Wall Pass", playerWallPass, aiWallPass);

        // Player charge times
        var playerChargeEntries = entries.Where(e => e.actionType == "PLAYER_SHOOT").ToList();
        if (playerChargeEntries.Count > 0)
        {
            sb.AppendLine("\n  Player shot charge times:");
            foreach (var shot in playerChargeEntries)
            {
                sb.AppendLine($"    [{shot.timestamp:F1}s] {shot.message}");
            }
        }

        sb.AppendLine();
    }

    private static void AppendComparisonRow(StringBuilder sb, string action, int playerCount, int aiCount)
    {
        string ratio = aiCount > 0 ? $"{(float)playerCount / aiCount:F1}x" : (playerCount > 0 ? "∞" : "0");
        sb.AppendLine($"  {action,-20} {playerCount,8} {aiCount,8}  {ratio,8}");
    }

    #endregion

    #region Stuck Loop Detection

    private static void AnalyzeStuckLoops(List<AIDebugLogger.AILogEntry> entries, StringBuilder sb)
    {
        sb.AppendLine("--- STUCK LOOP DETECTION ---");

        // Group entries by rod, look for repeating action patterns
        var rodEntries = new Dictionary<string, List<string>>();

        foreach (var entry in entries)
        {
            if (entry.rodName == "SYSTEM" || entry.rodName == "UNKNOWN" ||
                entry.actionType == "UNITY_LOG" || entry.actionType == "MAGNET_OFF") continue;

            if (!rodEntries.ContainsKey(entry.rodName))
                rodEntries[entry.rodName] = new List<string>();

            rodEntries[entry.rodName].Add(entry.actionType);
        }

        bool foundLoop = false;

        foreach (var rod in rodEntries)
        {
            var actions = rod.Value;
            if (actions.Count < PATTERN_WINDOW_SIZE * STUCK_LOOP_THRESHOLD) continue;

            // Sliding window: check if a pattern of PATTERN_WINDOW_SIZE actions repeats
            for (int windowStart = 0; windowStart <= actions.Count - PATTERN_WINDOW_SIZE * STUCK_LOOP_THRESHOLD; windowStart++)
            {
                var pattern = actions.GetRange(windowStart, PATTERN_WINDOW_SIZE);
                int repeats = 0;

                for (int i = windowStart; i <= actions.Count - PATTERN_WINDOW_SIZE; i += PATTERN_WINDOW_SIZE)
                {
                    var chunk = actions.GetRange(i, PATTERN_WINDOW_SIZE);
                    if (PatternMatches(pattern, chunk))
                        repeats++;
                    else
                        break;
                }

                if (repeats >= STUCK_LOOP_THRESHOLD)
                {
                    foundLoop = true;
                    string patternStr = string.Join(" → ", pattern);
                    sb.AppendLine($"  ⚠ STUCK LOOP on {rod.Key}: [{patternStr}] repeated {repeats}x");
                    sb.AppendLine($"    Starting at action index {windowStart}");
                    break;
                }
            }
        }

        if (!foundLoop)
        {
            sb.AppendLine("  ✓ No stuck loops detected");
        }
        sb.AppendLine();
    }

    private static bool PatternMatches(List<string> a, List<string> b)
    {
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
        {
            if (a[i] != b[i]) return false;
        }
        return true;
    }

    #endregion

    #region Possession Timeline

    private static void AnalyzePossessionTimeline(List<AIDebugLogger.AILogEntry> entries, StringBuilder sb)
    {
        sb.AppendLine("--- POSSESSION TIMELINE ---");

        var possessionEntries = entries.Where(e => e.actionType == "POSSESSION").ToList();

        if (possessionEntries.Count == 0)
        {
            sb.AppendLine("  No possession changes recorded");
            sb.AppendLine();
            return;
        }

        // Count time in each state
        var stateDurations = new Dictionary<string, float>();
        string currentState = "Free";

        for (int i = 0; i < possessionEntries.Count; i++)
        {
            string newState = possessionEntries[i].message.Contains("→")
                ? possessionEntries[i].message.Split('→')[1].Trim()
                : possessionEntries[i].message;

            float duration = 0;
            if (i + 1 < possessionEntries.Count)
                duration = possessionEntries[i + 1].timestamp - possessionEntries[i].timestamp;
            else if (entries.Count > 0)
                duration = entries[entries.Count - 1].timestamp - possessionEntries[i].timestamp;

            if (!stateDurations.ContainsKey(newState))
                stateDurations[newState] = 0;
            stateDurations[newState] += duration;

            currentState = newState;
        }

        float totalTime = stateDurations.Values.Sum();
        foreach (var state in stateDurations.OrderByDescending(s => s.Value))
        {
            float pct = totalTime > 0 ? state.Value / totalTime * 100 : 0;
            sb.AppendLine($"  {state.Key,-15} {state.Value:F1}s ({pct:F1}%)");
        }

        sb.AppendLine($"  Total changes: {possessionEntries.Count}");
        sb.AppendLine();
    }

    #endregion

    #region Magnet-WallPass Pattern Analysis

    /// <summary>
    /// Specifically checks for the reported bug: magnet ↔ wallpass loop without shooting
    /// </summary>
    private static void AnalyzeMagnetWallPassPattern(List<AIDebugLogger.AILogEntry> entries, StringBuilder sb)
    {
        sb.AppendLine("--- MAGNET ↔ WALLPASS LOOP CHECK ---");

        // Look for sequences of MAGNET_ON → WALLPASS_EXEC without any SHOOT_EVAL(WILL_SHOOT) in between
        var rodEntries = new Dictionary<string, List<AIDebugLogger.AILogEntry>>();

        foreach (var entry in entries)
        {
            if (entry.rodName == "SYSTEM" || entry.rodName == "UNKNOWN") continue;
            if (!rodEntries.ContainsKey(entry.rodName))
                rodEntries[entry.rodName] = new List<AIDebugLogger.AILogEntry>();
            rodEntries[entry.rodName].Add(entry);
        }

        bool foundIssue = false;

        foreach (var rod in rodEntries)
        {
            int magnetOnCount = 0;
            int wallPassCount = 0;
            int shootCount = 0;
            int magnetWallPassWithoutShoot = 0;
            bool lastWasMagnet = false;

            foreach (var entry in rod.Value)
            {
                if (entry.actionType == "MAGNET_ON")
                {
                    magnetOnCount++;
                    lastWasMagnet = true;
                }
                else if (entry.actionType == "WALLPASS_EXEC")
                {
                    wallPassCount++;
                    if (lastWasMagnet)
                    {
                        magnetWallPassWithoutShoot++;
                    }
                    lastWasMagnet = false;
                }
                else if (entry.actionType == "SHOOT_EVAL" && entry.message.Contains("WILL_SHOOT"))
                {
                    shootCount++;
                    lastWasMagnet = false;
                }
            }

            if (magnetWallPassWithoutShoot > 3)
            {
                foundIssue = true;
                sb.AppendLine($"  ⚠ {rod.Key}: MAGNET→WALLPASS without shooting: {magnetWallPassWithoutShoot}x");
                sb.AppendLine($"    Magnet activations: {magnetOnCount}, Wall passes: {wallPassCount}, Shots: {shootCount}");

                if (shootCount == 0)
                    sb.AppendLine($"    🔴 ZERO SHOTS — AI never shoots on this rod!");
                else if (shootCount < wallPassCount / 2)
                    sb.AppendLine($"    🟡 Low shot ratio — shoots {shootCount}x vs wallpass {wallPassCount}x");
            }
        }

        if (!foundIssue)
        {
            sb.AppendLine("  ✓ No magnet↔wallpass loops detected");
        }
        sb.AppendLine();
    }

    #endregion

    #region Shot Effectiveness Analysis

    private static void AnalyzeShotEffectiveness(List<AIDebugLogger.AILogEntry> entries, StringBuilder sb)
    {
        sb.AppendLine("--- SHOT EFFECTIVENESS ---");

        var shotsFired = entries.Where(e => e.actionType == "SHOOT_EXEC").ToList();
        var shotsWhiffed = entries.Where(e => e.actionType == "SHOOT_WHIFF").ToList();
        var ballHits = entries.Where(e => e.actionType == "BALL_HIT").ToList();
        var shotsMissed = entries.Where(e => e.actionType == "SHOT_MISSED").ToList();
        var goals = entries.Where(e => e.actionType == "GOAL_SCORED").ToList();
        var magnetToShoot = entries.Where(e => e.actionType == "MAGNET_TO_SHOOT").ToList();

        int totalAttempts = shotsFired.Count + shotsWhiffed.Count;
        sb.AppendLine($"  Shots attempted: {totalAttempts}");
        sb.AppendLine($"  Shots fired:     {shotsFired.Count}");
        sb.AppendLine($"  Shots whiffed:   {shotsWhiffed.Count} ({(totalAttempts > 0 ? (float)shotsWhiffed.Count / totalAttempts * 100 : 0):F1}%)");
        sb.AppendLine($"  Ball hits:       {ballHits.Count} (confirmed contact)");
        sb.AppendLine($"  Shots missed:    {shotsMissed.Count} (window expired, no contact)");

        if (shotsFired.Count > 0)
        {
            // Hit rate = ball hits / (shots fired that weren't whiffs)
            // Note: multiple figures fire per shot, so ballHits may count multiple per rod shot
            sb.AppendLine($"  Contact rate:    {(ballHits.Count > 0 ? "YES" : "NO CONTACTS")} ({ballHits.Count} contacts from {shotsFired.Count} shots)");
        }

        sb.AppendLine($"  Magnet→Shoot chains: {magnetToShoot.Count}");

        // Per-rod breakdown
        sb.AppendLine("\n  Per-rod breakdown:");
        var rodShots = shotsFired.GroupBy(e => e.rodName);
        foreach (var rod in rodShots.OrderBy(r => r.Key))
        {
            int rodHits = ballHits.Count(e => e.rodName == rod.Key);
            int rodMisses = shotsMissed.Count(e => e.rodName == rod.Key);
            int rodWhiffs = shotsWhiffed.Count(e => e.rodName == rod.Key);
            sb.AppendLine($"    {rod.Key,-20} fired:{rod.Count()} hits:{rodHits} missed:{rodMisses} whiffed:{rodWhiffs}");
        }

        // Goals
        if (goals.Count > 0)
        {
            sb.AppendLine($"\n  Goals scored: {goals.Count}");
            foreach (var goal in goals)
            {
                sb.AppendLine($"    [{goal.timestamp:F1}s] {goal.message}");
            }
        }
        else
        {
            sb.AppendLine("\n  Goals scored: 0");
        }

        sb.AppendLine();
    }

    #endregion

    #region Wall Pass Follow-Up Analysis

    private static void AnalyzeWallPassFollowUp(List<AIDebugLogger.AILogEntry> entries, StringBuilder sb)
    {
        sb.AppendLine("--- WALL PASS FOLLOW-UP ---");

        var wallPasses = entries.Where(e => e.actionType == "WALLPASS_EXEC" && e.message.Contains("Executed")).ToList();

        if (wallPasses.Count == 0)
        {
            sb.AppendLine("  No wall passes executed");
            sb.AppendLine();
            return;
        }

        sb.AppendLine($"  Wall passes executed: {wallPasses.Count}");

        int followedByShoot = 0;
        int followedByMagnet = 0;
        int followedByNothing = 0;

        foreach (var wp in wallPasses)
        {
            // Look for the next action on the same rod within 2 seconds
            var followUp = entries
                .Where(e => e.timestamp > wp.timestamp && e.timestamp < wp.timestamp + 2f
                       && e.rodName == wp.rodName
                       && (e.actionType == "SHOOT_EXEC" || e.actionType == "MAGNET_ON" || e.actionType == "BALL_HIT"))
                .FirstOrDefault();

            if (followUp.actionType == "SHOOT_EXEC" || followUp.actionType == "BALL_HIT")
                followedByShoot++;
            else if (followUp.actionType == "MAGNET_ON")
                followedByMagnet++;
            else
                followedByNothing++;
        }

        sb.AppendLine($"  → Followed by shoot:   {followedByShoot} ({(wallPasses.Count > 0 ? (float)followedByShoot / wallPasses.Count * 100 : 0):F0}%)");
        sb.AppendLine($"  → Followed by magnet:  {followedByMagnet} ({(wallPasses.Count > 0 ? (float)followedByMagnet / wallPasses.Count * 100 : 0):F0}%)");
        sb.AppendLine($"  → No follow-up action: {followedByNothing}");
        sb.AppendLine();
    }

    #endregion
}

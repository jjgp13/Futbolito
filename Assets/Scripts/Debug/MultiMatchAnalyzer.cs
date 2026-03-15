using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Aggregates analysis across multiple automated AI vs AI matches.
/// Evaluates results against core design goals:
/// - Match duration (2-5 min target)
/// - Scoring pace (frenetic, goal-oriented gameplay)
/// - Competitive balance (neither side dominates)
/// - AI action effectiveness (actions lead to goals/defense)
/// - Gameplay pace (ball-in-play %, action density)
/// </summary>
public static class MultiMatchAnalyzer
{
    #region Design Goal Thresholds

    private const float MIN_MATCH_DURATION_SEC = 120f; // 2 minutes
    private const float MAX_MATCH_DURATION_SEC = 300f; // 5 minutes
    private const float MIN_GOALS_PER_MATCH = 3f;      // At least 3 goals for excitement
    private const float MAX_GOALS_PER_MATCH = 10f;     // Cap before it feels broken
    private const float BALANCE_WIN_RATE_MIN = 0.30f;   // Neither side wins less than 30%
    private const float BALANCE_WIN_RATE_MAX = 0.70f;   // Neither side wins more than 70%
    private const float MAX_SCORELESS_FRACTION = 0.30f;  // No more than 30% of matches scoreless

    #endregion

    public static string GenerateReport(
        List<AutoMatchRunner.MatchResult> results,
        string testRunId,
        int matchTimeMinutes,
        float timeScale)
    {
        var sb = new StringBuilder();

        WriteHeader(sb, results, testRunId, matchTimeMinutes, timeScale);
        WriteMatchResults(sb, results);
        WriteDesignGoalAssessment(sb, results, matchTimeMinutes);
        WritePerDifficultyBreakdown(sb, results);
        WritePerPresetBreakdown(sb, results);
        WritePerFormationBreakdown(sb, results);
        WritePerMatchAnalysisSummaries(sb, results);
        WriteRecommendations(sb, results);

        return sb.ToString();
    }

    #region Report Sections

    private static void WriteHeader(StringBuilder sb, List<AutoMatchRunner.MatchResult> results,
        string testRunId, int matchTimeMinutes, float timeScale)
    {
        float totalRealTime = results.Sum(r => r.realDurationSeconds);

        sb.AppendLine("╔══════════════════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║              AUTOMATED AI TEST SUITE — AGGREGATE REPORT                 ║");
        sb.AppendLine("╚══════════════════════════════════════════════════════════════════════════╝");
        sb.AppendLine();
        sb.AppendLine($"  Run ID:          {testRunId}");
        sb.AppendLine($"  Matches:         {results.Count}");
        sb.AppendLine($"  Match Time:      {matchTimeMinutes} min");
        sb.AppendLine($"  Time Scale:      {timeScale}x");
        sb.AppendLine($"  Total Real Time: {totalRealTime:F1}s ({totalRealTime / 60f:F1} min)");

        // List unique difficulty combos used
        var combos = results.Select(r => $"{DiffName(r.leftDifficulty)} vs {DiffName(r.rightDifficulty)}").Distinct();
        sb.AppendLine($"  Difficulty Combos: {string.Join(", ", combos)}");

        // List unique physics presets used
        var presets = results.Select(r => r.physicsPreset ?? "Default").Distinct();
        sb.AppendLine($"  Physics Presets:   {string.Join(", ", presets)}");

        // List unique formation presets used
        var formations = results.Select(r => r.formationPreset ?? "Default").Distinct();
        sb.AppendLine($"  Formation Presets: {string.Join(", ", formations)}");
        sb.AppendLine();
    }

    private static void WriteMatchResults(StringBuilder sb, List<AutoMatchRunner.MatchResult> results)
    {
        sb.AppendLine("┌──────────────────────────────────────────────────────────────────────────┐");
        sb.AppendLine("│                           MATCH RESULTS                                  │");
        sb.AppendLine("└──────────────────────────────────────────────────────────────────────────┘");
        sb.AppendLine();

        sb.AppendLine($"  {"#",-4} {"Score",-10} {"Winner",-8} {"End",-8} {"Difficulty",-16} {"Physics",-16} {"Formation",-16} {"Time",-8} {"Rsp",-4} {"Bmp",-4}");
        sb.AppendLine($"  {"─",-4} {"─────",-10} {"──────",-8} {"───",-8} {"──────────",-16} {"───────",-16} {"─────────",-16} {"────",-8} {"───",-4} {"───",-4}");

        foreach (var r in results)
        {
            string score = $"L{r.leftScore}-R{r.rightScore}";
            string end = r.knockout ? "KO" : "Time";
            string diff = $"{DiffName(r.leftDifficulty)}v{DiffName(r.rightDifficulty)}";
            string preset = (r.physicsPreset ?? "Default").Length > 14
                ? (r.physicsPreset ?? "Default").Substring(0, 14)
                : r.physicsPreset ?? "Default";
            string formation = (r.formationPreset ?? "Default").Length > 14
                ? (r.formationPreset ?? "Default").Substring(0, 14)
                : r.formationPreset ?? "Default";
            sb.AppendLine($"  {r.matchNumber,-4} {score,-10} {r.winner,-8} {end,-8} {diff,-16} {preset,-16} {formation,-16} {r.realDurationSeconds:F1}s   {r.ballRestarts,-4}{r.bumpCount}");
        }

        sb.AppendLine();

        // Summary stats
        int leftWins = results.Count(r => r.winner == "Left");
        int rightWins = results.Count(r => r.winner == "Right");
        int draws = results.Count(r => r.winner == "Draw");
        float avgLeftGoals = (float)results.Average(r => r.leftScore);
        float avgRightGoals = (float)results.Average(r => r.rightScore);
        float avgTotalGoals = (float)results.Average(r => r.leftScore + r.rightScore);
        int knockouts = results.Count(r => r.knockout);

        sb.AppendLine($"  Left wins:  {leftWins}/{results.Count} ({(float)leftWins / results.Count * 100:F0}%)");
        sb.AppendLine($"  Right wins: {rightWins}/{results.Count} ({(float)rightWins / results.Count * 100:F0}%)");
        sb.AppendLine($"  Draws:      {draws}/{results.Count} ({(float)draws / results.Count * 100:F0}%)");
        sb.AppendLine($"  Knockouts:  {knockouts}/{results.Count} ({(float)knockouts / results.Count * 100:F0}%)");
        sb.AppendLine();
        sb.AppendLine($"  Avg goals/match:  {avgTotalGoals:F1} (Left: {avgLeftGoals:F1}, Right: {avgRightGoals:F1})");
        sb.AppendLine($"  Avg score diff:   {(float)results.Average(r => Mathf.Abs(r.leftScore - r.rightScore)):F1}");
        sb.AppendLine();
    }

    private static void WriteDesignGoalAssessment(StringBuilder sb, List<AutoMatchRunner.MatchResult> results,
        int matchTimeMinutes)
    {
        sb.AppendLine("┌─────────────────────────────────────────────────────────────┐");
        sb.AppendLine("│                  DESIGN GOALS ASSESSMENT                    │");
        sb.AppendLine("└─────────────────────────────────────────────────────────────┘");
        sb.AppendLine();

        // 1. Match Duration
        float matchDurationSec = matchTimeMinutes * 60f;
        int knockouts = results.Count(r => r.knockout);
        float knockoutRate = (float)knockouts / results.Count;
        bool durationPass = matchDurationSec >= MIN_MATCH_DURATION_SEC && matchDurationSec <= MAX_MATCH_DURATION_SEC;
        bool knockoutTooHigh = knockoutRate > 0.6f; // More than 60% knockouts means matches end too fast

        string durationIcon = (durationPass && !knockoutTooHigh) ? "PASS ✓" : (knockoutTooHigh ? "WARN !" : "FAIL ✗");
        sb.AppendLine($"  [{durationIcon}] MATCH DURATION (target: 2-5 min)");
        sb.AppendLine($"         Configured: {matchTimeMinutes} min");
        sb.AppendLine($"         Knockout rate: {knockoutRate * 100:F0}% ({knockouts}/{results.Count})");
        if (knockoutTooHigh)
            sb.AppendLine($"         ⚠ High knockout rate — most matches end before time. Consider rebalancing.");
        sb.AppendLine();

        // 2. Scoring Pace
        float avgGoals = (float)results.Average(r => r.leftScore + r.rightScore);
        float goalsPerMinute = avgGoals / matchTimeMinutes;
        int scorelessMatches = results.Count(r => r.leftScore == 0 && r.rightScore == 0);
        bool scoringPass = avgGoals >= MIN_GOALS_PER_MATCH && avgGoals <= MAX_GOALS_PER_MATCH;
        bool scorelessOk = (float)scorelessMatches / results.Count <= MAX_SCORELESS_FRACTION;

        string scoringIcon = (scoringPass && scorelessOk) ? "PASS ✓" : "WARN !";
        sb.AppendLine($"  [{scoringIcon}] SCORING PACE (target: {MIN_GOALS_PER_MATCH}-{MAX_GOALS_PER_MATCH} goals/match)");
        sb.AppendLine($"         Avg goals/match: {avgGoals:F1}");
        sb.AppendLine($"         Goals/minute: {goalsPerMinute:F2}");
        sb.AppendLine($"         Scoreless matches: {scorelessMatches}");
        if (avgGoals < MIN_GOALS_PER_MATCH)
            sb.AppendLine($"         ⚠ Too few goals — gameplay may feel slow. AI needs to be more aggressive offensively.");
        if (avgGoals > MAX_GOALS_PER_MATCH)
            sb.AppendLine($"         ⚠ Too many goals — defense may be too weak. AI needs better defensive positioning.");
        if (!scorelessOk)
            sb.AppendLine($"         ⚠ Too many scoreless matches — AI isn't converting opportunities into goals.");
        sb.AppendLine();

        // 3. Competitive Balance
        float leftWinRate = (float)results.Count(r => r.winner == "Left") / results.Count;
        float rightWinRate = (float)results.Count(r => r.winner == "Right") / results.Count;
        float avgScoreDiff = (float)results.Average(r => Mathf.Abs(r.leftScore - r.rightScore));
        bool balancePass = leftWinRate >= BALANCE_WIN_RATE_MIN && leftWinRate <= BALANCE_WIN_RATE_MAX;
        bool closenessPass = avgScoreDiff <= 2.5f;

        string balanceIcon = (balancePass && closenessPass) ? "PASS ✓" : "WARN !";
        sb.AppendLine($"  [{balanceIcon}] COMPETITIVE BALANCE (target: 30-70% win rate each side)");
        sb.AppendLine($"         Left win rate:  {leftWinRate * 100:F0}%");
        sb.AppendLine($"         Right win rate: {rightWinRate * 100:F0}%");
        sb.AppendLine($"         Avg score differential: {avgScoreDiff:F1}");
        if (!balancePass)
            sb.AppendLine($"         ⚠ One side dominates — check for positional or mechanical advantage.");
        if (!closenessPass)
            sb.AppendLine($"         ⚠ Matches not close enough — average {avgScoreDiff:F1} goal difference is too high.");
        sb.AppendLine();

        // 4. Goal-Directed Behavior (from per-match analysis files)
        WriteGoalDirectedAnalysis(sb, results);

        // 5. Gameplay Pace (from per-match analysis files)
        WriteGameplayPaceAnalysis(sb, results);

        // 6. Ball Health (respawns and dead ball time)
        WriteBallHealthAssessment(sb, results);
    }

    private static void WriteGoalDirectedAnalysis(StringBuilder sb, List<AutoMatchRunner.MatchResult> results)
    {
        // Parse per-match analysis files for shot effectiveness data
        int totalShotsAttempted = 0;
        int totalBallHits = 0;
        int totalGoals = 0;
        int totalMagnetToShoot = 0;
        int totalWallPasses = 0;
        int totalWallPassToShoot = 0;
        int filesRead = 0;

        foreach (var r in results)
        {
            if (string.IsNullOrEmpty(r.logFilePath)) continue;
            string analysisPath = r.logFilePath.Replace(".txt", "_analysis.txt");
            if (!File.Exists(analysisPath)) continue;

            string content = File.ReadAllText(analysisPath);
            filesRead++;

            totalShotsAttempted += ExtractInt(content, "Shots attempted:");
            totalBallHits += ExtractInt(content, "Ball hits:");
            totalGoals += r.leftScore + r.rightScore;
            totalMagnetToShoot += ExtractInt(content, "Magnet→Shoot chains:");
            totalWallPasses += ExtractInt(content, "Wall passes executed:");
            totalWallPassToShoot += ExtractInt(content, "→ Followed by shoot:");
        }

        bool hasData = filesRead > 0 && totalShotsAttempted > 0;
        float contactRate = hasData ? (float)totalBallHits / totalShotsAttempted * 100 : 0;
        float goalsPerShot = hasData && totalBallHits > 0 ? (float)totalGoals / totalBallHits * 100 : 0;

        string effectIcon = hasData ? (contactRate > 30 ? "PASS ✓" : "WARN !") : "INFO ?";
        sb.AppendLine($"  [{effectIcon}] GOAL-DIRECTED AI BEHAVIOR");
        if (hasData)
        {
            sb.AppendLine($"         Shots attempted: {totalShotsAttempted} across {filesRead} matches");
            sb.AppendLine($"         Ball contact rate: {contactRate:F1}%");
            sb.AppendLine($"         Goals/ball-hit: {goalsPerShot:F1}%");
            sb.AppendLine($"         Magnet→Shoot chains: {totalMagnetToShoot} (mechanic combos)");
            sb.AppendLine($"         Wall passes: {totalWallPasses} (→ shoot: {totalWallPassToShoot})");

            if (contactRate < 20)
                sb.AppendLine($"         ⚠ Very low contact rate — AI shoots but misses. Check positioning/distance.");
            if (totalMagnetToShoot == 0 && totalShotsAttempted > 10)
                sb.AppendLine($"         ⚠ No magnet→shoot chains — AI isn't using magnet to set up goals.");
            if (totalWallPasses == 0 && totalShotsAttempted > 10)
                sb.AppendLine($"         ⚠ No wall passes used — consider relaxing wall pass trigger conditions.");
            if (totalWallPasses > 0 && totalWallPassToShoot == 0)
                sb.AppendLine($"         ⚠ Wall passes never followed by shots — passes don't lead to goal opportunities.");
        }
        else
        {
            sb.AppendLine($"         No per-match analysis files found — enable AIDebugLogger for detailed metrics.");
        }
        sb.AppendLine();
    }

    private static void WriteGameplayPaceAnalysis(StringBuilder sb, List<AutoMatchRunner.MatchResult> results)
    {
        int totalEntries = 0;
        float totalDuration = 0;
        int filesRead = 0;

        foreach (var r in results)
        {
            if (string.IsNullOrEmpty(r.logFilePath)) continue;
            string analysisPath = r.logFilePath.Replace(".txt", "_analysis.txt");
            if (!File.Exists(analysisPath)) continue;

            string content = File.ReadAllText(analysisPath);
            filesRead++;

            totalEntries += ExtractInt(content, "Total entries:");
            totalDuration += ExtractFloat(content, "Duration:");
        }

        bool hasData = filesRead > 0 && totalDuration > 0;
        float actionsPerMinute = hasData ? totalEntries / (totalDuration / 60f) : 0;

        string paceIcon = hasData ? (actionsPerMinute > 5 ? "PASS ✓" : "WARN !") : "INFO ?";
        sb.AppendLine($"  [{paceIcon}] GAMEPLAY PACE");
        if (hasData)
        {
            sb.AppendLine($"         Total AI actions: {totalEntries} across {filesRead} matches");
            sb.AppendLine($"         Actions/minute: {actionsPerMinute:F1}");
            sb.AppendLine($"         Avg match game time: {totalDuration / filesRead:F0}s");

            if (actionsPerMinute < 3)
                sb.AppendLine($"         ⚠ Very low action rate — AI may be idle too often. Check rod activation.");
            if (actionsPerMinute > 50)
                sb.AppendLine($"         ⚠ Extremely high action rate — possible spam/loop. Check for stuck patterns.");
        }
        else
        {
            sb.AppendLine($"         No timing data available from analysis files.");
        }
        sb.AppendLine();
    }

    private static void WriteBallHealthAssessment(StringBuilder sb, List<AutoMatchRunner.MatchResult> results)
    {
        float avgRestarts = (float)results.Average(r => r.ballRestarts);
        float avgDeadTime = (float)results.Average(r => r.deadBallTime);
        float totalRestarts = results.Sum(r => r.ballRestarts);
        int matchesWithRestarts = results.Count(r => r.ballRestarts > 0);
        float avgBumps = (float)results.Average(r => r.bumpCount);
        float totalBumps = results.Sum(r => r.bumpCount);

        string healthIcon = avgRestarts < 1 ? "PASS ✓" : (avgRestarts < 3 ? "WARN !" : "FAIL ✗");
        sb.AppendLine($"  [{healthIcon}] BALL HEALTH (respawns, bumps, and dead time)");
        sb.AppendLine($"         Avg respawns/match: {avgRestarts:F1} (total: {totalRestarts})");
        sb.AppendLine($"         Avg bumps/match:    {avgBumps:F1} (total: {totalBumps})");
        sb.AppendLine($"         Matches with respawns: {matchesWithRestarts}/{results.Count}");
        sb.AppendLine($"         Avg dead ball time/match: {avgDeadTime:F1}s");

        if (avgRestarts >= 3)
            sb.AppendLine($"         ⚠ HIGH respawn rate — ball gets stuck often, game feels broken and boring");
        else if (avgRestarts >= 1)
            sb.AppendLine($"         ⚠ Some ball inactivity — consider faster ball physics or larger minimum velocity");
        if (avgDeadTime > 30)
            sb.AppendLine($"         ⚠ Significant dead ball time — player will get bored waiting for ball");
        sb.AppendLine();
    }

    private static void WritePerDifficultyBreakdown(StringBuilder sb, List<AutoMatchRunner.MatchResult> results)
    {
        sb.AppendLine("┌──────────────────────────────────────────────────────────────────────────┐");
        sb.AppendLine("│                    PER-DIFFICULTY BREAKDOWN                              │");
        sb.AppendLine("└──────────────────────────────────────────────────────────────────────────┘");
        sb.AppendLine();

        // Group by difficulty combo
        var combos = results.GroupBy(r => $"{DiffName(r.leftDifficulty)} vs {DiffName(r.rightDifficulty)}");

        foreach (var combo in combos)
        {
            var matches = combo.ToList();
            int count = matches.Count;
            int leftWins = matches.Count(r => r.winner == "Left");
            int rightWins = matches.Count(r => r.winner == "Right");
            float avgGoals = (float)matches.Average(r => r.leftScore + r.rightScore);
            float avgLeft = (float)matches.Average(r => r.leftScore);
            float avgRight = (float)matches.Average(r => r.rightScore);
            int knockouts = matches.Count(r => r.knockout);

            sb.AppendLine($"  {combo.Key} ({count} matches):");
            sb.AppendLine($"    Wins: Left {leftWins} — Right {rightWins} — Draw {count - leftWins - rightWins}");
            sb.AppendLine($"    Avg goals: {avgGoals:F1} (L:{avgLeft:F1} R:{avgRight:F1}) | Knockouts: {knockouts}");

            // Check if higher difficulty wins more (expected)
            int leftDiff = matches[0].leftDifficulty;
            int rightDiff = matches[0].rightDifficulty;
            if (leftDiff != rightDiff)
            {
                string stronger = leftDiff > rightDiff ? "Left" : "Right";
                int strongerWins = stronger == "Left" ? leftWins : rightWins;
                float strongerRate = (float)strongerWins / count;
                if (strongerRate < 0.4f)
                    sb.AppendLine($"    ⚠ Higher difficulty ({stronger}) wins only {strongerRate * 100:F0}% — difficulty scaling may be too weak");
                else
                    sb.AppendLine($"    ✓ Higher difficulty ({stronger}) wins {strongerRate * 100:F0}%");
            }
            sb.AppendLine();
        }
    }

    private static void WritePerPresetBreakdown(StringBuilder sb, List<AutoMatchRunner.MatchResult> results)
    {
        var presets = results.GroupBy(r => r.physicsPreset ?? "Default");
        if (presets.Count() <= 1 && presets.First().Key == "Default") return;

        sb.AppendLine("┌──────────────────────────────────────────────────────────────────────────┐");
        sb.AppendLine("│                   PER-PHYSICS PRESET BREAKDOWN                          │");
        sb.AppendLine("└──────────────────────────────────────────────────────────────────────────┘");
        sb.AppendLine();

        foreach (var preset in presets)
        {
            var matches = preset.ToList();
            int count = matches.Count;
            float avgGoals = (float)matches.Average(r => r.leftScore + r.rightScore);
            int knockouts = matches.Count(r => r.knockout);
            int scoreless = matches.Count(r => r.leftScore + r.rightScore == 0);
            float avgDiff = (float)matches.Average(r => Mathf.Abs(r.leftScore - r.rightScore));
            float avgRestarts = (float)matches.Average(r => r.ballRestarts);
            float avgDeadTime = (float)matches.Average(r => r.deadBallTime);

            sb.AppendLine($"  {preset.Key} ({count} matches):");
            sb.AppendLine($"    Avg goals: {avgGoals:F1} | Knockouts: {knockouts} | Scoreless: {scoreless} | Avg diff: {avgDiff:F1}");
            sb.AppendLine($"    Avg respawns: {avgRestarts:F1} | Avg dead ball time: {avgDeadTime:F1}s");

            if (avgGoals < MIN_GOALS_PER_MATCH)
                sb.AppendLine($"    ⚠ Low scoring — this preset may produce slow gameplay");
            if (avgGoals > MAX_GOALS_PER_MATCH)
                sb.AppendLine($"    ⚠ Excessive scoring — defense struggles with this preset");
            if (scoreless > 0)
                sb.AppendLine($"    ⚠ Scoreless matches — ball may get stuck or AI can't convert");
            sb.AppendLine();
        }
    }

    private static void WritePerFormationBreakdown(StringBuilder sb, List<AutoMatchRunner.MatchResult> results)
    {
        var formations = results.GroupBy(r => r.formationPreset ?? "Default");
        if (formations.Count() <= 1 && formations.First().Key == "Default") return;

        sb.AppendLine("┌──────────────────────────────────────────────────────────────────────────┐");
        sb.AppendLine("│                   PER-FORMATION PRESET BREAKDOWN                        │");
        sb.AppendLine("└──────────────────────────────────────────────────────────────────────────┘");
        sb.AppendLine();

        foreach (var formation in formations)
        {
            var matches = formation.ToList();
            int count = matches.Count;
            float avgGoals = (float)matches.Average(r => r.leftScore + r.rightScore);
            int knockouts = matches.Count(r => r.knockout);
            float avgDiff = (float)matches.Average(r => Mathf.Abs(r.leftScore - r.rightScore));
            float avgDeadTime = (float)matches.Average(r => r.deadBallTime);
            int leftWins = matches.Count(r => r.winner == "Left");
            int rightWins = matches.Count(r => r.winner == "Right");
            float leftWinRate = count > 0 ? (float)leftWins / count * 100f : 0f;
            float rightWinRate = count > 0 ? (float)rightWins / count * 100f : 0f;

            sb.AppendLine($"  {formation.Key} ({count} matches):");
            sb.AppendLine($"    Avg goals: {avgGoals:F1} | Knockouts: {knockouts} | Avg diff: {avgDiff:F1} | Avg dead ball: {avgDeadTime:F1}s");
            sb.AppendLine($"    Left wins: {leftWins} ({leftWinRate:F0}%) | Right wins: {rightWins} ({rightWinRate:F0}%)");

            if (Mathf.Abs(leftWinRate - rightWinRate) > 30f)
                sb.AppendLine($"    ⚠ Significant team balance issue with this formation");
            else
                sb.AppendLine($"    ✓ Reasonable team balance");
            sb.AppendLine();
        }
    }

    private static void WritePerMatchAnalysisSummaries(StringBuilder sb, List<AutoMatchRunner.MatchResult> results)
    {
        sb.AppendLine("┌─────────────────────────────────────────────────────────────┐");
        sb.AppendLine("│              PER-MATCH ANALYSIS FILE PATHS                  │");
        sb.AppendLine("└─────────────────────────────────────────────────────────────┘");
        sb.AppendLine();

        foreach (var r in results)
        {
            string analysisPath = !string.IsNullOrEmpty(r.logFilePath)
                ? r.logFilePath.Replace(".txt", "_analysis.txt")
                : "(not captured)";

            bool exists = !string.IsNullOrEmpty(r.logFilePath) && File.Exists(analysisPath);
            string status = exists ? "✓" : "✗";

            sb.AppendLine($"  Match {r.matchNumber}: [{status}] {analysisPath}");
        }
        sb.AppendLine();
    }

    private static void WriteRecommendations(StringBuilder sb, List<AutoMatchRunner.MatchResult> results)
    {
        sb.AppendLine("┌─────────────────────────────────────────────────────────────┐");
        sb.AppendLine("│                     RECOMMENDATIONS                         │");
        sb.AppendLine("└─────────────────────────────────────────────────────────────┘");
        sb.AppendLine();

        float avgGoals = (float)results.Average(r => r.leftScore + r.rightScore);
        float leftWinRate = (float)results.Count(r => r.winner == "Left") / results.Count;
        float knockoutRate = (float)results.Count(r => r.knockout) / results.Count;
        int scorelessMatches = results.Count(r => r.leftScore + r.rightScore == 0);

        bool hasIssues = false;

        if (avgGoals < MIN_GOALS_PER_MATCH)
        {
            hasIssues = true;
            sb.AppendLine("  → INCREASE SCORING: Avg goals too low.");
            sb.AppendLine("    - Lower minShootScore threshold so AI shoots more often");
            sb.AppendLine("    - Increase shootProbability");
            sb.AppendLine("    - Check shootableDistanceThreshold — AI may never get close enough");
            sb.AppendLine();
        }

        if (avgGoals > MAX_GOALS_PER_MATCH)
        {
            hasIssues = true;
            sb.AppendLine("  → STRENGTHEN DEFENSE: Too many goals per match.");
            sb.AppendLine("    - Improve GK positioning and intercept logic");
            sb.AppendLine("    - Check defensive rod activation — should prioritize when ball in own half");
            sb.AppendLine();
        }

        if (leftWinRate < BALANCE_WIN_RATE_MIN || leftWinRate > BALANCE_WIN_RATE_MAX)
        {
            hasIssues = true;
            string dominant = leftWinRate > 0.5f ? "Left" : "Right";
            sb.AppendLine($"  → FIX BALANCE: {dominant} team wins too often ({leftWinRate * 100:F0}% left win rate).");
            sb.AppendLine("    - Check if there's a positional advantage (e.g., attack rod alignment)");
            sb.AppendLine("    - Verify ball spawn position is truly centered");
            sb.AppendLine("    - Check AI rod configurations are symmetric");
            sb.AppendLine();
        }

        if (knockoutRate > 0.6f)
        {
            hasIssues = true;
            sb.AppendLine($"  → SLOW DOWN SCORING: {knockoutRate * 100:F0}% of matches end in knockout.");
            sb.AppendLine("    - Matches should usually go the full duration for tension");
            sb.AppendLine("    - Strengthen defensive AI or reduce shot power");
            sb.AppendLine();
        }

        if (scorelessMatches > 0)
        {
            hasIssues = true;
            sb.AppendLine($"  → FIX SCORING: {scorelessMatches} match(es) had zero goals.");
            sb.AppendLine("    - AI may not be getting into shooting positions");
            sb.AppendLine("    - Check if ball gets stuck or AI gets into dead-lock patterns");
            sb.AppendLine();
        }

        if (!hasIssues)
        {
            sb.AppendLine("  ✓ No critical issues detected. Gameplay metrics look healthy!");
            sb.AppendLine("    Review individual match analysis files for fine-tuning opportunities.");
            sb.AppendLine();
        }
    }

    #endregion

    #region Parsing Helpers

    private static int ExtractInt(string content, string prefix)
    {
        int idx = content.IndexOf(prefix);
        if (idx < 0) return 0;

        int startIdx = idx + prefix.Length;
        // Skip whitespace
        while (startIdx < content.Length && content[startIdx] == ' ') startIdx++;

        int endIdx = startIdx;
        while (endIdx < content.Length && char.IsDigit(content[endIdx])) endIdx++;

        if (endIdx > startIdx && int.TryParse(content.Substring(startIdx, endIdx - startIdx), out int value))
            return value;
        return 0;
    }

    private static float ExtractFloat(string content, string prefix)
    {
        int idx = content.IndexOf(prefix);
        if (idx < 0) return 0;

        int startIdx = idx + prefix.Length;
        while (startIdx < content.Length && content[startIdx] == ' ') startIdx++;

        int endIdx = startIdx;
        while (endIdx < content.Length && (char.IsDigit(content[endIdx]) || content[endIdx] == '.')) endIdx++;

        if (endIdx > startIdx && float.TryParse(content.Substring(startIdx, endIdx - startIdx),
            System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float value))
            return value;
        return 0;
    }

    #endregion

    #region Utility

    private static string DiffName(int level) => level switch
    {
        1 => "Easy",
        2 => "Med",
        3 => "Hard",
        _ => $"L{level}"
    };

    #endregion
}

# AI Iteration 6 — Rod Bump System, Logging Overhaul & 270-Match Analysis
## Futbolito — Foosball AI

### Date: 2026-03-14
### Goal: Implement working rod bump mechanic, fix AI logging pipeline, establish tiered ball recovery, analyze 270-match test suite
### Related: [AI Iteration 5](AI_Iteration_5.md), [Game Design Reference](../MechanicsGameplay/Game_Design_Reference.md)

---

## What Was Changed This Iteration

### 1. Rod Bump Mechanic — Full Implementation (NEW)
**Problem:** `BumpNudge` movement mode existed in `AIRodMovement.cs` but was dead code — `SetMovementMode(BumpNudge)` was never called. The old `RodBumpEffect.cs` had been deleted. Ball stuck situations had no resolution mechanism.
**Solution:** Created new `RodBumpEffect.cs` with passive stuck-ball detection:
- Detects ball velocity < 1.0 for 1.5s threshold → triggers `BumpNudge` on closest rod
- Static `allBumpEffects` array ensures only the closest rod to the ball bumps (prevents multi-rod chaos)
- 2s cooldown between bumps, `ballHasBeenKickedOff` flag prevents post-goal-spawn bumps
- No figure-proximity requirement — any rod can bump regardless of figure position
- `TotalBumpCount` static counter shared with `PlayerRodBumpAction` for reporting
- Auto-added to AI rods via `TeamRodsController.AddAIComponents()`

### 2. Player Rod Bump Input (NEW)
**Problem:** Only AI could bump. Player needed the same mechanic for stuck situations.
**Solution:** Created `PlayerRodBumpAction.cs`:
- Input: **R key** (keyboard) or **Y button** (gamepad)
- 1s cooldown, applies same BumpNudge physics
- Added "Bump" action to `FutbolitoActions.inputactions`
- Auto-added via `TeamRodsController.AddPlayerComponents()`

### 3. Tiered Ball Recovery System
**Problem:** After disabling `autoRespawnWhenInactive`, some corner/dead-zone situations couldn't be resolved by bump alone (ball beyond figure reach).
**Solution:** Three-tier escalation in `BallBehavior`:
| Tier | Trigger | Action |
|------|---------|--------|
| 1 (1.5s) | Ball velocity < 1.0 | `RodBumpEffect` fires on closest AI rod |
| 2 (5.0s) | Ball still inactive | Emergency nudge toward center (random force) |
| 3 (10.0s) | Ball still inactive | Last-resort respawn at center |

### 4. AI Logging Overhaul
**Problem:** `AIDebugLogger.EnableAllDebugFlags()` used reflection to set `showDebugInfo=true` on ALL AI components at runtime, causing thousands of `Debug.Log` calls per second. Unity console became unusable after ~3 matches.
**Solution:**
- **Removed entirely:** `EnableAllDebugFlags()`, `RestoreDebugFlags()`, `OnUnityLogMessage()`, `originalDebugFlags` dictionary
- **Replaced ALL Debug.Log** in AI hot-path scripts with `AIDebugLogger.Log(rodName, actionType, message)`:
  - `AIRodShootAction.cs` — all Debug.Log → AIDebugLogger.Log
  - `AIRodMagnetAction.cs` — all Debug.Log → AIDebugLogger.Log
  - `AIRodWallPassAction.cs` — all Debug.Log → AIDebugLogger.Log
  - `PositioningState.cs` — all Debug.Log → AIDebugLogger.Log
  - `AIRodStateMachine.cs` — all Debug.Log → AIDebugLogger.Log
  - `PhysicsTelemetryLogger.cs` — all Debug.Log → AIDebugLogger.Log
  - `AIGoalEvaluator.cs` — guarded init logs, replaced showShootingScores Debug.Log
- **Guarded remaining Debug.Log** in `TeamRodsController`, `RodConfiguration`, `AITeamRodsController`, `PhysicsPresetManager` with `!AutoMatchRunner.IsAutoMode`
- `AIDebugLogger.Log()` writes exclusively to file (`{persistentDataPath}/ai_logs/ai_log_{timestamp}.txt`), only mirrors to console if `mirrorToConsole=true` AND `!IsAutoMode`

### 5. TimeMatchController Rewrite
**Problem:** Timer didn't work with AutoMatchRunner — it was disabled in scene, depended on `ballInGame` flag, and had a `matchTime * 59` bug.
**Solution:**
- New `ResetTimer()` method reads from: AutoMatchRunner slider → MatchInfo → fallback 5 min
- `timerActive` flag replaces `ballInGame` dependency — countdown starts immediately after `ResetTimer()`
- Fixed `matchTime * 59` → `matchTime * 60`
- `MatchController.InitAnimation()` enables the component and calls `ResetTimer()`
- Null-safe Animator access for headless testing scenes

---

## 270-Match Test Suite Results (Run 2026-03-13)

### Configuration
- **Matches:** 270 (269 valid — match #189 excluded, computer was paused/slept)
- **Timer:** 1 min per match
- **Rotation:** 6 difficulties × 6 physics × 5 formations
- **Run ID:** `auto_test_2026-03-13_21-29-10`

### Overall Summary (269 valid matches)

| Metric | Value | Assessment |
|--------|-------|------------|
| Avg goals/match | 4.9 | ✅ Active gameplay |
| Avg duration | 60.2s | ✅ Within 1-min timer |
| Ball respawns | 0 | ✅ Bump system handles it |
| Avg bumps/match | 28.3 (7,607 total) | ✅ System is active |
| Knockout rate | 94/269 (35%) | ✅ Healthy KO mix |
| Scoreless matches | 16/269 (6%) | ⚠️ Investigate |

### Team Balance — MAJOR IMPROVEMENT

| Metric | Run 7 (prev) | Run 8 (current) | Change |
|--------|-------------|-----------------|--------|
| Left wins | 30% | 40% | +10% |
| Right wins | 70% | 42% | −28% |
| Draws | — | 19% | new |

**Right-team bias essentially eliminated.** Balance is now 40/42 with 19% draws.

### Winner Distribution
- **Left:** 107 (40%)
- **Right:** 113 (42%)
- **Draw:** 50 (19%)

### Difficulty Scaling — WORKING CORRECTLY

Higher difficulty consistently wins against lower:

| Matchup | Higher-diff wins | Expected |
|---------|-----------------|----------|
| HardvEasy | Hard 70% | ✅ |
| MedvEasy | Med 63% | ✅ |
| EasyvHard | Hard 67% | ✅ |
| EasyvMed | Med 63% | ✅ |
| HardvMed | Hard 45% | ⚠️ Close (expected) |

Same-difficulty matchups: EasyvEasy R:50%, MedvMed L:40%, HardvHard R:43% — slight right lean remains at same level.

### Physics Preset Ranking

| Preset | Avg Goals | KOs | Scoreless | Avg Bumps | Assessment |
|--------|-----------|-----|-----------|-----------|------------|
| **Arcade** | 5.9 | 26 | 1 | 26 | 🏆 Best overall |
| **Current Default** | 5.6 | 15 | 2 | 28 | ✅ Good |
| **Pinball Chaos** | 5.1 | 15 | 1 | 24 | ✅ Good |
| **Speed Demon** | 5.0 | 25 | 4 | 24 | ⚠️ High variance |
| **Competitive** | 4.0 | 10 | 8 | 37 | 🔴 Problematic |
| **Air Hockey** | 3.9 | 3 | 0 | 31 | ⚠️ Low scoring, no KOs |

### Formation Ranking

| Formation | Avg Goals | KOs | Scoreless | Avg Bumps |
|-----------|-----------|-----|-----------|-----------|
| Attacking 2-3-5 | 5.1 | 22 | 5 | 31 |
| Classic 4-4-2 | 5.1 | 17 | 2 | 24 |
| Defensive 5-3-2 | 5.1 | 20 | 2 | 26 |
| Balanced 3-4-3 | 4.9 | 15 | 1 | 30 |
| Wide 3-5-2 | 4.6 | 20 | 6 | 31 |

Formations are **surprisingly balanced** — all within 0.5 goals of each other. Wide 3-5-2 has most scoreless matches (6) and highest bumps (31).

### Anomalies (Duration > 70s)

| Match | Score | Difficulty | Physics | Formation | Duration | Bumps |
|-------|-------|-----------|---------|-----------|----------|-------|
| #266 | L3-R5 | MedvHard | Arcade | Attacking | 1054.9s | 9 |
| #233 | L2-R1 | HardvEasy | Pinball Chaos | Classic | 537.1s | 35 |
| #104 | L3-R3 | MedvHard | Arcade | Defensive | 98.5s | 23 |
| #70 | L0-R0 | MedvEasy | Current Default | Wide | 81.1s | 8 |

Match #266 (17.5 min) and #233 (9 min) need investigation — possible timer failure or edge case.

### Ball-in-Corner Problem (Player Observation)

The user observed during live play that ball frequently gets stuck in corners and AI cannot pull it out. Data confirms this:
- **16 scoreless matches** despite bumps firing constantly (up to 109 bumps in a single match)
- **Pattern:** High bumps + low goals = bumps fire but don't resolve the situation
- All 6 worst cases (>90 bumps, 0 goals) are on **Competitive** preset
- Rod bump pushes ball but it returns to the corner — needs directional intelligence

---

## Key Issues Identified

### 🔴 P0 — Ball Stuck in Corners
- **Symptom:** Ball trapped in corners/dead zones. Rod bumps fire (up to 109/match) but ball bounces back to same spot
- **Root cause:** Bump force direction is naive — it doesn't consider where the ball needs to go, just slams the nearest rod. Corner geometry traps the ball in a loop
- **Impact:** 16 scoreless matches, frustrating for human players
- **Proposed fix:** Corner-aware recovery — detect ball in corner zones and apply directed force toward center/playable area, or use the closest figure to actually position and kick the ball out

### 🟡 P1 — Competitive Preset Scoring Deficit
- **Symptom:** 4.0 avg goals (lowest), 8 scoreless (most), 37 avg bumps (highest)
- **Root cause:** Too much drag/friction causes ball to die frequently. AI can't build enough momentum
- **Proposed fix:** Reduce drag, increase shot forces, or consider removing this preset like Realistic/Heavy Metal

### 🟡 P1 — Air Hockey Low Knockout Rate
- **Symptom:** 3.9 avg goals, only 3 KOs in 45 matches (7%)
- **Root cause:** Likely low ball friction — ball slides past figures without being caught/shot
- **Proposed fix:** Tune magnet catch velocity or increase shot accuracy for this preset

### 🟡 P2 — Match Time Cap Missing
- **Symptom:** 2 matches ran 537s and 1055s — timer apparently didn't fire
- **Root cause:** Unknown — possibly TimeMatchController deactivated during those specific matches
- **Proposed fix:** Add hard time cap in AutoMatchRunner itself as safety net

### 🟢 P3 — Wall Pass Mechanic
- **Symptom:** Still ~0 successful wall passes across all testing
- **Status:** Known since Iteration 4, deferred to dedicated rework

### 🟢 P3 — AI Passivity vs Humans
- **Symptom:** Not yet tested — user plans to play against AI next session
- **Status:** Need human-vs-AI match data before tuning

---

## Files Modified This Iteration

| File | Change |
|------|--------|
| `RodBumpEffect.cs` | **NEW** — AI passive stuck detection, 1.5s threshold, 2s cooldown, closest-rod-only |
| `PlayerRodBumpAction.cs` | **NEW** — Player bump input (R / Y button), 1s cooldown |
| `BallBehavior.cs` | Tiered recovery (emergency nudge 5s, last-resort respawn 10s), autoRespawn=false |
| `AIDebugLogger.cs` | Removed EnableAllDebugFlags/RestoreDebugFlags/OnUnityLogMessage, guarded own Debug.Log |
| `AIRodShootAction.cs` | All Debug.Log → AIDebugLogger.Log |
| `AIRodMagnetAction.cs` | All Debug.Log → AIDebugLogger.Log |
| `AIRodWallPassAction.cs` | All Debug.Log → AIDebugLogger.Log |
| `PositioningState.cs` | All Debug.Log → AIDebugLogger.Log |
| `AIRodStateMachine.cs` | All Debug.Log → AIDebugLogger.Log |
| `PhysicsTelemetryLogger.cs` | Debug.Log → AIDebugLogger.Log |
| `AIGoalEvaluator.cs` | Guarded init logs, showShootingScores → AIDebugLogger.Log |
| `TeamRodsController.cs` | Auto-adds RodBumpEffect + PlayerRodBumpAction, guarded Debug.Log |
| `RodConfiguration.cs` | Guarded ~5 Debug.Log |
| `AITeamRodsController.cs` | Guarded 3 Debug.Log |
| `PhysicsPresetManager.cs` | Guarded 1 Debug.Log |
| `TimeMatchController.cs` | Rewrite: timerActive flag, ResetTimer(), no ballInGame dependency |
| `MatchController.cs` | InitAnimation enables timer + calls ResetTimer() |
| `AutoMatchRunner.cs` | bumpCount in MatchResult, RodBumpEffect.TotalBumpCount |
| `MultiMatchAnalyzer.cs` | Bump column and stats |
| `FutbolitoActions.inputactions` | Added "Bump" action |
| `Assembly-CSharp.csproj` | Added Compile Includes for new scripts |

---

## Priority List for Next Work

1. **P0: Corner ball recovery** — Smart corner detection + directed force to get ball into play
2. **P1: Competitive preset tuning** — Reduce drag/friction or remove preset
3. **P1: Air Hockey KO rate** — Tune for more decisive matches
4. **P2: Match time safety cap** — Hard limit in AutoMatchRunner
5. **P3: Wall pass rework** — Dedicated mechanic overhaul
6. **P3: Human-vs-AI testing** — Gather player feedback on difficulty, fun factor

---

## How to Test

### Step 1: Run AI-vs-AI Test Suite
1. Open `GameMatchTesting_Scene`
2. Select AutoMatchRunner, set match count and timer
3. Enable Rotate Formations + Rotate Physics
4. Hit Play — results saved to `{persistentDataPath}/ai_logs/auto_test_*/`

### Step 2: Player Bump Testing
1. Open `GameMatch_Scene`, start a match
2. When ball is stuck, press **R** (keyboard) or **Y** (gamepad) to bump
3. Observe: bump should nudge the ball; 1s cooldown between bumps

### Step 3: Verify Logging
1. Run 2-3 auto matches
2. Check Unity console — should see **zero** AI debug logs
3. Check `ai_logs/ai_log_*.txt` — should have detailed per-rod action logs

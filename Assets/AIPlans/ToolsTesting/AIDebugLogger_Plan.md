# AI Debug Logger — Diagnostic Tool

## Session: 2026-02-14
## Status: In Progress
## Related: [AI Iteration 1](../AI/AI_Iteration_1.md)

---

## Problem Statement

After implementing AI Iteration 1, the AI gets stuck in a loop between magnet (attracting the ball) and wall pass, but never shoots. We need a diagnostic tool to:
1. Capture structured AI decision logs during gameplay
2. Write logs to a file that can be analyzed outside Unity Editor
3. Detect stuck loops and decision failures automatically
4. Identify exactly WHY the AI isn't shooting

## Current Debug Infrastructure

- All AI components have `showDebugInfo` flags (default: `false`)
- Logs go to Unity Console only via `Debug.Log`
- No file-based logging, no `Application.logMessageReceived` hooks
- Log prefixes: `[AIRodShootAction]`, `[AIRodMagnetAction]`, `[AIRodWallPassAction]`, `[AITeamRodsController]`, `[rodName]`

---

## Solution: Two New Files

### 1. `Assets/Scripts/Debug/AIDebugLogger.cs` — Singleton MonoBehaviour

**Responsibilities:**
- Subscribe to `Application.logMessageReceived` to capture all AI-prefixed messages
- Write structured log entries to `Application.persistentDataPath/ai_logs/`
- Auto-enable `showDebugInfo` on all AI components when logger is active
- Restore original debug flag values when logger is disabled
- Provide Inspector toggle for on/off

**Log Entry Format:**
```
[Frame] [Timestamp] [RodName] [ActionType] Message
```

Example:
```
[1234] [00:05.231] DefenseRod SHOOT_EVAL Score 0.35 below threshold 0.40 (distance: 0.8, ballInFront: true)
[1234] [00:05.231] DefenseRod MAGNET_ON Ball slow (1.2), in range (0.9), not shootable
[1298] [00:05.512] DefenseRod MAGNET_OFF Opponent has possession
[1350] [00:05.732] DefenseRod WALLPASS Proactive: 3 cycles without shot
[1410] [00:05.952] DefenseRod MAGNET_ON Ball slow (0.8), in range (1.1), not shootable
...pattern repeats = STUCK LOOP
```

### 2. `Assets/Scripts/Debug/AILogAnalyzer.cs` — Static Utility

**Responsibilities:**
- Parse log file and produce analysis summary
- **Action frequency histogram**: shoots vs magnets vs wall passes per rod
- **Stuck-loop detection**: flag when same action sequence repeats >N times
- **Shoot failure reasons**: breakdown of WHY shots weren't taken
- **Possession timeline**: AI/Opponent/Free state over time
- Output summary to `_analysis.txt` alongside the log file

---

## Implementation Workplan

### Phase 1: Create AIDebugLogger
- [x] Create `Assets/Scripts/Debug/` directory
- [x] Create `AIDebugLogger.cs` singleton MonoBehaviour
  - [x] `Application.logMessageReceived` hook with AI prefix filter
  - [x] `StreamWriter` file output to `persistentDataPath/ai_logs/`
  - [x] Frame number + timestamp per entry
  - [x] Inspector toggle `[SerializeField] bool enableLogging`
  - [x] Auto-enable all `showDebugInfo` flags on Start

### Phase 2: Add structured action events
- [x] Add always-on structured log calls (not gated by `showDebugInfo`) at key decision points:
  - [x] `AIRodShootAction.EvaluateShoot()` — log every evaluation with result + reason
  - [x] `AIRodShootAction.FindBestFigureForShoot()` — log per-figure rejection reasons
  - [x] `AIRodMagnetAction.EvaluateAndUpdateMagnet()` — log activate/deactivate with reason
  - [x] `AIRodWallPassAction.EvaluateAndExecuteWallPass()` — log execute/reject reason
  - [x] `PositioningState.EvaluateActions()` — log action evaluation result
  - [x] `AITeamRodsController.UpdateBallPossession()` — log possession changes

### Phase 3: Create AILogAnalyzer
- [x] Create `AILogAnalyzer.cs` static utility
  - [x] Parse log entries into structured data
  - [x] Action frequency histogram per rod
  - [x] Stuck-loop detector (sliding window pattern matching)
  - [x] Shoot failure reason aggregation
  - [x] Write `_analysis.txt` summary

### Phase 4: Integration
- [x] Add `[ContextMenu("Dump AI Log Summary")]` to AIDebugLogger
- [x] Auto-save log + analysis when match ends (hook into match end event)

---

## Files Created
| File | Purpose |
|------|---------|
| `Assets/Scripts/Debug/AIDebugLogger.cs` | Main logger — captures, filters, writes structured logs |
| `Assets/Scripts/Debug/AILogAnalyzer.cs` | Log parser — histogram, stuck-loop detection, summary |

## Files Modified (minimal instrumentation)
| File | Change |
|------|--------|
| `AIRodShootAction.cs` | Add structured log at EvaluateShoot + FindBestFigureForShoot |
| `AIRodMagnetAction.cs` | Add structured log at activate/deactivate |
| `AIRodWallPassAction.cs` | Add structured log at execute |
| `PositioningState.cs` | Add structured log at action evaluation |
| `AITeamRodsController.cs` | Add structured log at possession/config changes |

## Log File Location
`C:\Users\jjgal\AppData\LocalLow\{CompanyName}\Futbolito\ai_logs\ai_log_YYYY-MM-DD_HH-mm-ss.txt`

---

## Changes Log
| Date | Change |
|------|--------|
| 2026-02-14 | Plan created |
| 2026-02-14 | Implementation complete — AIDebugLogger.cs + AILogAnalyzer.cs created, 5 AI components instrumented |

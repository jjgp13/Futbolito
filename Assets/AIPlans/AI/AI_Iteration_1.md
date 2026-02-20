# AI Behavior Redesign — Iteration 1: Fundamentals
## Futbolito — Foosball AI

### Date: 2026-02-14
### Goal: Make AI behave like a real human foosball player

---

## Problem Statement

The current AI doesn't feel like playing against a human. The main issues are:
1. **Rod activation is wrong** — rods activate based on static field zones, not based on game context (who has the ball, attacking vs defending)
2. **AI blocks its own shots** — no awareness that a friendly rod in front of the ball will block the shot
3. **AI rarely shoots** — shooting requires ball within 1.0 unit + high shot quality score, making it too picky
4. **No passing between rods** — AI never intentionally passes from defense → mid → attacker
5. **No awareness of opponent positions** — doesn't look for open space or gaps between opponent figures
6. **Wall pass only used as last resort** — only triggered when 2+ blockers exist
7. **Magnet has no tactical purpose** — just attracts ball without clear intent

---

## Core Design Principles (This Iteration)

### Principle 1: Two-Hand Simulation
AI controls exactly 2 rods at a time, simulating a human with 2 hands.

### Principle 2: Context-Aware Rod Selection
Which 2 rods are active depends on WHO HAS THE BALL:
- **Defending** (opponent has ball): activate rods BEHIND the ball (between ball and AI goal) to block passing lanes and shots
- **Attacking** (AI has ball): activate the rod WITH the ball + the next rod FORWARD toward opponent goal, ensuring the forward rod doesn't self-block

### Principle 3: Shots Are Not Just For Scoring
Shooting is the primary way to move the ball forward. AI should shoot frequently to:
- Pass from defense → mid
- Pass from mid → attacker
- Score from any rod with a clear path

### Principle 4: Space Awareness
AI must be aware of opponent rod positions and find gaps/open space to pass or shoot through.

### Principle 5: Action Intent
Each action has a clear tactical purpose:
- **Shoot**: pass forward or score
- **Magnet**: attract ball to figure center for better shot positioning
- **Wall pass**: move ball between figures on the SAME rod to confuse opponent and find better angles

---

## Current Architecture (What Exists)

```
AITeamRodsController (parent)
  ├── Manages which 2 rods are active
  ├── Difficulty presets (Easy/Medium/Hard)
  └── Pushes settings to child rods

Each Rod has:
  ├── AIRodStateMachine (FSM: Idle → Positioning → Shooting → Cooldown)
  ├── AIRodMovementAction (5 movement modes)
  ├── AIRodShootAction (charge-based, parallel with positioning)
  ├── AIRodMagnetAction (continuous, every frame)
  ├── AIRodWallPassAction (conditional, when shots blocked)
  └── AIGoalEvaluator (raycasting for shot quality)

Rod indices: [0]=GoalKepperRod, [1]=DefenseRod, [2]=MidfieldRod, [3]=AttackerRod
AI attacks: right-to-left (RightTeam) or left-to-right (LeftTeam)
```

---

## Change 0: Architecture Cleanup — Remove Dead Code

### Problem
The codebase went through a refactoring from a pure FSM to a hybrid FSM+Components
architecture, but the old code was never cleaned up. The result:

**The FSM is mostly dead weight.** Only 4 states are registered (Idle, Positioning,
Shooting, Cooldown), but:
- `PositioningState` does 80% of the work — it's the only real "active" state
- `ShootingState` is a 1-frame pass-through (just locks movement during animation)
- `CooldownState` is a 0.5s timer (could be a simple field in the component)
- Actions (Shoot, Magnet, WallPass) run as independent components in `Update()`,
  NOT through FSM states

**5 state files exist but are NEVER registered or used:**
- `ChargingShotState.cs` — replaced by `AIRodShootAction` component
- `MagnetState.cs` — replaced by `AIRodMagnetAction` component
- `WallPassState.cs` — replaced by `AIRodWallPassAction` component
- `EvaluatingActionState.cs` — merged into `PositioningState`
- `TrackingState.cs` — merged into `PositioningState`

**22 old documentation files** in `Assets/Scripts/Rods/FSM/` reference the old
architecture and are now misleading.

### What To Delete

**Dead state files (5 files + 5 .meta files):**
- [x] `Assets/Scripts/Rods/FSM/States/ChargingShotState.cs` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/States/MagnetState.cs` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/States/WallPassState.cs` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/States/EvaluatingActionState.cs` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/States/TrackingState.cs` (+.meta)

**Old documentation (22 files + 22 .meta files):**
- [x] `Assets/Scripts/Rods/COMPONENT_MANAGEMENT_IMPROVEMENT.md` (+.meta)
- [x] `Assets/Scripts/Rods/BUGFIX_ATTRACTION_FORCE_AND_MULTI_FIGURE.md` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/VISUAL_ARCHITECTURE.md` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/README.md` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/MIGRATION_GUIDE.md` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/GOAL_ORIENTED_IMPLEMENTATION_SUMMARY.md` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/GOAL_ORIENTED_AI_QUICK_REF.md` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/GOAL_ORIENTED_AI_GUIDE.md` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/GOAL_ORIENTED_AI_CHANGELOG.md` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/FSM_QUICK_REFERENCE.md` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/FSM_IMPLEMENTATION_GUIDE.md` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/DIFFICULTY_QUICK_REFERENCE.md` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/COMPLETE_SYSTEM_SUMMARY.md` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/CHANGE_SUMMARY.md` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/CENTRALIZED_DIFFICULTY_SUMMARY.md` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/CENTRALIZED_DIFFICULTY_GUIDE.md` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/ARCHITECTURE_REFACTORING.md` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/AI_TESTING_GUIDE.md` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/AI_SHOOTING_TROUBLESHOOTING_CHECKLIST.md` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/AI_GOAL_EVALUATOR_DEBUG_GUIDE.md` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/AI_ENHANCEMENT_IMPLEMENTATION.md` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/AITeamRodsController_Integration_Guide.md` (+.meta)
- [x] `Assets/Scripts/Rods/FSM/ACTION_REFACTORING_SUMMARY.md` (+.meta)

### What To Keep & Simplify

**Keep these files (they contain alive code):**
- `AIRodStateMachine.cs` — Simplify: keep as a thin coordinator (Idle/Active toggle + settings hub)
- `AIRodState.cs` — Base class, still used by remaining states
- `IdleState.cs` — Rod inactive state
- `PositioningState.cs` — Main active state (will be rewritten in later changes)
- `ShootingState.cs` — Keep for now (animation lock), may merge into component later
- `CooldownState.cs` — Keep for now (rate limiting), may merge into component later
- `AIGoalEvaluator.cs` — Shot quality evaluation, will be extended
- `AITeamRodsControllerEditor.cs` — Custom inspector, keep

### Architecture After Cleanup

```
AITeamRodsController (parent — brain)
  ├── Ball possession detection (NEW)
  ├── Context-aware rod activation (REWRITTEN)
  ├── Difficulty presets
  └── Coordinates rod behavior

Each Rod:
  ├── AIRodStateMachine (simplified: Idle ↔ Active toggle + settings)
  ├── AIRodMovementAction (movement modes, runs in FixedUpdate)
  ├── AIRodShootAction (charge & shoot, runs in Update — independent)
  ├── AIRodMagnetAction (ball attraction, runs in Update — independent)
  ├── AIRodWallPassAction (wall passes, triggered by PositioningState)
  └── AIGoalEvaluator (raycasting, gap finding — extended)
```

---

## Implementation Plan

### Change 1: Ball Possession Detection
**Problem**: AI doesn't know who has the ball.
**Solution**: Add ball possession tracking to `AITeamRodsController`.

**Logic**:
- Ball is "possessed by AI" if: ball velocity is low (<2 m/s) AND ball is within reach of any AI rod figure
- Ball is "possessed by opponent" if: ball velocity is low AND ball is within reach of any opponent rod figure
- Ball is "contested/free" if: ball is moving fast OR not near any figure

**Files to modify**:
- [x] `AITeamRodsController.cs` — Add `BallPossession` enum and detection method

**New fields/methods**:
```csharp
public enum BallPossession { AI, Opponent, Free }
public BallPossession CurrentPossession { get; private set; }

private void UpdateBallPossession()
{
    // Check ball velocity
    // Check proximity to AI figures vs opponent figures
    // Set CurrentPossession
}
```

---

### Change 2: Context-Aware Rod Activation
**Problem**: Current system uses static field zones (defensive/balanced/attacking thirds).
**Solution**: Rewrite rod activation to consider ball possession and ball position relative to rods.

**New activation rules**:

**When DEFENDING (opponent has ball or ball is free moving toward AI goal)**:
- Find the 2 AI rods that are BEHIND the ball (between ball and AI's own goal)
- These rods should block passing lanes and intercept
- If only 1 rod is behind the ball (ball near GK), activate only GK
- If ball is past all rods (behind GK), activate GK only

**When ATTACKING (AI has ball)**:
- Activate the rod that HAS the ball (ball closest to this rod's figures)
- Activate the NEXT rod forward (toward opponent goal) — this rod should position to RECEIVE a pass
- **Self-blocking prevention**: the forward rod should move its figures OUT of the direct shooting lane of the rod with the ball
- If ball is on attacker rod (frontmost), activate attacker + midfield (mid provides backup)

**Edge cases**:
- Ball exactly between two rods: activate both
- Ball behind goalkeeper: only goalkeeper active
- Transition: when possession changes, rod activation should update within 1 frame

**Files to modify**:
- [x] `AITeamRodsController.cs` — Rewrite `DetermineRodConfiguration()` and `UpdateRodActivation()`

---

### Change 3: Self-Blocking Prevention
**Problem**: AI's own rods block its own shots when shooting forward.
**Solution**: When AI is attacking, the forward rod should clear the shooting lane.

**Logic**:
- When rod A is charging/shooting, check if rod B (forward) has figures in the shooting lane
- "Shooting lane" = horizontal band from ball Y position ± half figure height
- If rod B figures are in the lane, move rod B to clear the lane (offset Y position)
- This should be a movement mode in `AIRodMovementAction`: `ClearingLane`

**Files to modify**:
- [x] `AIRodMovementAction.cs` — Add `ClearingLane` movement mode
- [x] `AITeamRodsController.cs` — Coordinate lane clearing between active rods

---

### Change 4: Forward Passing (Shoot to Pass)
**Problem**: AI only shoots to score. Shooting threshold is too high (requires good shot quality).
**Solution**: AI should shoot to pass the ball forward to the next friendly rod.

**New shooting logic**:
- **Pass shot**: When the next forward rod has a figure in position to receive, shoot with light/medium force to pass
- **Score shot**: When there's a clear path to goal, shoot with full force
- Pass shots should have a MUCH LOWER quality threshold than score shots
- Pass shots use lighter charge (faster execution)

**Pass evaluation**:
- Check if next forward rod's figure is roughly aligned with ball Y position (±reasonable tolerance)
- Check if path between current figure and target figure is clear of opponent figures
- If clear → light shot to pass forward

**Files to modify**:
- [x] `AIRodShootAction.cs` — Add `EvaluatePass()` method alongside `EvaluateShoot()`
- [x] `AIGoalEvaluator.cs` — Add `EvaluatePassToRod()` that checks path clearance to friendly rod
- [x] `PositioningState.cs` — Update action evaluation to consider passing

---

### Change 5: Opponent Awareness & Space Finding
**Problem**: AI doesn't look for gaps between opponent figures.
**Solution**: AI should scan opponent rod positions and find open space to shoot/pass through.

**Logic**:
- For each opponent rod between AI ball-holder and the goal, analyze figure positions
- Find the GAPS between opponent figures (Y positions where no figure exists)
- Prefer shooting/passing through the widest gap
- This feeds into both shot direction and rod positioning (move to align with gap)

**Files to modify**:
- [x] `AIGoalEvaluator.cs` — Add `FindOpponentGaps()` method
- [x] `AIRodMovementAction.cs` — When in `AttackingPosition` mode, position figure to align with gaps
- [x] `AIRodShootAction.cs` — Use gap information to determine best shooting angle

---

### Change 6: Tactical Magnet Usage
**Problem**: Magnet just attracts ball with no clear intent.
**Solution**: Magnet should be used to control the ball and set up shots.

**New magnet logic**:
- Activate magnet when ball is NEAR a figure but NOT in ideal shooting position
- Once ball is attracted to figure center, IMMEDIATELY evaluate shoot/pass
- Magnet should have a clear purpose: "attract → position → shoot/pass"
- Deactivate magnet as soon as ball is in shootable position (current logic already does this partly)

**Files to modify**:
- [x] `AIRodMagnetAction.cs` — Tighten activation conditions, ensure quick transition to shooting

---

### Change 7: Tactical Wall Pass
**Problem**: Wall pass only used when 2+ blockers exist. Should be used proactively.
**Solution**: Wall pass should be used to reposition ball on the same rod to find better angles.

**New wall pass logic**:
- Use wall pass when current figure doesn't have a clear shot BUT another figure on the same rod would
- Use wall pass to confuse opponent: if opponent is tracking current figure, pass to another figure
- Wall pass should be evaluated BEFORE giving up on shooting, not after

**Files to modify**:
- [x] `AIRodWallPassAction.cs` — Add proactive wall pass evaluation
- [x] `AIGoalEvaluator.cs` — Add `WouldOtherFigureHaveBetterShot()` method
- [x] `PositioningState.cs` — Reorder action evaluation: Shoot → WallPass → Magnet (not Shoot → last-resort WallPass)

---

## Implementation Order

0. **Change 0: Architecture Cleanup** — Delete dead states, dead docs. Clean foundation before building
1. **Change 1: Ball Possession Detection** — Foundation for all other changes
2. **Change 2: Context-Aware Rod Activation** — Biggest impact on feel, uses possession
3. **Change 3: Self-Blocking Prevention** — Prevents frustrating AI behavior
4. **Change 4: Forward Passing** — Makes AI feel proactive and intelligent
5. **Change 5: Opponent Awareness** — Adds strategic depth
6. **Change 6: Tactical Magnet** — Polishes ball control flow
7. **Change 7: Tactical Wall Pass** — Adds variety and unpredictability

---

## Files Summary

| File | Changes |
|------|---------|
| **DELETED** (5 dead states) | `ChargingShotState.cs`, `MagnetState.cs`, `WallPassState.cs`, `EvaluatingActionState.cs`, `TrackingState.cs` |
| **DELETED** (22 old docs) | All `.md` files in `Rods/FSM/` and 2 in `Rods/` |
| `AITeamRodsController.cs` | Ball possession detection, rewrite rod activation, lane clearing coordination |
| `AIRodStateMachine.cs` | Simplify to thin coordinator (Idle/Active + settings hub) |
| `AIRodMovementAction.cs` | Add `ClearingLane` mode, gap-aligned positioning |
| `AIRodShootAction.cs` | Add pass evaluation, lower pass thresholds, use gap info |
| `AIRodMagnetAction.cs` | Tighten conditions, quick transition to shooting |
| `AIRodWallPassAction.cs` | Proactive wall pass, evaluate before giving up |
| `AIGoalEvaluator.cs` | Pass path evaluation, opponent gap finding, figure comparison |
| `PositioningState.cs` | Reorder action priority, integrate passing |

---

## Success Criteria

After this iteration, the AI should:
- [x] Control only 2 rods at a time, chosen intelligently based on game context
- [x] Never block its own shots with its own rods
- [x] Shoot frequently — both to pass forward and to score
- [x] Pass the ball forward from defense → mid → attacker
- [x] Find gaps between opponent figures to shoot/pass through
- [x] Use magnet to control ball and set up shots quickly
- [x] Use wall pass proactively to find better angles
- [x] Feel like playing against a real human foosball player

---

## Notes for Next Iteration
- Add difficulty scaling for all new behaviors (Easy AI makes mistakes, Hard AI is precise)
- Add goalkeeper specialization (already partially implemented)
- Add adaptive strategy based on score/time
- Add inter-rod coordination (signaling passing intent)
- Consider adding "personality" — some AIs prefer attacking, others defensive

---

## Post-Implementation Fixes (Debug Sessions 2026-02-14)

### Fix 1: Runtime Threshold Override
- **Problem**: Unity serialized Inspector values override code defaults — `shootableDistanceThreshold` stuck at 1.0
- **Fix**: Push `shootableDistanceThreshold` (2.0) and `possessionDistance` (2.5) at runtime via `ApplyDifficultySettingsToRods()` calling `SetShootableDistanceThreshold()` on each rod's AIRodShootAction and AIRodMagnetAction
- **Files**: `AITeamRodsController.cs`

### Fix 2: Ball-Behind Tolerance
- **Problem**: Ball barely behind figure (0.07 units) rejected by strict `ballInFront` check → 108 rejections
- **Fix**: Added `ballBehindTolerance = 0.5f` to both `FindBestFigureForShoot` and `ShouldInterruptCharge`
- **Files**: `AIRodShootAction.cs`

### Fix 3: Wall Pass + Magnet Speed Vicious Cycle
- **Problem**: Ball behind rod → can't shoot → wall pass kicks ball → speed 13+ → magnet blocked → repeat
- **Fix A**: `PositioningState.EvaluateActions()` — skip wall pass when `IsBallBehindRod()` is true
- **Fix B**: `AIRodMagnetAction.FindFigureForMagnet()` — 4x speed threshold when ball is behind any figure
- **Files**: `PositioningState.cs`, `AIRodMagnetAction.cs`

### Fix 4: Stuck Animation After Charge Interrupt
- **Problem**: `StopCharging()` stopped shootActions but never reset figure animation → `IsCharging` stayed true on animator
- **Fix**: Added `figures[i].TriggerKickAnimation(0f)` to `StopCharging()` to reset animation
- **Files**: `AIRodShootAction.cs`

### Fix 5: Charge-Restart Loop (GK)
- **Problem**: GK starts charging, DefensiveIntercept cancels, charge restarts at 0.00s — repeats forever, never completes
- **Fix**: Track consecutive charge restarts. After 3 restarts within 2s, enter 1.5s cooldown to let magnet stabilize the ball first
- **Files**: `AIRodShootAction.cs`

### Fix 6: Defense Rod Self-Blocks GK Shots
- **Problem**: When GK has possession, `DetermineAttackingConfiguration` pairs GK(0)+Defense(1), keeping Defense active. Defense figures physically block GK's shots toward opponent goal.
- **Fix**: Added `RodConfiguration.GoalkeeperOnly` enum value. When AI has possession and closest rod is GK, only GK is activated. `CombineConfigurationWithReachability` skips minimum-2-rod enforcement for this mode.
- **Files**: `AITeamRodsController.cs`

### Fix 7: GK Centers When Deactivated (Medium/Hard)
- **Problem**: GK stays at last position when deactivated, making it harder to react to incoming shots from either side of the goal.
- **Fix**: Added `CenteringIdle` movement mode. When GK is deactivated on Medium/Hard difficulty, it first centers (moves to Y=0) before going idle. Easy difficulty leaves GK where it is. Centering continues even after `isActive=false`.
- **Files**: `AIRodMovement.cs`, `AITeamRodsController.cs`

---

## Final State After Iteration 1

### Architecture (Post-Cleanup)
```
AITeamRodsController (brain)
  ├── Ball possession detection (BallPossession enum: AI/Opponent/Free)
  ├── Context-aware rod activation (4 configs: GoalkeeperOnly/Defensive/Balanced/Attacking)
  ├── Difficulty presets (Easy=1, Medium=2, Hard=3)
  ├── Runtime threshold push via ApplyDifficultySettingsToRods()
  └── GK centering on deactivation (Med/Hard)

Each Rod (4 total: [0]=GK, [1]=Defense, [2]=Midfield, [3]=Attack):
  ├── AIRodStateMachine (Idle ↔ Positioning → Shooting → Cooldown)
  ├── AIRodMovementAction (7 modes: Tracking, DefensiveBlocking, DefensiveCovering,
  │                         AttackingPosition, Intercepting, ClearingLane, CenteringIdle)
  ├── AIRodShootAction (charge-based, ballBehindTolerance=0.5, restart cooldown tracking)
  ├── AIRodMagnetAction (context-aware speed threshold, no magnet during charge/opponent possession)
  ├── AIRodWallPassAction (proactive after 3 cycles, skipped when ball behind rod)
  └── AIGoalEvaluator (raycasting + FindBestOpponentGap)
```

### Key Tuning Values (Runtime-Forced)
| Parameter | Easy (1) | Medium (2) | Hard (3) |
|-----------|----------|------------|----------|
| minimumShootScore | 0.50 | 0.40 | 0.30 |
| shootableDistanceThreshold | 2.50 | 2.00 | 2.00 |
| possessionDistance | 2.50 | 2.50 | 2.50 |
| ballBehindTolerance | 0.50 | 0.50 | 0.50 |
| GK minimumShootScore | 0.25 | 0.20 | 0.15 |
| GK targetChargeTime | 0.80s | 0.80s | 0.80s |

### Files Modified (Complete List)
| File | Status | What Changed |
|------|--------|--------------|
| `AITeamRodsController.cs` | Modified | Possession detection, rod configs (GoalkeeperOnly), difficulty presets, threshold push, GK centering trigger |
| `AIRodShootAction.cs` | Modified | Thresholds, tolerances, magnet-shoot decoupling, animation reset, charge restart cooldown |
| `AIRodMagnetAction.cs` | Modified | Context-aware speed threshold, possession/charging blocks |
| `AIRodMovement.cs` | Modified | ClearingLane mode, CenteringIdle mode, centering logic |
| `PositioningState.cs` | Modified | Ball-behind wall pass skip, proactive wall pass counter, IsBallBehindRod() |
| `AIRodWallPassAction.cs` | Modified | Structured logging |
| `AIGoalEvaluator.cs` | Modified | FindBestOpponentGap() |
| `AIDebugLogger.cs` | Created | Singleton structured log capture + file writer |
| `AILogAnalyzer.cs` | Created | Static analysis utility (histograms, stuck-loop detection) |
| 56 files | Deleted | 5 dead states + 22 old docs (+ all .meta files) |

### Performance Metrics (Match Logs)
| Metric | Before | After Iteration 1 |
|--------|--------|--------------------|
| Shoot rate (will_shoot/total_eval) | 0% | ~27% |
| Shots fired per match | 0 | 8-10 |
| GK shots fired | 0 | 8 |
| Magnet-wallpass stuck loops | Constant | None |
| Animation stuck bugs | Frequent | None |

---

## Key Learnings & Gotchas

1. **Unity Serialization Override**: `[SerializeField]` defaults are ONLY used when component is first added. Inspector values persist and override code changes. Always force values at runtime via setter methods.

2. **Physical Figure Spacing**: Minimum figure-to-ball distance during gameplay is ~1.65 units. All distance thresholds must be ≥ 2.0.

3. **Rod Name Typo**: GK rod is named `"GoalKepperRod"` (missing 'e') — hardcoded throughout, don't try to fix.

4. **Opponent Access**: `AITeamRodsController` uses `rods[]`, Human `TeamRodsController` uses `lines[]`. Use `opponentTeam.lines` to access opponent figures.

5. **Log-Driven Debugging**: The AIDebugLogger + AILogAnalyzer combo is essential. Always run a match → analyze logs → identify root cause → fix → repeat. Don't guess.

6. **Game Identity**: Company=j2Games, Product=Futbolito, AI logs at `AppData/LocalLow/j2Games/Futbolito/ai_logs/`

---

## Recommendations for Iteration 2

### Priority Improvements
1. **Goalkeeper Specialization** — GK should have unique behaviors:
   - Faster reaction to shots on goal
   - Smarter clearing (pass to defense, not random kick)
   - Positional awareness relative to goal posts
2. **Difficulty-Scaled Behaviors** — Easy AI should make mistakes (delayed reactions, missed shots, poor positioning). Current difficulty only scales thresholds, not behavior quality.
3. **Adaptive Strategy** — Adjust aggression based on score/time:
   - Losing → more aggressive, push forward
   - Winning → more defensive, hold possession
4. **Inter-Rod Coordination** — Active rods should coordinate:
   - When defense has ball, midfield should position to receive pass
   - Signal passing intent between rods
5. **AI Personality** — Different AI opponents could have different play styles:
   - Aggressive (always attacking, fast shots)
   - Defensive (holds position, waits for opponent mistakes)
   - Technical (lots of wall passes, precise shots)

### Known Issues to Monitor
- `evaluationCyclesWithoutShot` never triggers wall pass for GK (wall pass skipped when ball behind) — GK may need alternative fallback
- DefensiveIntercept priority override frequency on GK — may need per-rod tuning
- Long idle gaps (60s+) where rods have no ACTION_EVAL — expected when ball is far away

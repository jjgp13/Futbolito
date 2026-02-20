# AI Behavior Redesign — Iteration 2: Intelligence & Personality
## Futbolito — Foosball AI

### Date: 2026-02-15
### Goal: Make AI feel smarter, make difficulty feel different, and adapt to match context
### Branch: `feature/ai-improvements`

---

## Problem Statement

After Iteration 1, the AI has solid fundamentals: possession detection, context-aware rod activation, self-blocking prevention, forward passing, gap finding, and tactical magnet/wall pass. However:

1. **Difficulty feels like "same AI with looser thresholds"** — Easy, Medium, and Hard play identically, just with different numbers. Easy AI should make visible mistakes; Hard AI should feel relentless.
2. **Goalkeeper is generic** — GK uses the same logic as every other rod despite having a fundamentally different role (protect goal, clear ball to defense — NOT score).
3. **AI plays the same regardless of score/time** — Down 0-4? Still the same behavior. 30 seconds left and losing? No urgency. This makes the AI feel robotic, not competitive.

---

## Scope: 3 High-Priority Changes

| # | Change | Impact |
|---|--------|--------|
| 1 | Goalkeeper Specialization | GK plays its unique role — smarter saves, intentional clears |
| 2 | Difficulty-Scaled Behaviors | Easy AI makes real mistakes; Hard AI is precise and fast |
| 3 | Adaptive Strategy | AI adjusts aggression based on score differential and time remaining |

---

## Current Architecture (Post Iteration 1)

```
AITeamRodsController (brain)
  ├── BallPossession detection (AI/Opponent/Free)
  ├── Context-aware rod activation (GoalkeeperOnly/Defensive/Balanced/Attacking)
  ├── Difficulty presets (Easy=1, Medium=2, Hard=3) — threshold-only scaling
  ├── ApplyDifficultySettingsToRods() — runtime threshold push
  └── GK centering on deactivation (Med/Hard)

Each Rod [0]=GK, [1]=Def, [2]=Mid, [3]=Atk:
  ├── AIRodStateMachine (Idle ↔ Positioning → Shooting → Cooldown)
  ├── AIRodMovementAction (7 modes: Tracking, DefensiveBlocking, DefensiveCovering,
  │                         AttackingPosition, Intercepting, ClearingLane, CenteringIdle)
  ├── AIRodShootAction (charge-based, ballBehindTolerance=0.5, restart cooldown)
  ├── AIRodMagnetAction (context-aware, no magnet during charge/opponent possession)
  ├── AIRodWallPassAction (proactive after 3 cycles, skipped when ball behind)
  └── AIGoalEvaluator (raycasting + FindBestOpponentGap)

Match Controllers:
  ├── MatchController.instance — game state (ballInGame, endMatch, gameIsPaused)
  ├── MatchScoreController.instance — LeftTeamScore / RightTeamScore (goals, max 5)
  └── TimeMatchController — countdown timer (may be disabled in test scene)
```

### Key Tuning Values (Current — Iteration 1)

| Parameter | Easy (1) | Medium (2) | Hard (3) |
|-----------|----------|------------|----------|
| minimumShootScore | 0.50 | 0.40 | 0.30 |
| shootableDistanceThreshold | 2.50 | 2.00 | 2.00 |
| reactionDelay | 0.3s | 0.2s | 0.1s |
| chargeTimeMultiplier | 0.5x | 0.75x | 1.0x |
| attractionForce | -5 | -10 | -15 |
| maxMagnetDuration | 999s | 1.5s | 0.8s |
| GK minimumShootScore | 0.25 | 0.20 | 0.15 |

---

## Change 1: Goalkeeper Specialization

### Problem
GK uses the same shooting, magnet, and evaluation logic as field rods. But GK's job is fundamentally different:
- **Defend the goal** — react to incoming shots and block them
- **Clear the ball** — pass to defense rod, not shoot at opponent goal
- GK currently has minor tweaks (50% lower shoot score, 80% charge time) but no unique behaviors

### Solution: GK-Specific Behaviors (No New Animations)

#### 1A: Shot Reaction Positioning
**Goal**: GK should react to incoming ball trajectory, not just track ball Y position.

**Current**: GK uses `Intercepting` mode (ball prediction with `currentPos + velocity * 0.3s`).

**Improvement**: 
- When opponent has possession or ball is moving fast toward AI goal, GK should:
  - Use ball velocity vector to predict WHERE the ball will cross the GK's X position
  - Weight prediction more heavily when ball speed > threshold (committed trajectory)
  - Snap to predicted crossing Y, not current ball Y
- Add `shotThreatLevel` (0-1) based on ball speed + direction toward goal:
  - Low threat (slow/moving away) → standard DefensiveBlocking
  - High threat (fast + heading to goal) → Intercepting with tighter prediction

**Files to modify**:
- [x] `AIRodMovement.cs` — Add `GoalkeeperIntercept` movement mode with trajectory prediction weighted by threat level
- [x] `PositioningState.cs` — GK uses `GoalkeeperIntercept` when threat detected instead of generic Intercepting

#### 1B: Intentional Clearing to Defense
**Goal**: When GK has the ball, clear it toward defense rod figures, not randomly.

**Current**: GK shoots with 50% lower threshold, basically kicking in whatever direction.

**Improvement**:
- When GK has ball, find the defense rod's best figure position
- Aim the clear toward that figure's Y position
- Use light charge (quick clear) — don't hold the ball
- If no defense figure is well-positioned, use standard kick (still better than holding)

**Files to modify**:
- [x] `AIRodShootAction.cs` — Add GK clearing logic with `isGKClearing` flag, quicker charge time (60% of light threshold)
- [x] `AIGoalEvaluator.cs` — Add `FindBestClearingTarget()` that returns best defense figure Y position

#### 1C: Goal Post Awareness
**Goal**: GK should know where the goal posts are and position relative to them.

**Current**: GK movement is limited to half goal height but has no concept of "covering the near post" vs "covering the far post."

**Improvement**:
- When ball is on one side of the goal (Y > 0 or Y < 0), GK should bias toward that side
- GK should never leave the goal area undefended — clamp movement more tightly when under threat
- When ball is central, GK stays central

**Files to modify**:
- [x] `AIRodMovement.cs` — Goal-post-relative positioning bias integrated into `GoalkeeperIntercept` mode

### Implementation Notes (Completed 2026-02-17)

**GoalkeeperIntercept mode** (`AIRodMovement.cs`):
- Calculates `threatLevel` (0-1) using `Vector2.Dot(ballVelocity, ownGoalDirection) / 10f`
- When ball moves toward goal (speed > 1 m/s), predicts crossing Y at GK's X position
- `Mathf.Lerp(ballY, predictedY, threatLevel)` — blends tracking with prediction
- Goal post bias: adds `sign(ballY) * 0.3 * (1 - threatLevel)` for side coverage
- Movement clamped to `rodConfig.rodMovementLimit ± halfPlayer`

**GK Clearing** (`AIRodShootAction.cs`):
- `isGKClearing` flag tracks clearing intent (reset per evaluation cycle)
- When GK has ball and shot score < minimumShootScore, calls `FindBestClearingTarget()`
- If defense figure found, accepts shot with quicker charge (60% of light threshold vs 80%)
- Shot direction is physics-based (contact point determines angle) — clearing target is informational

**FindBestClearingTarget** (`AIGoalEvaluator.cs`):
- Accesses defense rod at index 1 (`teamController.rods[1]`)
- Scores each defense figure: center proximity + same-side-as-ball bias (0.2)
- Returns best figure Y position for logging/future angle control

**PositioningState.cs GK mode selection**:
- Defensive → `GoalkeeperIntercept` (always, replaces speed-based Intercepting/DefensiveBlocking split)
- Neutral + ball speed > 2 → `GoalkeeperIntercept`
- Neutral + slow ball / Attacking → `DefensiveBlocking` (stay-home)

---

## Change 2: Difficulty-Scaled Behaviors

### Problem
Easy, Medium, and Hard feel like the same AI with different numbers. Players can't tell the difference in behavior — only in thresholds. Easy AI should visibly make mistakes; Hard AI should feel oppressive.

### Solution: Behavior Mutations by Difficulty

#### 2A: Shot Whiffing (Easy/Medium)
**Goal**: Easy AI occasionally misses the ball when trying to shoot.

**Logic**:
- After charge completes, roll a whiff chance before executing the shot
- Whiff = animation plays but no force applied to ball (or very weak force in random direction)
- Whiff chance: Easy=20%, Medium=5%, Hard=0%
- Visual feedback: animation still plays (figure kicks), ball just doesn't go where intended

**Files to modify**:
- [x] `AIRodShootAction.cs` — Added `whiffChance` field, whiff check in `ExecuteShoot()`, charge reduced to 0.05f on whiff

#### 2B: Positioning Overshoot (Easy/Medium)
**Goal**: Easy AI overshoots target position, oscillating around the ball instead of snapping to it precisely.

**Logic**:
- Add `positioningAccuracy` (0-1): 1.0 = perfect, 0.5 = overshoot by 50% of remaining distance
- Easy=0.6, Medium=0.85, Hard=1.0
- Implementation: multiply movement delta by `1.0 + (1.0 - accuracy) * overshootFactor`
- Results in slight wobble on Easy, near-perfect tracking on Hard

**Files to modify**:
- [x] `AIRodMovement.cs` — Added `positioningAccuracy` with overshoot in `MoveTowardYPosition()`

#### 2C: Slower Rod Movement Speed (Easy)
**Goal**: Easy AI moves rods slower, giving the human player more time to react.

**Logic**:
- Add `movementSpeedMultiplier`: Easy=0.7, Medium=0.85, Hard=1.0
- Apply to all movement modes (Tracking, Intercepting, etc.)
- Stacks with existing `reactionDelay` for compounding effect

**Files to modify**:
- [x] `AIRodMovement.cs` — Added `movementSpeedMultiplier` applied in `MoveTowardYPosition()`
- [x] `AITeamRodsController.cs` — Pushes speed multiplier via `ApplyDifficultySettingsToRods()`

#### 2D: Reaction Delay Enhancement (All Difficulties)
**Status**: Deferred — existing `reactionDelay` (0.1-0.3s) + `movementSpeedMultiplier` + `decisionInterval` already provide sufficient differentiation. Adding per-action delays would add complexity without proportional benefit.

#### 2E: Difficulty Preset Summary (New Values)

| Parameter | Easy (1) | Medium (2) | Hard (3) |
|-----------|----------|------------|----------|
| whiffChance | 0.20 | 0.05 | 0.00 |
| positioningAccuracy | 0.60 | 0.85 | 1.00 |
| movementSpeedMultiplier | 0.70 | 0.85 | 1.00 |
| reactionDelay | 0.30s | 0.20s | 0.10s |
| *All Iteration 1 values* | *unchanged* | *unchanged* | *unchanged* |

### Implementation Notes (Completed 2026-02-17)

**Whiff System** (`AIRodShootAction.cs`):
- `Random.value < whiffChance` check in `ExecuteShoot()` before applying force
- On whiff: kick animation plays normally, but `effectiveCharge = 0.05f` → light shot force (barely moves ball)
- Logged as `SHOOT_WHIFF` in AI debug logs
- GK is NOT exempt from whiffing (applies to all rods equally)

**Positioning Accuracy** (`AIRodMovement.cs`):
- Applied in `MoveTowardYPosition()` when `positioningAccuracy < 1.0`
- When distance to target < 0.1 units, applies overshoot factor: `(1 - accuracy) * 2`
- Creates visible wobble on Easy (amplitude ~0.04 units), imperceptible on Hard (accuracy = 1.0)

**Movement Speed** (`AIRodMovement.cs`):
- `effectiveSpeed = speed * movementSpeedMultiplier` in `MoveTowardYPosition()`
- Applies to ALL movement modes uniformly (single bottleneck method)
- Stacks with existing per-figure-count speed (`RodConfigurationSpeed`) and difficulty speed offset

**Inspector Integration**:
- All 3 new fields visible in AITeamRodsController Inspector under "Behavior Quality"
- Editor summary shows current values
- Click Easy/Medium/Hard buttons → values update immediately during Play mode

---

## Change 3: Adaptive Strategy

### Problem
AI plays identically whether winning 4-0 or losing 0-4. No urgency when behind, no caution when ahead. Feels robotic and predictable.

### Solution: Score/Time-Based Strategy Shifts

#### 3A: Strategy State Machine
**Goal**: AI shifts between Aggressive, Neutral, and Defensive strategies based on match context.

**Strategy enum**:
```csharp
public enum AIStrategy { Defensive, Neutral, Aggressive }
```

**Determination logic**:
```
scoreDiff = aiScore - opponentScore  (negative = losing)

If scoreDiff <= -2 → Aggressive (down by 2+, push hard)
If scoreDiff >= 2  → Defensive (up by 2+, protect lead)
Else               → Neutral (close game, balanced play)

Time modifier (when TimeMatchController active):
If timeRemaining < 30s AND losing → force Aggressive
If timeRemaining < 30s AND winning → force Defensive
```

**Files to modify**:
- [ ] `AITeamRodsController.cs` — Add `AIStrategy` enum, `UpdateStrategy()` method reading from `MatchScoreController.instance` and `TimeMatchController`

#### 3B: Strategy Effects on Rod Activation
**Goal**: Strategy shifts which rods are active.

**Changes**:
- **Aggressive**: Bias rod activation toward forward rods
  - When ball is in neutral zone, activate Mid+Atk instead of Def+Mid
  - When ball is in own third, activate Def+Mid (not GK+Def) to push forward faster
  - GK stays active only when ball is very close to own goal
- **Defensive**: Bias rod activation toward rear rods
  - When ball is in neutral zone, activate GK+Def instead of Def+Mid
  - When in opponent third, activate Def+Mid (keep defense ready)
  - More conservative — always keep defense active
- **Neutral**: Current behavior (unchanged from Iteration 1)

**Files to modify**:
- [ ] `AITeamRodsController.cs` — Modify `DetermineRodConfiguration()` to factor in `currentStrategy`

#### 3C: Strategy Effects on Shooting Behavior
**Goal**: Aggressive AI shoots more; Defensive AI is more selective.

**Changes**:
- **Aggressive**: 
  - Lower `minimumShootScore` by 30% (shoot at worse angles)
  - Reduce charge time by 20% (faster, less powerful shots — volume over quality)
  - Increase pass attempt frequency
- **Defensive**: 
  - Increase `minimumShootScore` by 20% (only shoot on clear opportunities)
  - Hold ball longer (magnet duration increased)
  - Prefer clearing over scoring from GK/Def
- **Neutral**: Current values

**Files to modify**:
- [ ] `AITeamRodsController.cs` — Apply strategy multipliers on top of difficulty presets
- [ ] `AIRodShootAction.cs` — Read strategy modifier for shoot score threshold
- [ ] `AIRodMagnetAction.cs` — Read strategy modifier for magnet duration

#### 3D: Strategy Effects on Movement
**Goal**: Aggressive AI pushes rods forward; Defensive AI stays home.

**Changes**:
- **Aggressive**: 
  - AttackingPosition mode used more frequently (even in neutral ball state)
  - GK plays higher (closer to midfield line) — risky but pressures opponent
- **Defensive**: 
  - DefensiveBlocking used more frequently
  - GK stays deep (closer to goal line)
  - All rods bias toward own-goal side of their movement range

**Files to modify**:
- [ ] `AIRodMovement.cs` — Add `strategyPositionBias` offset applied to target Y positions
- [ ] `PositioningState.cs` — Strategy influences movement mode selection

#### 3E: Score Access Strategy
**Goal**: Safely access match score/time even when controllers may be disabled.

**Logic**:
- `MatchScoreController.instance` may be null (disabled in test scene)
- `TimeMatchController` may be disabled
- Fallback: if score unavailable → always Neutral strategy
- Check `MatchController.instance.endMatch` to stop strategy updates after match ends

**Files to modify**:
- [ ] `AITeamRodsController.cs` — Null-safe score/time access in `UpdateStrategy()`

---

## Implementation Order

1. **Change 1: Goalkeeper Specialization** — Self-contained, touches GK-specific paths
   - [ ] 1A: Shot Reaction Positioning (`GoalkeeperIntercept` mode)
   - [ ] 1B: Intentional Clearing to Defense
   - [ ] 1C: Goal Post Awareness
2. **Change 2: Difficulty-Scaled Behaviors** — Builds on existing difficulty system
   - [ ] 2A: Shot Whiffing
   - [ ] 2B: Positioning Overshoot
   - [ ] 2C: Slower Rod Movement Speed
   - [ ] 2D: Reaction Delay Enhancement
3. **Change 3: Adaptive Strategy** — Requires match controller integration
   - [ ] 3A: Strategy State Machine
   - [ ] 3B: Strategy Effects on Rod Activation
   - [ ] 3C: Strategy Effects on Shooting Behavior
   - [ ] 3D: Strategy Effects on Movement
   - [ ] 3E: Score Access Strategy (null-safety)

---

## Files Summary

| File | Changes |
|------|---------|
| `AITeamRodsController.cs` | AIStrategy enum, UpdateStrategy(), strategy multipliers, new difficulty params push |
| `AIRodMovement.cs` | GoalkeeperIntercept mode, positioningAccuracy, movementSpeedMultiplier, modeSwitchDelay, strategyPositionBias |
| `AIRodShootAction.cs` | EvaluateGKClear(), whiffChance, evaluationDelay, strategy shoot score modifier |
| `AIRodMagnetAction.cs` | activationDelay, strategy magnet duration modifier |
| `AIGoalEvaluator.cs` | FindBestClearingTarget() |
| `PositioningState.cs` | GK GoalkeeperIntercept trigger, strategy-influenced mode selection |

---

## Success Criteria

After this iteration, the AI should:
- [ ] GK reacts to incoming shot trajectories, not just ball position
- [ ] GK clears ball toward defense rod figures, not randomly
- [ ] GK covers near-post when ball is on one side
- [ ] Easy AI visibly makes mistakes (whiffed shots, overshooting, slow reactions)
- [ ] Hard AI feels precise and relentless (no mistakes, instant reactions)
- [ ] Player can feel the difficulty difference within 30 seconds of play
- [ ] AI pushes forward aggressively when losing by 2+ goals
- [ ] AI plays conservatively when winning by 2+ goals
- [ ] AI shows urgency in final 30 seconds when behind
- [ ] Strategy shifts are smooth (no jarring behavior changes)
- [ ] All new features degrade gracefully when score/time controllers are disabled

---

## Testing Approach

1. **GK Testing**: Shoot at goal from various angles → GK should predict and position
2. **Difficulty Testing**: Play 3 matches on Easy/Medium/Hard → Easy should feel beatable, Hard should feel challenging
3. **Strategy Testing**: Score goals to trigger strategy shifts → observe rod activation and shooting behavior changes
4. **Edge Cases**: Test with TimeMatchController disabled, test at 0-0 score, test at 4-4 score

---

## Notes for Iteration 3
- AI Personality/Play Styles (Aggressive/Defensive/Technical archetypes)
- Inter-Rod Coordination (signaling passing intent between active rods)
- Shot Faking/Feints (charge then delay to bait opponent)
- Counter-Attack Detection (fast forward passing after interception)

---

## Enhanced Logging (Added 2026-02-18)

### Problem
AI logs lacked critical data: whether shots actually hit the ball, goal events, and action chains (magnet→shoot, wallpass→shoot). This made diagnosing the "animation too slow" timing issue impossible.

### Changes

**New log event types** (`AIDebugLogger.cs`):
- `BALL_HIT` — Shot connected with ball (logged from `FoosballFigureShootAction.AttemptShot()`)
- `SHOT_MISSED` — Shot window expired without ball contact (logged from `ActivateShotWindow()`)
- `GOAL_SCORED` — Goal scored (logged from `GolController.OnTriggerEnter2D()`)
- `MAGNET_TO_SHOOT` — Magnet→shoot chain detected (logged from `AIRodShootAction.StartCharging()`)

**New analyzer sections** (`AILogAnalyzer.cs`):
- `AnalyzeShotEffectiveness()` — Hit rate, whiff rate, goals, per-rod breakdown
- `AnalyzeWallPassFollowUp()` — What happens after wall passes (→ shoot, → magnet, → nothing)

**Bug fix** (`AIRodMagnetAction.cs`):
- Fixed MAGNET_OFF log spam: was logging every frame during opponent possession (10,652 entries per match!)
- Now only logs when magnet is actually transitioning from active to inactive

**Stuck loop fix** (`AILogAnalyzer.cs`):
- Excluded MAGNET_OFF from stuck loop detection (was causing false positives)

---

## Behavioral Tuning (Added 2026-02-20)

### Problem
After 3 match analyses with player vs AI comparison logging, critical gaps were identified:

| Metric | Player | AI | Gap |
|--------|--------|-----|-----|
| Shots | 67 | 23 | Player 3x more |
| Shot frequency | every 3.7s | every 11.8s | AI 3x slower |
| Avg charge time | 0.54s | 0.81s | AI overcharges |
| Magnet hold | 0.73s avg | 0.15s avg | AI 5x too brief |
| Wall pass | 11 | 0 | AI never uses it |
| WP→Shoot chain | 91% follow-up | 0% | Player's key combo |

### Changes Made

#### Fix 1: Relaxed Wall Pass Conditions (`AIGoalEvaluator.cs`)
- **Before**: Required 2+ blockers AND shotScore < 0.3 (almost never true)
- **After**: Triggers when shotScore < 0.5 AND (shot blocked OR 1+ blocker)

#### Fix 2: Minimum Magnet Hold Time (`AIRodMagnetAction.cs`)
- **New field**: `minimumMagnetHoldTime` — prevents instant magnet release
- Increased `maxBallVelocityForMagnet` from 3.0 to 6.0 (default)
- **Difficulty scaling**: Easy=0.2s, Medium=0.5s, Hard=0.8s hold | Velocity: Easy=4, Med=5, Hard=7

#### Fix 3: Faster Shot Charging (`AIRodShootAction.cs`)
- **New field**: `shotChargeTarget` — fraction of lightShotThreshold for non-GK shots
- **Before**: base charge = 1.5s → **After**: Easy=0.8s, Medium=0.45s, Hard=0.4s

#### Fix 4: Reduced Shot Cooldown (`AIRodShootAction.cs`)
- **New field**: `chargeCooldownDuration` — Easy=2.0s, Medium=1.2s, Hard=0.5s (rapid-fire)

#### Fix 5: Wall Pass → Shoot Combo (`PositioningState.cs`)
- After wall pass, schedules follow-up shoot evaluation 0.3s later (logged as `COMBO_SHOT`)

#### Updated Difficulty Presets

| Parameter | Easy (1) | Medium (2) | Hard (3) |
|-----------|----------|------------|----------|
| minimumShootScore | 0.5 | 0.3 | 0.15 |
| shotChargeTarget | 0.8 (0.8s) | 0.6 (0.45s) | 0.4 (0.4s) |
| chargeCooldownDuration | 2.0s | 1.2s | 0.5s |
| minimumMagnetHoldTime | 0.2s | 0.5s | 0.8s |
| maxBallVelocityForMagnet | 4.0 | 5.0 | 7.0 |
| whiffChance | 20% | 5% | 0% |
| positioningAccuracy | 0.60 | 0.85 | 1.00 |
| movementSpeedMultiplier | 0.70 | 0.85 | 1.00 |

### Files Modified
- `AIGoalEvaluator.cs` — Relaxed ShouldUseWallPass()
- `AIRodMagnetAction.cs` — minimumMagnetHoldTime, velocity threshold, setters
- `AIRodShootAction.cs` — shotChargeTarget, chargeCooldownDuration, updated CalculateTargetChargeTime()
- `AITeamRodsController.cs` — New fields, updated presets, push to rods
- `AITeamRodsControllerEditor.cs` — New inspector sections
- `PositioningState.cs` — Wall pass → shoot combo

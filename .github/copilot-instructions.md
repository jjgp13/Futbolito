# Copilot Instructions — Futbolito

## Project Overview

Futbolito is a 2D foosball game built in **Unity 6** (6000.2.12f1) using **URP**, **Unity's new Input System**, and **2D physics** (Rigidbody2D / Collider2D). The game features AI opponents with an FSM-based decision system, multiple game mechanics (shoot, magnet, wall pass, bump), configurable physics/formation presets, and automated AI-vs-AI testing infrastructure.

Platform targets: Windows (primary), Android (APK build exists). Company: j2Games.

## Build & Project Setup

- **Unity 6** project — open in Unity Hub, no CLI build system.
- `Assembly-CSharp.csproj` uses **explicit `<Compile Include>` entries** (not wildcards). When adding a new `.cs` script, it must be added to this file for `dotnet build` to work. Unity regenerates this file on reimport, but manual edits may be needed for out-of-editor builds.
- No CI/CD pipelines exist. Builds are triggered from the Unity Editor.
- No automated linting or unit test suite beyond Unity's Test Framework package. The main testing tool is `AutoMatchRunner` (see below).

## Architecture

### Scene Flow

```
MainMenu_Scene
├─ QuickMatchMenu_Scene → GameMatch_Scene (player vs AI)
└─ TournamentSelection_Scene → TourMainMenu_Scene → GameMatch_Scene

GameMatchTesting_Scene — standalone AI-vs-AI scene used by AutoMatchRunner
```

`MatchInfo` is a `DontDestroyOnLoad` GameObject created in menu scenes that carries match configuration (teams, difficulty, formations, physics preset) into `GameMatch_Scene`. `SetMatchController` reads it on scene load.

### Match Lifecycle

`MatchController` orchestrates the match via static events:
- `OnMatchStart` — fired after initial animation, ball spawned
- `OnBallSpawned` — fired each time a ball is instantiated
- `OnMatchEnd` — fired on timeout or knockout (5 goals)

`GolController.OnGoalScored(string goalTag)` fires on each goal. `BallBehavior.OnBallImpact(BallImpactEventArgs)` fires on ball collisions.

All static events are nulled in `OnDestroy()` to prevent stale delegates across scene reloads.

### Rod & Figure Hierarchy

Each team has 4 rods: Goalkeeper (1 figure), Defense, Midfield, Attack (variable figures per formation). `RodConfiguration` instantiates figures, calculates spacing, magnet radii, and movement limits.

**Two-layer action system:**
- **Rod-level** (decision/coordination): `AIRodShootAction`, `AIRodMagnetAction`, `AIRodWallPassAction` — decide *when* to act
- **Figure-level** (physics/execution): `FoosballFigureShootAction`, `FoosballFigureMagnetAction`, `FoosballFigureWallPassAction` — execute the physics

Player equivalents (`PlayerRodShootAction`, etc.) mirror this pattern but are input-driven.

### AI System

**FSM with parallel actions** — 4 states: `IdleState`, `PositioningState`, `ShootingState`, `CooldownState`. The AI is ball-centric (no role-specific behavior for GK vs attacker). Actions (magnet, shoot evaluation) run in parallel during `PositioningState`.

`AITeamRodsController` manages rod activation with a **two-rod limit** (simulates human "two hands"). Which 2 rods are active depends on ball position (defensive/balanced/attacking configuration).

`AIGoalEvaluator` provides strategic scoring: clear shot path (raycast), pass advantage evaluation, wall pass opportunity detection.

**Difficulty system** — 3 tiers (Easy/Medium/Hard) with 40+ parameters including whiff chance, positioning accuracy, movement speed multiplier, reaction delay, shot charge behavior, magnet hold time, and more. Loaded from `MatchInfo.instance.matchLevel`. Also includes adaptive strategy (Aggressive/Neutral/Defensive) based on score differential.

### Data-Driven Configuration

Three key `ScriptableObject` types drive game balance:

- **`PhysicsPreset`** — ball mass/drag, bounciness, shot forces, magnet/bump strength. Applied at runtime by `PhysicsPresetManager`. 6 presets exist (Current Default, Arcade, Air Hockey, Competitive, Pinball Chaos, Speed Demon).
- **`FormationPreset`** — figure counts per rod + per-figure-count speed curves for player and AI rods. `FormationPreset.Active` (static) is the global override; falls back to `Team` SO if null.
- **`Team`** — team name, region, flag sprite, uniforms, default formation.

### Automated Testing (AutoMatchRunner)

`AutoMatchRunner` runs AI-vs-AI matches in `GameMatchTesting_Scene`. It rotates through combinations of difficulty, physics presets, and formation presets. Results are saved to `{persistentDataPath}/ai_logs/auto_test_*/`. `MultiMatchAnalyzer` produces aggregate reports.

Key detail: `Time.fixedDeltaTime` must be scaled with `timeScale` (`0.02f * timeScale`) to prevent physics spiral-of-death. `BulletTimeController` and `TimeSlowEffect` must not reset timeScale during auto-test.

### Sound System

`MatchAudioManager` (singleton on MatchController) manages 4 AudioSource layers: ambient crowd, tension, excitement SFX, celebration. `BallSoundsController` (on Ball prefab) uses velocity-scaled pitch/volume.

## Conventions

### C# Style

- **PascalCase**: classes, public methods, properties, enum values
- **camelCase**: public fields, `[SerializeField]` private fields
- **Singletons**: `public static ClassName instance` (or `Instance` property), set in `Awake()`, cleared in `OnDestroy()`
- **Event pattern**: subscribe in `OnEnable()`, unsubscribe in `OnDisable()` or `OnDestroy()`
- **Execution order**: `[DefaultExecutionOrder(-100)]` for AutoMatchRunner, `[DefaultExecutionOrder(0)]` for SetMatchController
- **`[RequireComponent]`** used to guarantee sibling component dependencies

### Key Patterns

- **Static events for decoupling** — systems communicate via `MatchController.OnMatchStart`, `GolController.OnGoalScored`, `BallBehavior.OnBallImpact` rather than direct references.
- **DontDestroyOnLoad** for cross-scene data (`MatchInfo`, `PlayerDataController`, `TournamentController`). Must guard against duplicates.
- **`AutoMatchRunner.IsAutoMode`** — checked throughout the codebase to skip UI animations, panels, and player input during automated testing.
- **Shot system**: only 2 levels — Light (charge < 1s) and Heavy (charge ≥ 1s). Animation collider sweep determines physical reach (~2.0 units max).
- **Magnet system**: "Catch & Position" pattern — attract → catch (ball becomes kinematic) → slide to front of figure → ready to shoot. `maxCatchVelocity` prevents catching fast shots.
- **Bump mechanic**: `BumpNudge` movement mode in `AIRodMovement` — rapid slam + reverse creates physics impulse. Passive stuck detection (2s threshold), closest-rod-only.
- **Anti-stall**: `BallBehavior` respawns ball after configurable inactive period. `RodBumpEffect` fires before anti-stall as a less disruptive fix.

### Known Issues

- **Right-team bias**: Right team wins ~70% of AI-vs-AI matches. Under investigation via formation rotation testing.
- **Wall passes**: Currently producing 0 successful executions — mechanic is being reworked.

## AI Planning Documents

Design context and iteration history live in `Assets/AIPlans/`:
- `AI/AI_Iteration_5.md` — current iteration (formation testing, preset cleanup)
- `MechanicsGameplay/Game_Design_Reference.md` — comprehensive design bible (mechanics, presets, rules)
- `ToolsTesting/AIDebugLogger_Plan.md` — debug logging/analysis pipeline
- `Sound/Sound_System_Log.md` — audio architecture

Read `AI_Iteration_5.md` and `Game_Design_Reference.md` first when starting work on gameplay or AI changes.

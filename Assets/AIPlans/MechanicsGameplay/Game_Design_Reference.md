# Futbolito — Game Design Reference

## Overview
Futbolito is a **2D top-down foosball (table football) game** built in Unity. Players control rods with figures to score goals, just like a real foosball table. Supports 1–4 local players with AI opponents.

## Game Layout
- **View**: Top-down 2D
- **Goals**: Left and right sides of the field
- **Rods**: Vertical bars that slide **up/down (Y axis)** for positioning
- **Figures**: Attached to rods, kick by sweeping their collider **left/right (X axis)**
- **Ball**: Moves freely on the table, bounces off walls and figures

## Rod Layout (per team)
| Index | Rod | Figures | Role |
|-------|-----|---------|------|
| 0 | GoalKepperRod | 1 | Last line of defense |
| 1 | DefenseRod | 2–5 | Block and clear |
| 2 | MidfieldRod | 1–5 | Control and pass |
| 3 | AttackerRod | 1–5 | Score goals |

Figure count depends on the team's **Formation** (e.g., 4-4-2, 3-5-2). Goalkeeper is always 1.

## Formation Presets (Testable Layouts)
Formation presets are **ScriptableObject assets** (`FormationPreset`) that bundle:
- **Figure counts** per rod (defense, midfield, attack)
- **Rod speed curves** for both Player and AI, per figure count

### Default Formation Presets
| Preset | Def | Mid | Atk | Design Intent |
|--------|-----|-----|-----|---------------|
| Classic 4-4-2 | 4 | 4 | 2 | Standard layout, baseline |
| Wide 3-5-2 | 3 | 5 | 2 | Strong midfield control |
| Defensive 5-3-2 | 5 | 3 | 2 | Packed defense |
| Attacking 2-3-5 | 2 | 3 | 5 | All-out attack |
| Balanced 3-4-3 | 3 | 4 | 3 | Even spread |

### Rod Speed Scaling
More figures on a rod means less space for the ball to pass through, so the rod moves slower as a trade-off:

**Player Rod Speeds (default curve):**
| Figures | Speed | Rationale |
|---------|-------|-----------|
| 1 (GK) | 10 | Wide gaps to cover, needs agility |
| 2 | 8 | Good coverage, fast |
| 3 | 6 | Balanced |
| 4 | 4 | Good coverage, slower |
| 5 | 2 | Near-wall-to-wall coverage, slowest |

**AI Rod Speeds (default curve):**
| Figures | Speed | Rationale |
|---------|-------|-----------|
| 1 (GK) | 3.0 | AI doesn't need as much raw speed |
| 2 | 2.5 | |
| 3 | 2.0 | |
| 4 | 1.5 | |
| 5 | 1.0 | |

Speed values are tunable per FormationPreset, so different formations can experiment with different speed curves. AI difficulty's `movementSpeedMultiplier` is applied on top of the base speed.

### How Formation Presets Work
1. **FormationPreset ScriptableObject** defines formation + speed curves
2. **FormationPreset.Active** (static) is set by AutoMatchRunner during test rotation
3. **RodConfiguration** reads figure counts from active preset (or falls back to Team's formation)
4. **AIRodMovementAction / PlayerRodMovementAction** read speeds from active preset (or fall back to hardcoded defaults)
5. **AutoMatchRunner** rotates through formation presets alongside physics presets and difficulty combos

## Controls

### Player Join System
Players join matches via **manual join** (not auto-join). `GameControlsConfigPanel` detects raw input and calls `PlayersInputController.JoinPlayerManually()` with the correct control scheme and device. Max 4 players (2 keyboard + 2 gamepad, or any mix).

**Join flow:**
1. Panel shows "Press key to join" prompt
2. Pressing a P1 key (WASD region) or P2 key (Arrow region) or gamepad button triggers a manual join
3. `PlayerInputManager.JoinPlayer(controlScheme, device)` creates a `PlayerInput` instance
4. Player selects a team side (left/right) using their assigned keys
5. Accept confirms, Start begins the match

### Control Schemes
| Scheme | Device | Join Keys | Region |
|--------|--------|-----------|--------|
| Keyboard_P1 | Keyboard | F, Space, WASD | Left side of keyboard |
| Keyboard_P2 | Keyboard | NumpadEnter, RightShift, Arrows | Right side of keyboard |
| Xbox | XInput Controller | Start, A button | Per gamepad (unique device) |

### Gameplay Controls
| Action | Keyboard P1 | Keyboard P2 | Xbox |
|--------|------------|------------|------|
| Move rod | W/S | ↑/↓ | Left Stick |
| Shoot | Space | Right Ctrl | A button |
| Magnet | E | Right Shift | B button |
| Wall Pass | Q | Numpad 0 | X button |
| Bump | R | Numpad . | Y button |
| Switch rod left | A | ← | LB |
| Switch rod right | D | → | RB |

### UI Controls
| Action | Keyboard P1 | Keyboard P2 | Xbox |
|--------|------------|------------|------|
| Join | F | Numpad Enter | Start |
| Select left team | A | ← | LB |
| Select right team | D | → | RB |
| Accept | Space | Right Shift | A button |

### Rod Control Modes
- **Rod sliding**: Analog stick Y axis — moves the active rod up/down
- **Shoot**: Button press — charge time determines shot power (light/heavy)
- **Magnet**: Hold to attract and catch the ball (see Magnet Catch & Position below)
- **Switch rod**: Cycle which rod(s) the player controls
- **1 player per team**: Controls 2 rods at a time (GK+Def or Def+Mid or Mid+Atk)
- **2 players per team**: Player 1 = GK+Defense, Player 2 = Midfield+Attack

## Shot Mechanics
- **Quick tap** (<1s): Light shot — used ~90% of the time
- **Full charge** (1+s): Heavy shot — triggers screen shake, time-slow
- Only **2 shot levels** exist (light and heavy). No medium shot.
- **Aiming**: Determined by two factors:
  1. **Contact point** — Where the ball hits the figure (top/bottom) deflects up/down
  2. **Rod sliding** — Moving the rod while shooting adds angle (like real foosball)
- **Ball momentum**: Incoming ball velocity partially preserved on contact (15%)
- **No randomness**: Shot angle is purely physics-driven

## Rod Bump (Unstick Mechanic)
In real foosball, players bump or shake the table to nudge a stuck ball. In Futbolito:
- **Trigger**: Ball velocity must be near zero for **2 seconds** (truly stuck, not just pausing to aim)
- **Who can bump**: Only the **closest rod** to the ball — prevents "cheating" (e.g., opponent bumping a ball you're positioning)
- **Effect**: Ball is pulled **toward the closest figure** on the bumping rod (like a table vibration nudging ball to the nearest player)
- **Range**: `maxBumpRange` (default 4.5 units) — wider than kick reach to resolve dead zones
- **Cooldown**: 2 seconds between bumps
- **Passive**: Works automatically for both player and AI rods — no manual trigger needed
- **Preset-tunable**: `bumpStrength` and `maxBumpRange` vary per physics preset

## Magnet Ability
Replaces the constant-force PointEffector2D with manual, velocity-aware force application. Simulates ball control that real foosball players achieve via rod rotation.

### How It Works
1. **Player holds magnet button** (or AI evaluates conditions) → activates magnet on all figures of the rod
2. **Per-figure force**: Each figure applies pull force ONLY when the ball is physically inside its trigger collider (CircleCollider2D)
3. **Velocity scaling**: Force is inversely proportional to ball speed — strong pull on slow balls, zero effect on fast shots
4. **Dampening**: Ball velocity is dampened inside the magnet zone so it settles predictably

### Force Formula
```
speedRatio = 1 - (ballSpeed / magnetMaxBallSpeed)
effectiveForce = magnetPullForce × speedRatio    (toward figure center)
dampen = lerp(dampenFactor, 1.0, ballSpeed / magnetMaxBallSpeed)
```
- Ball at rest → full `magnetPullForce`, strongest dampening
- Ball at `magnetMaxBallSpeed` → zero force, no dampening
- Ball faster than threshold → completely ignored (fast shots pass through)

### Magnet Radius
Calculated dynamically from figure spacing to prevent overlap between adjacent figures:
- For N figures: `radius = (figureSpacing / 2) × 0.9`
- Goalkeeper (1 figure): `radius = min(availableHeight / 4, 3.0)`

### Preset-Tunable Parameters
| Parameter | Default | Purpose |
|-----------|---------|---------|
| `magnetPullForce` | 15 | Max pull force when ball is slow |
| `magnetMaxBallSpeed` | 8 | Ball speed above which magnet has zero effect |
| `dampenFactor` | 0.92 | Velocity multiplier per frame inside magnet zone |

### Per-Preset Values
| Preset | Pull Force | Max Speed | Design Intent |
|--------|-----------|-----------|---------------|
| Air Hockey | 18 | 7 | Moderate |
| Default / Competitive | 15 | 8 | Balanced |
| Arcade | 12 | 10 | Weaker magnet, faster game |
| Pinball Chaos | 10 | 12 | Minimal magnet, chaos |
| Speed Demon | 8 | 15 | Weakest, ultra-fast |

### Anti-Abuse Rules
- **Speed gate**: Fast shots pass right through magnet zones (no catching power shots)
- **Trigger-based only**: Force only applies when ball is inside the figure's CircleCollider2D trigger — no remote pulling
- **Shootable check**: AI won't activate magnet if ball is already in kick range and in front

### Technical Notes
- PointEffector2D is disabled (`forceMagnitude = 0`) when magnet is on — all force is manual via `AddForce`
- `MagnetOff()` does NOT clear ball tracking — prevents losing the reference when ball is still inside trigger (OnTriggerEnter2D won't re-fire)
- `MagnetOn()` calls `RefreshBallInTrigger()` using `Physics2D.OverlapCircleAll` to detect ball already inside
- Destroyed ball detection in FixedUpdate handles goal/respawn without relying on OnTriggerExit2D

## Figure Dampening (Ball Cushioning)
Based on real foosball physics where stationary figures absorb ball energy:
- **Idle/blocking figures**: When ball hits a figure that is NOT kicking, ball velocity is reduced by 70% (`idleFigureDampenFactor = 0.3`). This simulates trapping/cushioning.
- **Kicking figures**: Full shot force applied (handled by FoosballFigureShootAction). No dampening during active shots.
- **Result**: Fast balls slow down on contact with stationary figures, making gameplay more controllable and less chaotic.

## Ball Physics
- **PhysicsMaterial2D**: Ball, Figures, Walls each have bounciness and friction
- **Rigidbody2D**: Mass and linear drag determine weight and deceleration
- **Collision detection**: Continuous mode on ball to prevent tunneling
- **Speed limit**: Optional max ball speed (configurable per preset)
- **Boundary clamp**: Safety clamp in `BallBehavior.FixedUpdate` prevents ball from escaping field (uses camera bounds minus margin). Logs `BOUNDARY_CLAMP` events.
- **Physics presets**: Switchable via Inspector buttons (Arcade, Competitive, Current Default, Air Hockey, Pinball Chaos, Speed Demon)
- **Anti-stall nudge**: If ball is near zero velocity for 1s, a gentle random push is applied as a fallback
- **Ball sprite**: Set from `MatchInfo.instance.ballSelected` on spawn; falls back to prefab default if null

## Match Rules
- **Win by knockout**: First to **5 goals** wins immediately
- **Win by time**: When the timer expires, highest score wins
- **Match time**: Configurable (1–5 minutes)
- **Difficulty**: Easy, Normal, Hard — affects AI behavior
- **Ball respawn**: After a goal, ball respawns at center with random kick-off. Also respawns after prolonged inactivity. Stuck balls are resolved by rod bump and anti-stall nudge.

## Game Flow
```
MainMenu_Scene
  └─ QuickMatchMenu_Scene
       ├─ Controller assignment (1–4 players → teams)
       ├─ Team selection (by continent)
       ├─ Match settings (ball, grass, difficulty, time)
       └─ GameMatch_Scene
            ├─ MatchController (orchestrates match lifecycle)
            ├─ MatchScoreController (tracks 0–5 goals per side)
            ├─ TimeMatchController (countdown timer)
            ├─ GolController (goal trigger detection)
            ├─ TeamRodsController ×2 (left team + right team)
            │    └─ 4 rods each with Player or AI control
            └─ Result → coins awarded → return to menu
```

## Tournament Mode
- **Group phase**: Round-robin within groups, AI simulates matches you don't play
- **Knockout phase**: Single-elimination bracket (Round of 16 → QF → SF → Final)
- **Persistence**: Tournament progress saved/loaded via JSON

## AI System
- **Architecture**: FSM per rod (Idle → Positioning → Shooting → Cooldown)
- **AITeamRodsController**: High-level strategy — decides which rods are active based on ball position
- **Ball possession detection**: Tracks which team has the ball
- **Rod activation**: Defend behind ball, attack with ball and forward rods
- **Difficulty scaling**: whiffChance, positioningAccuracy, movementSpeedMultiplier

## Persistence & Economy
- **PlayerDataController**: Tracks total matches, wins, losses, goals, coins
- **Coins**: Earned per match (varies by difficulty and result)
- **Shop**: Cosmetic items — balls, uniforms, grass skins
- **Save system**: JSON serialization via SaveSystem.cs

## Key Technical Details
- **Input**: Unity Input System with **manual join** via `PlayerInputManager.JoinPlayersManually`. Three control schemes: `Keyboard_P1` (WASD region), `Keyboard_P2` (Arrow region), `Xbox` (per gamepad). `GameControlsConfigPanel` polls raw input to detect join requests. `PlayersInputController` (DontDestroyOnLoad singleton) manages player lifecycle. `PlayerInputController_prefab` has `NeverAutoSwitchControlSchemes = true`.
- **Input architecture**: `PlayersInputController` → `PlayerInputManager` (manual join) → spawns `PlayerInputController_prefab` (PlayerInput + PlayerController) → `GameControlsConfigPanel` manages team selection → `MatchInfo` stores controllers → `TeamRodsController` wires inputs to rods
- **Physics**: Unity 2D Physics with PhysicsMaterial2D
- **Camera**: Cinemachine with impulse-based screen shake on shots
- **Rod naming**: GK rod is spelled "GoalKepperRod" (historical typo, preserved for compatibility)
- **Teams**: ScriptableObject assets with name, flag, formation, uniforms
- **Persistent data path**: `C:\Users\{user}\AppData\LocalLow\j2Games\Futbolito\`

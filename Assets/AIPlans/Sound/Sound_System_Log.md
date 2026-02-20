# Sound System — Architecture & Change Log

## Architecture

### Overview
Hybrid sound architecture:
- **Ball** = primary AudioSource for impact SFX (kick, wall hit)
- **MatchAudioManager** = singleton orchestrating crowd layers + match-flow audio
- **Event-driven** — leverages `BallBehavior.OnBallImpact` + new game events

### Component Map
```
MatchAudioManager (on MatchController GO)
├── AudioSource: ambientCrowd (loop, low volume)
├── AudioSource: tensionLayer (loop, fades in during attacks)
├── AudioSource: excitementSFX (one-shots: gasp, save, dangerous play)
├── AudioSource: celebrationLayer (goal roar, then fade)
└── Subscribes to: OnBallImpact, OnGoalScored, OnMatchStart, OnMatchEnd

BallSoundsController (on Ball prefab)
├── AudioSource: impact sounds
├── Subscribes to: BallBehavior.OnBallImpact (on same GO)
└── Plays velocity-scaled kick/wall clips with random variation

SoundMatchController (on MatchScoreController GO)
├── Delegates ambient to MatchAudioManager
├── Kickoff whistle on OnMatchStart
└── Thin wrapper — most logic moved to MatchAudioManager

GolController (on goal triggers)
├── Plays goal SFX (existing)
└── Fires OnGoalScored event → MatchAudioManager celebrates

FoosballFigureShootAction (on each figure)
├── AudioSource: shotSound (per-figure)
└── Plays shot variations scaled by charge level (existing code)
```

### Audio Clips Inventory
**Existing (Assets/Sounds/):**
- Ball/BallSound_1..9 — ball impact variations
- GolSounds/BallSound_10..13 — goal celebration
- BallAgainstPaddle.wav, BallAgainstPaddle_2.wav — figure kick
- BallHitAgainstWall.wav — wall impact
- FigureHitBall.wav — figure kick
- BallDropsInGoal.wav — ball entering goal
- CrowdSound_1..4 — ambient crowd
- GoalSound.wav — goal celebration
- JugadaPeligrosa.wav — dangerous play reaction
- Whistle_4.wav — kickoff/end whistle

**Needed (to source):**
- CrowdTension_1..2 — rising crowd murmur for attack buildup
- CrowdGasp_1..2 — short gasp for near-misses
- CrowdCelebration_1..2 — loud roar for goals (or reuse CrowdSound at higher vol)
- SaveReaction_1..2 — crowd "ohhh!" for great saves
- Whistle_End.wav — end match whistle (can reuse Whistle_4 with different pitch)

---

## Change Log

### Iteration 1 — Foundation (2026-02-20)

#### 1. Enhanced BallSoundsController
- Refactored to subscribe to `BallBehavior.OnBallImpact` event
- Added AudioClip[] arrays for figure-hit and wall-hit variations
- Velocity-based pitch (0.8–1.3) and volume (0.3–1.0) scaling
- Cooldown (0.05s) prevents overlapping sounds on rapid collisions
- Subscribes OnEnable, unsubscribes OnDisable (ball is instantiated/destroyed)

#### 2. Added Game Events
- `GolController.OnGoalScored` — static Action event fired on goal trigger
- `MatchController.OnMatchStart` — fired after init animation completes
- `MatchController.OnMatchEnd` — fired when end match sequence starts
- `MatchController.OnBallSpawned` — fired when ball is instantiated

#### 3. Created MatchAudioManager
- Singleton on MatchController GO
- 4 AudioSource layers: ambient, tension, excitement, celebration
- Ambient: loops CrowdSound clips continuously at base volume
- Tension: fades in when ball enters attacking third, fades out when cleared
- Excitement: one-shot gasps/reactions on near-miss, save, dangerous play
- Celebration: goal roar triggered by OnGoalScored, auto-fades after duration
- Near-miss detection: ball hitting TopGoalWall/BottomGoalWall at high velocity
- Dangerous play: ball in opponent zone at high velocity → JugadaPeligrosa.wav
- Volume controls: Master, SFX, Crowd — saved to PlayerPrefs

#### 4. Fixed SoundMatchController
- Uncommented PlayGolSound() — now delegates to MatchAudioManager
- Ambient loop delegates to MatchAudioManager layered system
- Kickoff whistle connected to OnMatchStart event

#### 5. Volume Settings UI
- Added Master/SFX/Crowd sliders to PauseMatchController
- Reads/writes PlayerPrefs for persistence
- Calls MatchAudioManager volume setters

#### 6. FoosballFigureShootAction
- Code already complete — needs Inspector wiring of AudioClips
- Assign BallAgainstPaddle.wav, FigureHitBall.wav as shotSoundVariations[0..2]

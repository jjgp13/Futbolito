# AI Iteration 5 — Formation Testing System & Preset Cleanup
## Futbolito — Foosball AI

### Date: 2026-03-12
### Goal: Create testable formation system to investigate right-team bias, remove broken physics presets
### Related: [AI Iteration 4](AI_Iteration_4.md), [Game Design Reference](../MechanicsGameplay/Game_Design_Reference.md)

---

## What Was Changed This Iteration

### 1. Formation Preset System (NEW)
**Problem:** Right team wins 70% — suspected root cause: rod formations and speed scaling aren't balanced. Rod speeds were hardcoded in switch statements with no way to test different formations or speed curves.
**Solution:** New `FormationPreset` ScriptableObject that bundles formation layout + tunable per-figure-count speed curves.
- `FormationPreset.cs` — ScriptableObject with defense/mid/attack counts + Player and AI speed tables
- `FormationPreset.Active` — Static reference set by AutoMatchRunner for test rotation
- `RodConfiguration.cs` — Reads formation from active preset when available (falls back to Team SO)
- `AIRodMovementAction.RodConfigurationSpeed()` — Reads AI speed from active preset (falls back to hardcoded)
- `PlayerRodMovementAction.RodConfigurationSpeed()` — Reads player speed from active preset (falls back to hardcoded)
- **Backward compatible:** When no FormationPreset is active, all behavior is identical to before

### 2. Five Default Formation Presets
Created via `FormationPresetFactory.cs` (Assets → Create → Futbolito → Generate Default Formation Presets):

| Preset | Def | Mid | Atk | Design Intent |
|--------|-----|-----|-----|---------------|
| Classic 4-4-2 | 4 | 4 | 2 | Standard layout (current default), baseline |
| Wide 3-5-2 | 3 | 5 | 2 | Strong midfield control |
| Defensive 5-3-2 | 5 | 3 | 2 | Packed defense |
| Attacking 2-3-5 | 2 | 3 | 5 | All-out attack |
| Balanced 3-4-3 | 3 | 4 | 3 | Even spread |

All start with the same speed curve as baseline. Tuning individual preset speeds is the next step after initial data.

### 3. AutoMatchRunner Formation Rotation
- New `formationPresets` list and `rotateFormations` toggle
- Formations rotate alongside physics presets and difficulty combos
- `UniqueCombos = difficulty × physics × formations` (e.g., 6 × 6 × 5 = 180 combos)
- Formation name logged in `MatchResult` and aggregate report
- `FormationPreset.Active` cleaned up on suite finish/stop/destroy

### 4. Report Generation Updated
- `MultiMatchAnalyzer` now includes per-formation breakdown section
- Match results table includes formation column
- Per-formation analysis shows: avg goals, knockouts, dead ball, left/right win rates, balance assessment

### 5. Removed Broken Physics Presets
**Problem:** Realistic Foosball (10.5 min avg, 283s dead ball) and Heavy Metal (7.0 min, 209s dead ball) were broken — too much friction/drag causing chronic re-stalling.
**Solution:** Deleted both preset assets and removed factory methods.
- Deleted: `Assets/Physics Presets/Realistic Foosball.asset`
- Deleted: `Assets/Physics Presets/Heavy Metal.asset`
- Removed: `CreateRealisticPreset()` and `CreateHeavyMetalPreset()` from `PhysicsPresetFactory.cs`
- Updated: `Game_Design_Reference.md` magnet preset table

**Remaining presets (6):** Current Default, Arcade, Air Hockey, Competitive, Pinball Chaos, Speed Demon

---

## Files Modified This Iteration

| File | Change |
|------|--------|
| `FormationPreset.cs` | **NEW** — ScriptableObject with formation + speed curves + static Active |
| `FormationPresetFactory.cs` | **NEW** — Editor factory creating 5 default formation presets |
| `RodConfiguration.cs` | Reads formation from FormationPreset.Active when set |
| `AIRodMovementAction.cs` | RodConfigurationSpeed reads from FormationPreset when active |
| `PlayerRodMovementAction.cs` | RodConfigurationSpeed reads from FormationPreset when active |
| `AutoMatchRunner.cs` | Formation preset rotation, MatchResult.formationPreset field |
| `MultiMatchAnalyzer.cs` | Per-formation breakdown section, formation in match table |
| `PhysicsPresetFactory.cs` | Removed Realistic Foosball and Heavy Metal factory methods |
| `Game_Design_Reference.md` | Formation Preset system docs, removed broken presets from tables |

---

## How to Test

### Step 1: Generate Formation Preset Assets
In Unity: **Assets → Create → Futbolito → Generate Default Formation Presets**
This creates 5 `.asset` files in `Assets/Formation Presets/`

### Step 2: Assign to AutoMatchRunner
1. Select AutoMatchRunner in Inspector
2. Enable "Rotate Formations" toggle
3. Drag the 5 formation preset assets into the `formationPresets` list
4. Formation presets in `Assets/Formation Presets/` are NOT in Resources — must be manually assigned

### Step 3: Run Test Suite
- **Quick profile:** 6 difficulties × 6 physics × 5 formations = 180 unique combos × 1 = 180 matches
- **Tip:** For initial formation-only testing, disable physics rotation to reduce combos: 6 × 1 × 5 = 30 matches

### Step 4: Analyze Results
- Check `aggregate_report.txt` → "PER-FORMATION PRESET BREAKDOWN" section
- Compare left/right win rates across formations — if one formation significantly reduces the 70% bias, that's the lead
- Compare dead ball time, goals, and knockouts across formations

---

## What to Look For in Results

1. **Does formation affect team balance?** If all formations show 70% right wins, the bias is NOT formation-related — look elsewhere (ball spawn, AI evaluation)
2. **Which formations produce the best match pace?** Target: 2-5 min, 5-10 goals
3. **Do speed curves need per-formation tuning?** If 5-figure rods are too slow (high dead ball), increase their speed in that preset

---

## Recommended Next Steps (Iteration 6)

1. **Run formation test suite** — 30-180 matches depending on scope
2. **Analyze right-team bias per formation** — determine if formation is the root cause
3. **If bias persists:** Investigate ball spawn velocity, rod X positions, AI evaluation asymmetry
4. **Tune speed curves** per formation based on data
5. **Wall pass rework** — still 0 wall passes across all testing
6. **AI passivity vs humans** — increase shoot aggression

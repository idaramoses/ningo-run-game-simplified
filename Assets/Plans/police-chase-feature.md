# Project Overview
- Game Title: Ningo Run
- High-Level Concept: Endless 3D runner set in Nigerian cities (Lagos, Abuja, Port Harcourt) combining fast-paced lane switching, word collection, and culturally vibrant street chase dynamics.
- Players: Single player
- Inspiration / Reference Games: Subway Surfers, Temple Run
- Tone / Art Direction: Stylized, vibrant arcade 3D street runner
- Target Platform: WebGL / Mobile
- Screen Orientation / Resolution: Landscape / Portrait responsive WebGL
- Render Pipeline: Built-in Render Pipeline

# Game Mechanics
## Core Gameplay Loop
The player runs forward along bustling Nigerian roads, swerving across 3 lanes, jumping over barriers and sliding under obstacles while spelling words by collecting flying letters.

With the **Police Quick-Time Threat & Lane Change Mechanic**:
1. **Periodic Threat Trigger**: While running, after a configurable interval (e.g. every 12–20 seconds), a police pursuit threat event begins.
2. **Warning & Direction Indicator**:
   - The police siren ramps up into urgent rapid wails.
   - The police cruiser accelerates directly into the player's current lane right behind them (within ~3m).
   - A pulsing warning alert appears on screen (e.g., "SWIPE TO CHANGE LANE!" with animated left/right warning chevrons / countdown bar showing the remaining reaction window: `X` seconds, configurable, e.g. 2.0 to 2.5 seconds).
3. **Player Evades (Success)**:
   - If the player swipes left or right to change into another lane before time expires:
     - The police cruiser roars forward past the player's old lane or screeches past and falls back into trailing position.
     - A quick "CLEAN DODGE!" / "+BONUS" flash triggers, rewarding the player.
     - Siren calms down to normal trailing level.
4. **Player Fails to Change Lane (Busted / Hit)**:
   - If the timer hits 0 and the player is still in the same lane:
     - The police cruiser rams the player from behind at full speed.
     - Triggers runner knockback / fall animation (`HitObstacle()`) and game over fail sequence with tire screech and loud crash sound.

## Controls and Input Methods
- Swipe Left / Right (or Left/Right Arrow / A/D): Change lanes before the countdown expires to avoid being hit from behind.

# UI
- **Lane Threat Overlay (Canvas_HUD)**:
  - Warning banner: "POLICE BEHIND! CHANGE LANE!" with red/blue flashing header.
  - Directional swipe indicator arrows (showing which lanes are available: Left, Right, or Both).
  - Circular or horizontal countdown bar indicating the time left (e.g. 2.5s -> 0s).
  - Cleanly auto-hides when evading or resetting.

# Key Asset & Context
- Existing Police Audio: `Assets/Resources/Audio/Police Siren .wav`
- Existing Police Cruiser: `Assets/Prefab/Vehicles/PoliceCruiser.prefab` with `PoliceChaserController.cs`
- Player Controller: `Assets/Script/SimplePlayerController.cs` (`currentLane`, `HitObstacle()`, `isMoving`, `MoveLane()`)
- HUD UI: `Canvas_HUD` managed in `UImanager.cs`

# Implementation Steps
1. **Extend `PoliceChaserController.cs`**:
   - Add Lane Threat state machine:
     - Configurable countdown duration `threatReactionTime` (default 2.5s).
     - Configurable interval between threats `minThreatInterval` (14s) and `maxThreatInterval` (22s).
     - `StartThreat()`: locks onto player's current lane, zooms to close proximity (~2.8m), triggers HUD warning.
     - `OnPlayerChangedLane(int newLane)`: detects if player left the targeted lane in time; triggers dodge reward, cruiser brake/pass, and resets to trailing.
     - `ThreatTimerExpired()`: if player failed to change lane in time, accelerates directly into runner position, calls `playerController.HitObstacle()` and triggers `TriggerBusted()`.
   - Dependency: Existing `PoliceChaserController.cs` and `SimplePlayerController.cs`.

2. **Add Lane Threat Warning UI (`PoliceThreatUI.cs`)**:
   - Create a lightweight HUD overlay component on `Canvas_HUD` or instantiated dynamically under the HUD.
   - Visual elements:
     - Warning title text ("CHANGE LANE!").
     - Countdown progress bar / radial fill showing `X` seconds remaining.
     - Left / Right directional indicator arrows telling the player which way to swipe (e.g. if in left lane, points Right; if in right lane, points Left; if center, points Both).
   - Dependency: Step 1.

3. **Wire Event Hooks in `SimplePlayerController.cs`**:
   - In `MoveLane(int direction)`, notify `PoliceChaserController.Instance.OnPlayerLaneChanged(currentLane)`.
   - In `TriggerFall()` / `HitObstacle()`, cancel any ongoing threat timer.
   - In `SetRoadsideIdlePose()` / `StartRunning()`, start/stop threat timers cleanly.
   - Dependency: Steps 1 and 2.

4. **Audio & Visual Polish**:
   - Police siren pitch / volume spikes when threat is active.
   - Screen shake or subtle red border vignette during the countdown.
   - Crash impact sound when rammed from behind.
   - Dependency: Step 3.

# Verification & Testing
- Enter Play Mode and start running.
- Verify normal trailing behavior for the first ~15 seconds.
- Verify threat event triggers: police car rushes close behind runner, warning UI shows countdown ("CHANGE LANE!").
- Test Success path: Swipe to an adjacent lane within the countdown; verify police car falls back, bonus/dodge message triggers, and threat ends.
- Test Failure path: Stay in the same lane until countdown expires; verify police car rams runner from behind, trigger fall animation, and game over screen appears.
- Test Edge cases: Pause game, hit an obstacle before threat expires, return to menu, and verify timers and UI reset cleanly.

---

# Current Extension: Nigerian City Scenery
This extension is the current requested work. The police-chase scope above is retained for history and is not to be reimplemented or changed by this scenery pass.

# Project Overview
- Game Title: Ningo Run.
- High-Level Concept: Strengthen the identity of Lagos, Abuja, and Port Harcourt with reusable, hand-authored street scenery on the existing endless-runner tiles. Preserve word collection, readable obstacles, and smooth running.
- Players: Single player.
- Inspiration / Reference Games: Subway Surfers for readable, vibrant scenery; Nigerian city streets for architectural and cultural cues.
- Tone / Art Direction: Stylized arcade 3D; reuse existing project assets and materials, with city-specific composition rather than photorealistic reconstruction.
- Target Platform: Android (current project target); retain compatibility with existing other builds.
- Screen Orientation / Resolution: Preserve existing settings; verify gameplay-camera framing at the project's actual phone aspect ratio before placement.
- Render Pipeline: Built-in; no pipeline conversion.

# Game Mechanics
## Core Gameplay Loop
Unchanged: run, change lanes, jump, slide, collect letters and pickups. Decoration supports the MDA aesthetic of a recognisable place without increasing mechanical difficulty or hiding threats.

## Controls and Input Methods
Preserve Legacy Input Manager and current controls. No controller or input-backend changes in this scope.

# UI
No new menus or HUD. Keep CityLocationPanel and CityHeaderImage using the existing CityManager selection.
Gameplay layout:
```
[distant city silhouettes / landmarks]
[shops + trees] [three clear running lanes] [shops + furniture]
                       [runner]
                  existing HUD overlay
```
Use readable baked sign surfaces on props where practical; avoid introducing a canvas per sign. No extra crowd animation or new interactive obstacles.

# Key Asset & Context
## Verified architecture
- Assets/Script/City/CityManager.cs: CityDefinition contains cityName, starterRoadGroup, roadTiles, plaqueSprite, headerSprite. ApplyCity activates the selected starter group and calls RoadSpawner.ReplaceActiveTiles; CityChanged notifies UI.
- Home.unity contains three configured entries: Lagos, Abuja, PortHarcourt. Preserve the internal PortHarcourt identifier and saved selection key.
- RoadSpawner already instantiates city-specific road tiles and applies CurvedWorldManager to them. Decorations should be nested prefab content, not a second runtime spawner.
- StarterRoads has Lagos, Abuja, PortHarcourt groups with existing markets, monuments, housing, trees and roadside characters. Enhance gaps rather than duplicate everything.
- Current configured tiles already contain 121–254 renderers and 108–226 colliders each. More detail must use a conservative budget; these counts are not measured runtime bottlenecks.

## Assets to create or modify
- Create reusable scenery-group prefabs under Assets/Prefab/CityScenery/Lagos/, Abuja/, PortHarcourt/, and Shared/.
- Create only necessary shared materials or baked sign textures under Assets/Art/CityScenery/. Prefer existing assets; any missing landmark can be a simple low-poly city-inspired silhouette, not a claim of exact reproduction.
- Modify the four configured tiles in each directory: Assets/Prefab/Road/Lagos/ (Road1, Road2, Road3, Road junction), Assets/Prefab/Road/Abuja/ (Abuja Road1, Abuja Road2, Abuja Road3, Abuja Road3 junction), Assets/Prefab/Road/PH/ (PH Road 1, PH Road 2, PH Road 3, PH Road junction).
- Modify Assets/Scene/Home.unity only for matching starter-group decoration, retaining existing city links.
- No CityManager or RoadSpawner code changes expected; make no unrelated police, UI or movement changes.
- Reuse candidates: Lagos POS shop, corn/food stalls, kiosks, billboard, National Arts Theatre and stadium; Abuja city gate, modern homes, planters, suya stand and rock assets; Port Harcourt kiosks, houses, gas station and palms.

## City-specific visual proposals
- Lagos: lively but orderly market clusters, local storefront/transport signs, yellow transport-related colour accents, rooftop water tanks, and occasional theatre/stadium or lagoon-inspired background compositions.
- Abuja: planned boulevard character, spaced modern facades, maintained planters, bus-stop furniture, city-gate framing, and an Aso Rock-inspired distant silhouette using suitable existing rocks.
- Port Harcourt: leafy residential-commercial streets mixed with occasional port-inspired containers, warehouse silhouettes and waterfront accents; retain shops and homes so the city is not reduced to industry alone.
- Target approximately three modular cluster types per city: storefront/market, landmark/background, and street-furniture/greenery. Rotate their placement across the four tiles instead of placing a landmark on every tile.

# Implementation Steps
1. Audit asset geometry, materials, tile bounds, camera framing and starter placement.
   - Files: the 12 configured road prefabs, Home.unity, selected source scenery assets.
   - Run dependency/dependent impact checks before every asset edit; avoid editing unrelated Lagoss legacy prefabs or recovery scenes.
   - Record baseline renderer/material/triangle counts and Android frame timings in representative runs. Previous lag claims without a valid capture are not a baseline.
   - Dependencies: approved extension.
2. Author one small demonstration scenery cluster for each city using existing assets and low-poly additions only where needed.
   - Files: new CityScenery prefab folders and any required owned shared materials/sign surfaces.
   - Remove decorative colliders from new groups, not from existing gameplay geometry. Strip unnecessary movement, pickup and animation components from decorative copies.
   - Use shared materials, economical mesh counts, no new realtime lights, no reflection probes and no per-frame scenery scripts.
   - Initial budget: at most about 12 additional renderers per ordinary tile and 20 per occasional landmark tile; revise downward if profiling requires it.
   - Dependencies: Step 1.
3. Place and visually review the three demonstration clusters before broad rollout.
   - Keep full decoration bounds outside the runner, jump, slide and pickup envelope, with additional buffer verified in gameplay view.
   - Check contrast, scale, local signage and street composition; prefer existing cultural cues rather than generic clutter.
   - Dependencies: Step 2.
4. Roll the approved visual direction across all 12 configured road prefabs and the three Home starter groups.
   - Keep additions under a clearly named CityScenery child; avoid duplicates between starter groups and tiles.
   - Preserve road lengths, seams, obstacle tags, spawn positions and all CityManager references.
   - Keep landmark silhouettes back from the track, vary sides and spacing, and preserve CurvedWorld shader behaviour and culling bounds.
   - Dependencies: Step 3.
5. Validate selection, endless spawning, restart and performance; reduce scenery if necessary.
   - Files: affected scenery prefabs/materials only unless a directly attributable integration issue is discovered.
   - Do not run global quality optimizers, clone materials per renderer, or change package source.
   - Dependencies: Step 4.

# Verification & Testing
- Edit Mode: all three city entries retain their names, sprites, starter groups and four tile references; no missing scripts/materials and no duplicate decorative groups.
- Visual: inspect gameplay views for Lagos, Abuja and Port Harcourt at the actual target aspect ratio, including home intro, ordinary tile and junction. Verify distinct identities, readable signs, grounded props, unobstructed lanes and no magenta shaders.
- Play Mode: select each city, start running, cross multiple tile seams, collect letters, jump/slide, activate magnet/glider, fail/restart, switch city and reload saved selection. Decorative objects must not collide, collect, move unexpectedly, or leak across cities.
- Curvature: far scenery bends consistently with roads and does not disappear prematurely because shader displacement exceeds renderer bounds.
- Android: compare baseline and updated captures on the same device, same city/quality settings and similar run duration, including tile-spawn frames. Target 60 FPS where hardware permits; do not claim it from Editor inspection alone.
- Acceptance: no new steady-state managed allocations from scenery logic, no added realtime lights, bounded geometry/material growth, and no repeatable frame-time regression beyond measurement noise. If the existing build already misses budget, report it and trim scenery rather than promise smoothness.
- Final report: list new groups and edited tiles, show each city’s visual result, and distinguish tested Android performance from any unverified target.

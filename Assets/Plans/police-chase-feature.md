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

---

# Current Extension: City-Specific Roadside Housing
This housing extension is the next requested scope. Preserve the completed scenery pass; do not rerun its builder or implement the historical police-chase scope. User selected **Mix both**: reuse suitable existing models and author a few missing modular house styles.

# Project Overview
- Game Title: Ningo Run.
- High-Level Concept: Replace unsuitable generic roadside residential buildings with varied city-inspired housing in Lagos, Abuja and Port Harcourt, retaining readable runner gameplay and established scenery.
- Players: Single player.
- Inspiration / Reference Games: Subway Surfers for stylized readable streets; Nigerian residential architecture for visual cues.
- Tone / Art Direction: Stylized 3D matching surrounding streets. Each city includes multiple neighbourhood styles, not an exclusive architectural stereotype or exact geographic reconstruction.
- Target Platform: Android; preserve other existing build compatibility.
- Screen Orientation / Resolution: Preserve current settings; inspect the actual portrait gameplay camera and existing intro framing.
- Render Pipeline: Built-in.

# Game Mechanics
## Core Gameplay Loop
Unchanged: running, lane changes, jumping, sliding and collection. Housing reinforces place identity (MDA aesthetics) without changing obstacles, difficulty or collision rules.
## Controls and Input Methods
Preserve Legacy Input Manager and all existing controls. No movement or input changes.

# UI
No HUD/menu changes. Keep existing city selection and badges.
```
[city housing + existing scenery] [clear running lanes] [city housing + existing scenery]
                                      runner
                                 existing HUD
```
Facades face the road; silhouettes, balconies and roof shapes should read from the running camera rather than only in aerial views.

# Key Asset & Context
- CityManager selects four configured tiles per city and the corresponding Home starter group. Keep Lagos, Abuja and PortHarcourt identifiers and all selection references unchanged.
- Scope: the 12 configured prefabs under Assets/Prefab/Road/Lagos/, Abuja/ and PH/, and StarterRoads/Lagos, Abuja, PortHarcourt in Assets/Scene/Home.unity.
- Audit confirmed city-specific houses already exist alongside cross-city reuse: some Lagos/PH tiles have objects named modern house abuja. Names alone do not prove a model's appearance or suitability.
- Objects anha and nha2 contain many renderers and may include road, sidewalk, furnishings or other shared structure. Never delete or replace these whole groups by name matching.
- Existing reusable candidates include Assets/Model/Lagos/duplext house/new-duplex-house.prefab, Assets/Model/Lagos/duplex+house+3d+model/duplex+house+3d+model.prefab, Assets/Model/Abuja/modern house 3d model/modern house 3d model.prefab, and residential-house, beige-two-story and villa FBX assets. Inspect meshes, transforms, materials and geometry budgets before selection.
- Create owned reusable housing prefabs under Assets/Prefab/CityScenery/{City}/Housing/ and required combined meshes/shared materials under Assets/Art/CityScenery/Housing/.
- Author approximately one new modular house type per city initially, supplemented with suitable existing models; increase only if the initial visual review demonstrates a gap.
- Housing direction: Lagos compact balcony apartment/courtyard blocks with water-tank details plus existing duplex variety; Abuja modern duplexes and walled compounds with spaced frontage; Port Harcourt pitched-roof bungalows, verandas and low-rise compounds with greenery. These are design cues shared across Nigeria, not claims of styles unique to a city.
- Preserve all completed CityScenery props and markers. Do not modify imported source models or third-party materials globally; use owned variants when adaptation is needed.
- An editor-only, idempotent housing authoring helper may be added under Assets/Editor/ if needed. No runtime house spawner or per-house Update scripts.

# Implementation Steps
1. **Detailed housing inventory and baseline.**
   - Inspect residential mesh objects within all 12 tile prefabs and the three Home starter groups, including nested anha/nha2 contents; identify residential geometry separately from road and landmark content.
   - Inspect suitable source assets and their dependencies/dependents; repeat impact checks for each asset before editing.
   - Record exact replacement targets, frontage direction, ground elevation, bounds, triangle/render/material counts and any gameplay component dependencies. Avoid legacy Lagoss and recovery-scene edits.
   - Dependency: approved housing extension.
2. **Prepare three housing palettes and sample variants.**
   - Reuse suitable low-cost models; author one missing modular style per city using combined low-poly geometry and shared materials, not stacks of individual renderer-heavy primitives.
   - Prefer 1–3 renderers per new modular house, about 5,000 triangles or less per new house, and no net increase in whole-tile triangle/render counts where practical. Retained imported houses must be budgeted from actual geometry rather than these targets.
   - No realtime lights, new animated residents, decorative colliders or runtime material clones. Avoid interiors that cannot be seen.
   - Files: new Housing prefabs, owned meshes/materials, optional editor-only authoring helper.
   - Dependency: Step 1.
3. **Demonstrate one residential street segment per city.**
   - Replace identified residential targets only; match reasonable footprint and height without distorting models. Adjust placement for road-facing frontages and grounded bases.
   - Preserve road/sidewalk meshes, gameplay colliders, obstacles, landmarks, shops, greenery, signs and existing scenery. Remove only confirmed decorative house content and no referenced components.
   - Inspect gameplay-camera screenshots and multi-angle scene views for scale, occlusion, seams, overlap and city identity. Limit capture-and-fix rounds to three; seek feedback if still unsatisfactory.
   - Dependency: Step 2.
4. **Roll out selected palettes across configured tiles and starters.**
   - Replace unsuitable repeated residential buildings, not every building indiscriminately. Keep suitable houses and skyline/landmark diversity.
   - Vary at least two suitable housing appearances per city using models or owned material/facade variants. Preserve believable spacing and unobstructed junctions.
   - Keep deliberate house groups named CityHousing for traceability. Any scripted authoring must be idempotent and use explicit inspected targets, not destructive broad name searches.
   - Do not automatically rerun NigerianCitySceneryBuilder.RefreshPlacement; it also sees marker children and is not a housing-placement tool.
   - Dependencies: Step 3 and per-asset impact checks.
5. **Validate and trim geometry.**
   - Check city selection, spawned tiles, startup, restart, glider view, shaders and culling after the replacements.
   - Compare before/after whole-tile geometry and materials, and profile representative Android runs if a device/capture is available. Report unavailable device testing explicitly.
   - Files: only housing variants and directly affected scene/prefab content. No unrelated input, police, camera, package or global quality edits.
   - Dependency: Step 4.

# Verification & Testing
- Static: CityManager retains all three entries, four tiles each, sprites and starter references. No missing materials/scripts or broken prefab dependencies.
- Geometry safety: unchanged road and sidewalk structure, gameplay colliders and spawn markers; residential mesh bounds remain outside the full runner/pickup/jump envelope and side-road junctions.
- Visual: inspect each city's ordinary and junction tiles plus starter intro, from actual gameplay framing. Houses must face the street, sit on the ground and avoid intersecting greenery, markets, markers or neighbouring houses. New detail must be visible rather than hidden far behind old buildings.
- Curvature: use compatible shared shaders; check far facade deformation and culling without marking moving/recycled tiles as permanent static world geometry.
- Play Mode: select each city, run through tile seams, change lanes, jump/slide, activate glider, fail/restart and switch cities. Use full game flow when validating HUD occlusion; identify isolated camera-only captures separately.
- Performance: compare whole-tile renderer, material, triangle and collider counts with Step 1 baseline. Aim to replace rather than increase density. No new per-frame housing allocations or scripts; preserve lighting cost.
- Android frame pacing: verify on the same device/quality settings if available. No 60-FPS guarantee based on Editor or static counts.
- Final deliverable: list new housing variants and replaced targets by city, show before/after visuals, and distinguish completed static checks from runtime or Android checks that could not run.

---

# Current Extension: Nearby Abuja Junction Turning Taxis
This is the current requested implementation scope. Preserve previous scenery and housing work, and do not implement or revise the historical police feature. User confirmed: only the two side-road taxis, random destination lane and turn, and speed-aware activation.

# Project Overview
- Game Title: Ningo Run.
- High-Level Concept: Bring the Abuja junction to life with two taxis that approach from side streets, turn into a running lane, and continue driving. These visible moving hazards add variety and dodge decisions without moving unseen long before the runner arrives.
- Players: Single player.
- Inspiration / Reference Games: Existing Ningo Run vehicle hazards and the readable traffic encounters of arcade endless runners such as Subway Surfers.
- Tone / Art Direction: Preserve existing stylized Abuja taxi models, streets, housing and curved-world rendering.
- Target Platform: Android, current project target.
- Screen Orientation / Resolution: Preserve existing project resolution/orientation; validate using actual gameplay-camera framing and phone aspect ratios.
- Render Pipeline: Built-in; no new rendering packages.

# Game Mechanics
## Core Gameplay Loop
- Runner continues running, lane switching, jumping/sliding and collecting as before.
- Each specified taxi stays parked until the moving runner is approaching its junction entry point, approximately 3–4 seconds away at the current forward speed. Tune timing using taxi approach duration as well as runner speed so the turn is actually visible.
- Choose a destination running lane (left, center or right) and a geometrically valid left/right turn once at activation. Do not chase or retarget the runner after committing.
- Taxi moves laterally along its own side street, follows a smooth 90-degree turn onto the chosen lane, then cruises along that lane, either with or against the runner as dictated by the turn and approach direction.
- Both turn directions must be possible for each taxi when road geometry allows, rather than hardcoding one direction per taxi. Keep paths on drivable surfaces and avoid impossible turning radii at outer lanes.
- Coordinate the two taxi departures and conflicting routes: stagger starts or hold a taxi before entry so cars do not visibly overlap or create an unavoidable combined blockade. Allow at least one practical escape lane/reaction window; also assess existing junction obstacles.
- Once activated, complete the route rather than freezing abruptly when the runner crosses the proximity threshold. Stop or clean up when sufficiently behind/out of the encounter. Never activate an already-passed junction.
- Use the existing obstacle collision rules: ordinary hit/fall behavior, skateboard crash and invulnerability remain unchanged. No new damage or rewards system.
- Design rationale: route variety supplies MDA challenge/surprise, while precommitted motion and speed-aware notice preserve player competence and readable choices. This is traffic, not a homing hazard.

## Controls and Input Methods
- Verified activeInputHandler = 0 (Legacy Input Manager).
- Preserve all existing touch/swipe and keyboard controls; no new player controls or input packages.

# UI
No new menus, HUD overlays or text. Movement itself is the primary cue, with the existing HUD unobstructed.
```
             main running road
              L   C   R
side taxi --> [smooth turn] <-- side taxi
              |   |   |
                runner
             existing HUD
```
The diagram is conceptual: the inspected side-road strips are offset longitudinally rather than exactly opposite. Author each route against the actual geometry. Existing taxi alerts are not required for initial scope; do not add new headlights, realtime lights, audio assets or per-car canvases. Camera-view timing/readability is an acceptance requirement.

# Key Asset & Context
## Verified architecture and geometry
- Target: `Assets/Prefab/Road/Abuja/Abuja Road3 junction.prefab`.
- Exact targets: `Sidelane/RightLane/city taxi` and `Sidelane/LeftLane/city taxi`. Contrary to the original description, they are children of Sidelane, not Mainlane; user confirmed these two.
- Leave root taxis `city taxi (5)` and `city taxi (7)` unchanged.
- Both target taxis currently have MeshFilter, MeshRenderer, BoxCollider and enabled VehicleMover; they are tagged Obstacle, with triggerStart false, AgainstPlayer direction and speed range 6–12.
- `VehicleMover.Update()` only translates along world +/-Z and does not turn; rotating these models alone cannot fix this. `Start()` also ground-snaps and creates an AudioSource, so disabling it only after Start is too late for deterministic authored setup.
- Taxi model orientation is nonstandard: local Euler rotations approximately (270,90,0) and (270,270,0); ancestor scales are approximately 31.35. Preserve model correction separately from road-facing yaw and do not multiply intended world speed by ancestor scale.
- Renderer bounds: taxi length approximately 4.01m, width 1.85m, height 1.58m. Main road is approximately 7.35m wide and 40.08m long. Right side-road center relative to prefab root is approximately (23.69,-0.19,17.37); left is (-23.54,-0.19,3.58). Taxi centers are about (9.12,0.56,17.28) and (-9.34,0.56,3.79).
- Prefab root has a large saved world translation; routes must be relative to the instantiated tile, never copied as absolute asset-world positions.
- Asset-only BoxCollider bounds are not a reliable live physics measurement. Verify collider alignment against instantiated mesh bounds; keep collider edits local to these two prefab overrides if needed.
- Home active systems verified: CityManager references this prefab as Abuja's fourth tile; enabled RoadSpawner on Createway follows `amaka runner rigged`. Runner SimplePlayerController is active with laneDistance 2.3 and forwardSpeed 10. Do not use the class default 2.5 lane spacing or unrelated legacy spawners as runtime truth.
- `RoadSpawner` creates tiles at world (0,0,z), retains tiles until behind the runner, destroys them on ResetSpawner, and destroys/replaces active tiles on city changes. It applies CurvedWorldManager to tile children.
- `SimplePlayerController` uses overlap-based detection, recognizes Obstacle tags and VehicleMover components, and handles skateboard/invulnerability before private HitObstacle(). No direct calls to HitObstacle() or player controller changes are needed.
- Dependencies and reverse dependents inspected via AssetDatabase: target is referenced by Home and recovery scenes. Do not edit recovery scenes, global taxi source models or imported package code.

## Chosen implementation and alternatives
- Recommended selected architecture: explicit prefab-local routes with lightweight junction/taxi controllers. This follows the narrow road geometry reliably, supports readable smooth turns, and avoids shared VehicleMover behavior changes.
- Alternative considered: extend shared VehicleMover with a general path-following mode. It could reuse alert code but increases regression risk for trains/pooled traffic and unrelated callers; not needed for two localized taxis.
- NavMesh/physics-driven traffic is not required: fixed lane paths on spawned endless tiles are simpler and cheaper than introducing navigation baking or a traffic simulation package.

## Files to create or modify
- Create `Assets/Script/Vehicles/JunctionTaxiController.cs`: side-street approach, curved turn, cruise, cleanup and reset states, speed/world-distance path advancement, model-facing correction, and inspector/gizmo diagnostics.
- Create `Assets/Script/Vehicles/JunctionTrafficController.cs`: one coordinator on this junction, explicit references to two taxis and route anchors, cached runner/game references, speed-aware activation, route selection and departure/conflict spacing.
- Modify only the target Abuja junction prefab: add a named JunctionTraffic configuration/anchor group, attach controllers and assign both taxis. Apply all changes as local overrides, never edit `Assets/Model/Abuja/city taxi/city taxi.prefab` globally.
- Disable the existing VehicleMover components on the two targeted taxis in serialized prefab state before runtime; retain their components so collision identification remains compatible. New controllers are the sole owners of their transforms and cleanup; ordinary taxis retain their original movement.
- Implementation discovery: RoadObstacleHider randomly deactivates children tagged Obstacle in LateUpdate, which would suppress this guaranteed two-taxi encounter. Set only these two taxis to Untagged while retaining their VehicleMover collision markers; SimplePlayerController explicitly recognizes that component independently of the tag, including for trigger filtering. No shared hider/player code changes. Other obstacles keep their tags and randomized density.
- Add Edit Mode tests under `Assets/Tests/EditMode/` and Play Mode tests under `Assets/Tests/PlayMode/` if compatible test assemblies/framework already exist; otherwise use the project's supported verification harness and document prerequisites before adding test-only infrastructure.
- Read-only integration references: `Assets/Script/Vehicles/VehicleMover.cs`, `Assets/Script/SimplePlayerController.cs`, `Assets/Script/Road/RoadSpawner.cs`, `Assets/Script/City/CityManager.cs`, `Assets/Script/GameStateController.cs`, `Assets/Scene/Home.unity`. No expected edits to these.

# Implementation Steps
1. **Confirm live integration, route geometry and impact.**
   - Files: target junction, listed read-only integration scripts and test configuration.
   - Recheck dependencies/dependents before asset changes; inspect full game-state, city-switch and reset flow and existing test assemblies.
   - Inspect an instantiated junction's mesh orientation, collider bounds, ground elevation, drivable intersection footprint and current camera visibility. Record unaffected taxis and existing obstacles for regression checks.
   - Use actual runner lane centers and side-road elevations; preserve existing side streets rather than treating them as a single aligned crossroad.
   - Dependencies: approval of this extension.
2. **Implement taxi path/state controller.**
   - File: new JunctionTaxiController.cs.
   - States: Waiting, Approaching, Turning, Cruising, Finished. Gate advancement on active gameplay and scaled delta time; cache initial local pose and restore runtime state safely on enable/reuse.
   - Store approach/curve geometry relative to the tile root; evaluate into world coordinates and advance by world distance for consistent speed under transformed/scaled parents.
   - Use sampled arc-length or equivalent distance-based curve traversal, smooth tangent-derived yaw, and a preserved mesh rotation correction. Avoid teleportation, cutting corners and ground-snap side effects.
   - After the turn, maintain selected running lane and world +/-Z cruise direction. Keep taxi tile-owned for reliable city-switch/restart destruction; ensure parent tile lifetime covers the visible encounter and clean up only this taxi when far behind. Do not detach into leaked scene objects.
   - No per-frame Find, LINQ, new arrays, allocations or material instantiation. Missing runner/route references fail safely without movement or repeated console spam.
   - Dependencies: Step 1.
3. **Implement proximity, random routes and fair departures.**
   - File: new JunctionTrafficController.cs.
   - Bind SimplePlayerController.Instance and GameStateController only when available, with bounded retry outside hot-path work. Require a moving runner in active gameplay.
   - Predict time to the relevant side-road entry using current positive forwardSpeed; start from a tunable approximately 3.5-second notice window and account for each taxi's travel time. Expose reasonable activation-distance limits and departure spacing for tuning; avoid division by zero and reject passed encounters.
   - Randomly select from validated lane/turn combinations at activation, then lock route. Use per-junction coordination to hold conflicting departures; do not change routes mid-turn or track the runner's subsequent lane changes.
   - Tune entry/turn times so movement remains visible when the runner is nearby, not already completed before the runner reaches view. Do not trigger both offset entrances too early from a single tile-start distance.
   - Once started, finish or clean up; no repeated activation from threshold jitter. Pause/failure freeze motion, and tile recreation/re-enabling resets to the authored pose.
   - Dependencies: Steps 1–2.
4. **Wire only this prefab and inspect all route combinations.**
   - File: Abuja Road3 junction.prefab.
   - Add tile-local approach/turn anchors and explicit taxi references, disable the two existing VehicleMover components, attach the controllers, and preserve original meshes, materials, scenery and unrelated traffic.
   - Adjust only confirmed incorrect target BoxCollider overrides to fit the taxi model while retaining Obstacle tags and collision semantics.
   - Preview/test up to 12 basic combinations (2 approaches x 3 destination lanes x 2 turns), excluding any geometrically invalid combination with documented reason. Ensure variety still includes left and right turns when feasible.
   - Verify paths fit the main road, curb clearance and 4m taxi body, not just pivot positions. No new renderers or realtime lights.
   - Dependencies: Steps 2–3 and refreshed prefab impact check.
5. **Runtime balance, lifecycle and regression verification.**
   - Files: new controllers, target prefab and tests only.
   - Run full Home -> Abuja -> start gameplay with actual RoadSpawner spawning; tune speed-aware notice, taxi speed and staggering at initial and increased runner speeds.
   - Verify observed hits through the existing collision flow, avoiding both taxis, skateboard protection, invulnerability, glider clearance and normal traffic behavior.
   - Verify pause/resume, game over, restart, city replacement and several simultaneous spawned junctions. Check taxi ownership, no duplicate movement, stale routes or dangling references.
   - Compare frame timings in representative Android runs if a device is available; report Editor-only testing honestly.
   - Dependencies: Step 4.

# Verification & Testing
- Static scope: only specified two taxis gain turning behavior. Source taxi prefab, root taxis, other cities, existing road/city links, road lengths, scenery and housing stay unchanged; no missing scripts/references.
- Activation: taxis stay still when distant, player missing, intro idle, game not playing or encounter already passed. At 10m/s and increased speeds, movement begins within a tuned 3–4-second approach window and is visible from gameplay camera. No threshold retriggering.
- Routes: deterministic forced tests cover valid lane/turn combinations; selected path stays fixed after runner lane changes. Position/tangent continuity at approach/turn/cruise boundaries; correct model orientation and world speed despite nested scales and nonzero saved prefab position.
- Colliders: compare actual instantiated collider/renderer bounds at straight and turned poses. Runner collision uses existing fall/skateboard/invulnerability behavior rather than a new direct damage hook. Check max-speed encounters for missed overlaps; if a reproducible tunneling issue requires shared collision changes, report it and revise the plan before broadening scope.
- Fairness: departures do not overlap taxi bodies, simultaneously deny all escapes, or produce unavoidable encounters combined with authored obstacles. Test both taxis together at multiple speeds and verify turns happen in view rather than offscreen.
- Lifecycle: pause during each state; resume without jumps. Fail/restart and switch cities mid-turn; old tile-owned taxis disappear and recreated taxis reset. Cruise remains valid across seams and is not visibly cut off by premature parent removal; if tile ownership cannot cover intended cruise, revise the lifecycle design before detaching vehicles.
- Rendering: no tipped/flipped taxis, floating wheels, sideways longitudinal cruising, magenta materials or curvature/culling regression.
- Performance: no new steady-state managed allocations from traffic controllers, no per-frame scene searches, no extra realtime lights or materials. Device capture required for Android performance claims.
- Final report: list exact modified targets/files, show gameplay-camera turning behavior, describe tunable timing/speed settings, and distinguish automated, manual and Android checks actually run from unverified tests.

---

# Current Extension: City Radio Gist During Runs
This is the current requested scope. Preserve completed character accessories, scenery, housing and traffic; do not rerun historical implementations. User selected pre-generated offline speech, English with light Nigerian Pidgin, and short spaced segments.

# Project Overview
- Game Title: Ningo Run.
- High-Level Concept: Add a playful fictional local radio station to each Nigerian city. Random short presenter segments reinforce the current city's identity while the player runs, without distracting from obstacles or warning sounds.
- Players: Single player.
- Inspiration / Reference Games: Existing Ningo Run arcade gameplay; fictional in-world radio presenters and Nigerian city life, not licensed broadcasts or impersonations.
- Tone / Art Direction: Warm, family-friendly English/Pidgin banter; compact emerald-accent HUD styling matching the existing game. No celebrity voice imitation.
- Target Platform: Android (current target); retain existing build compatibility.
- Screen Orientation / Resolution: Preserve existing orientation, CanvasScaler and safe-area setup; check actual gameplay aspect ratios rather than impose a new resolution.
- Render Pipeline: Built-in; no conversion.

# Game Mechanics
## Core Gameplay Loop
- Existing running, jumping, sliding, pickups and lane changes remain unchanged. Radio supplies MDA atmosphere/place identity, not a gameplay advantage or a required interaction.
- Select clips only from the currently selected city's bank: Lagos, Abuja, or internal identifier PortHarcourt.
- Start a first short segment approximately 2–4 seconds after genuine running begins. Target 8–15-second segments followed by randomized 15–25-second quiet gaps, tunable in the Inspector. Countdown and paused time do not advance the scheduler.
- Shuffle each city's usable clips without immediate repetition; refill when exhausted. Handle zero/one-clip banks safely, skipping null clips.
- Strict eligibility: valid active runner with IsMoving, GameStateController.IsPlaying(), visible active gameplay HUD, no opening/resume countdown, positive time scale, and application focus. Do not infer running from HUD visibility alone.
- Jumping/sliding and existing run powerups remain part of the active run. Intro motion, falling/end-of-run presentation, menus, pause, resume countdown, failure, completion and backgrounded app are silent.
- Stop current audio immediately whenever eligibility becomes false; do not allow an old segment to leak across state transitions. Restart scheduling with a short delay when eligible again. City changes cancel old-city speech and clear pending selection before selecting the new bank.
- One dedicated 2D voice AudioSource; never overlapping presenters. No live TTS, runtime requests, credentials, microphone permissions or streamed real-world station.

## Controls and Input Methods
- Preserve verified Legacy Input Manager controls. No movement or backend changes.
- Radio indicator is informational in this scope, not a new interactive control. Respect the existing SoundEnabled/SFXVolume preferences and SoundEffectsManager's live state; Music mute remains independent. No new mute menu is required.

# UI
Create a small RadioIndicator child under Canvas_HUD. Place it in an inspected free area below the existing top HUD, outside the center track and current pickup/progress bars; final placement depends on actual safe area and gameplay screenshot review.
```
[existing pause / counters / distance]
             [radio icon  LAGOS RADIO  | equalizer]
                   clear track view
          [existing powerup indicators unchanged]
```
- Show during eligible running only. Idle between segments uses a steady icon; animate a restrained equalizer only while voice is playing. Hide/reset outside running.
- Display current city station name; use Port Harcourt as the human-readable label while retaining PortHarcourt as the data key.
- Reuse existing font/material and UI style, disable raycast targets, and avoid per-frame text rebuilding. No large scrolling text, new screen, or subtitles in this initial scope.
- Use a simple authored icon/equalizer from uGUI shapes or an owned sprite; no external asset-generation dependency for the icon.

# Key Asset & Context
## Verified integration
- Assets/Scene/Home.unity contains Canvas_HUD with Canvas, CanvasScaler, GraphicRaycaster, GameHUDController and CoinPickupEffectManager. Children include TopPanel, Countdown, ToastUI_Background and four named pickup progress bars.
- Assets/Script/City/CityManager.cs exposes CurrentCityName and CityChanged; city selection replaces road tiles. Keep its saved keys, identifiers and road references unchanged.
- Assets/Script/GameStateController.cs exposes IsPlaying()/SetPlaying(); false is used during non-gameplay, but is not sufficient alone for countdown/runner readiness.
- Assets/Script/Ui/UImanager.cs exposes IsCountingDown and static OnLevelReady. Pause hides canvasHUD, sets playing false and freezes time; Resume shows HUD before CountdownAndResume completes. Radio must not start just because OnEnable fires.
- Assets/Script/SimplePlayerController.cs exposes IsMoving and IsInRoadsideIdlePose; StartRunning enables movement after the intro. Rebind when the selected runner changes, without hot-path scene searches.
- Assets/Script/Audio/SoundEffectsManager.cs exposes IsSoundEnabled and maintains SFXVolume. Assets/Script/Ui/SimpleAudioToggle.cs writes SoundEnabled and changes that manager live.
- Assets/Script/Audio/BackgroundMusicManager.cs creates two crossfading AudioSources and applies music preferences. No AudioMixer asset was found; existing scene sources are unrouted. No mixer creation or broad audio rerouting is authorized by this scope.
- No radio/TTS assets or scripts were found by targeted filename search. A supported speech synthesis tool/provider and pronunciation quality have NOT yet been verified.

## Files to create or modify
- Create Assets/Script/Audio/CityRadioBank.cs: city key, display label, entries with stable ID, transcript and AudioClip reference.
- Create Assets/Script/Audio/CityRadioController.cs: scheduling, shuffle selection, state eligibility, audio control, city subscription, settings and cleanup.
- Create Assets/Script/Ui/CityRadioIndicator.cs: presentation-only binding to radio status and city display.
- Create three bank assets under Assets/Audio/Radio/Banks/ and owned voice clips under Assets/Audio/Radio/{Lagos,Abuja,PortHarcourt}/.
- Store reviewed transcripts and production/provenance notes under Assets/Audio/Radio/Content/. Initial target: six varied segments per city (18 total), one consistent warm presenter voice. Produce one sample per city before bulk synthesis to validate cadence and pronunciation.
- Modify Assets/Scene/Home.unity only to add/wire the radio controller, dedicated non-looping playOnAwake=false AudioSource, bank references and RadioIndicator. Keep controller on an active scene service object, not solely on the hidden HUD, so it can observe lifecycle transitions.
- Existing CityManager, player, GameStateController, UImanager, GameHUDController and sound systems are integration references; no edits expected. If true falling/completion gates cannot be observed reliably, propose the smallest explicit lifecycle hook and revise this plan before expanding edits.
- Add tests under existing compatible test assemblies if present, otherwise document manual checks; do not overwrite unrelated verification scripts.

## Content direction and production boundary
- Lagos: transport banter, neighbourhood food/markets, mainland/island everyday life and coastal atmosphere.
- Abuja: boulevards, parks, everyday neighbourhood life, food and city landmarks.
- Port Harcourt: Garden City greenery, food, rain-ready everyday life, waterfront/port culture without reducing the city to industry.
- Original fictional entertainment, not live news or claims about current traffic/weather. Avoid politics, insults, unsafe running advice, stereotypes and real-person gossip.
- Speech must be intelligible with music and existing SFX. Do not assume an accent/language is supported just because a generator produces audio.
- Before synthesis verify an actual text-to-speech-capable provider/tool, permitted voice usage and offline distribution. If unavailable, report the blocker and request approved external voice clips/provider; do not substitute generic sound effects or claim placeholder clips are finished.
- Preserve existing music initially and tune voice level in context. If speech needs music ducking, obtain approval for a narrowly scoped music-manager change rather than manipulating its crossfade sources or setting up mixers silently.

# Implementation Steps
1. **Confirm scene lifecycle, placement and asset impact.**
   - Inspect Home HUD rectangles, safe-area hierarchy, live runner selection, completion/falling flows and existing tests. Use AssetDatabase dependencies/reverse references where dependency tooling is unavailable, and repeat impact checks before every edited asset.
   - Confirm the strict eligibility gate against actual intro, start, pause/resume, death and completion sequences.
   - Dependencies: approval of this extension.
2. **Implement bank and deterministic scheduler with temporary test clips.**
   - Files: CityRadioBank.cs and CityRadioController.cs.
   - Implement eligibility, delayed initial playback, one voice at a time, shuffled city pools, gaps, settings, focus handling and cleanup. Cache references; subscribe/unsubscribe to city events; fail silently with a one-time diagnostic for missing data.
   - Stop voice in OnDisable/OnDestroy and on invalid eligibility even when time is frozen (AudioSource does not inherently stop with timeScale). Reset on run/city transitions and avoid per-frame allocations/LINQ.
   - Dependencies: Step 1. Temporary fixtures are for testing only, not final voice assets.
3. **Author radio content and validate speech production.**
   - Files: Content transcripts, sample clips and three bank assets.
   - Verify provider capabilities; synthesize one sample per city, listen for pronunciation, pacing, distortion and understandable Pidgin, then complete the six-segment banks if samples are acceptable. Keep exact text and provenance for replacements.
   - Import compressed mono voice with sensible Android settings; choose loading based on measured memory/startup cost rather than placing everything in Resources. Validate real nonempty audio, duration and references.
   - Dependencies: Step 2, verified speech provider. Missing synthesis capability blocks final voiced delivery, not silently waived.
4. **Create and wire the HUD indicator.**
   - Files: CityRadioIndicator.cs, Home.unity, optional owned icon sprite.
   - Add the compact named UI group; reuse existing text font and scaling; set all informational graphics to non-raycasting; connect playing/waiting/city states. Keep existing HUD and touch controls unchanged.
   - Wire scene service controller, single AudioSource, actual HUD and city banks. No persistent duplicate radio service across scene reloads.
   - Dependencies: Steps 1–3; refreshed scene dependency impact check.
5. **Verify full gameplay and tune clarity.**
   - Files: controller/data/UI and directly related voice clips only.
   - Test each city's actual gameplay run, silence gates and lifecycle, shuffle behavior, music/SFX clarity and UI placement. Adjust intervals/levels and clip content rather than suppressing critical warning effects.
   - Compare memory and frame timing on Android if available; inspect offline operation and application backgrounding. Remove temporary fixtures and preview objects.
   - Dependencies: Steps 2–4.

# Verification & Testing
- Pure scheduling tests: city-specific pool only; all usable clips visited before refill; no adjacent duplicate when bank has more than one usable clip; zero/null/single entries safe; randomized delays within configured bounds.
- Gating tests: no voice in menu, intro, opening or resume countdown, pause, fail, complete, disabled HUD, missing/stopped runner, timeScale=0 or backgrounded app. Immediate stop on transitions; no delayed coroutine revives old audio.
- Full Play Mode: select Lagos/Abuja/Port Harcourt, start after intro/countdown, hear appropriate segments, jump/slide/use powerups, pause during speech and quiet gap, resume, fail/retry/continue, complete, return home and switch city. Only one radio source speaks and the indicator matches its state.
- Settings: Sound off silences voice live and after reload; Sound on permits new segments only during running; SFXVolume adjusts voice consistently; Music mute does not enable/disable radio. Test missing managers without crashes.
- Content/audio: listen to every produced segment, verify transcripts/duration and regional pronunciations, original fictional framing, usable English/Pidgin, no loudness jumps or clipping, and critical SFX remain audible.
- HUD: actual gameplay camera/screens at target aspect ratios and safe areas; radio does not cover counters, pause control, pickups, warnings or lane view; no raycast interception and no equalizer motion when waiting/silent.
- Lifecycle/performance: repeated reload/retry creates no duplicate sources/listeners or stale city subscriptions; no runtime network calls; no steady-state scheduling allocations; check memory and audio load spikes, report unavailable Android testing honestly.
- Acceptance/report: identify generated/reused assets and exact edited files, distinguish complete voice clips from blocked or placeholder production, and report what was tested in Play Mode and on device. Do not claim the feature works solely from serialized references.

---

# Current Extension: 15 Additional Gist per City and Two Fixed Presenters
This supersedes the proposed runtime TTS migration. User explicitly chose the previous pre-generated offline method, 15 additional distinct gist per city, and both male and female voices. Do not install a TTS plugin or implement cloud/runtime synthesis. Earlier radio instructions are historical where they conflict with this extension.

# Project Overview
- Game Title: Ningo Run.
- High-Level Concept: Expand the fictional city radio with varied original English/Pidgin entertainment delivered by two consistent presenters. Preserve the established runner gameplay and offline playback.
- Players: Single player.
- Inspiration / Reference Games: Existing Ningo city radio and Nigerian everyday city life; no licensed broadcast reuse or presenter impersonation.
- Tone / Art Direction: Friendly, family-safe, city-specific English with light Nigerian Pidgin. Keep existing emerald HUD.
- Target Platform: Android; preserve other existing build support.
- Screen Orientation / Resolution: Preserve current CanvasScaler, orientation and layout.
- Render Pipeline: Built-in.

# Game Mechanics
## Core Gameplay Loop
- Verified six clips per city; append exactly 15 new distinct transcripts/clips each to Lagos, Abuja and PortHarcourt. Final count: 21 each, 63 total, including 18 preserved original clips and 45 new clips. This replaces the earlier request for 50 per city.
- Retain random shuffle without immediate clip repeats, current 2–4-second opening delay and 15–25-second gaps. One presenter speaks per segment; no overlapping speech or two-host dialogue required. Random shuffle does not promise strict voice alternation.
- Keep original male presenter Chris. Use one fixed stock female voice across all cities, proposed Sarah subject to current provider availability and user listening approval. Do not synthesize a claimed Nigerian accent without auditioning it.
- Balanced proposed allocation per city: preserve six original male clips, add four male and eleven female clips, giving ten male and eleven female segments. Each transcript has one fixed presenter assignment; do not double-produce every transcript in both voices.
- Preserve current eligibility: actual running, enabled/unmuted BackgroundMusicManager, visible HUD, no countdown, positive time scale and app focus. Music off hides the radio and stops speech. Sound preferences still silence voice independently.
- Preserve music ducking while speech plays, smooth restoration during gaps/stops, city-change cancellation and clean scene lifecycle. No soundtrack or SFX volume changes in this content pass.
## Controls and Input Methods
- Legacy Input Manager controls unchanged. No new buttons, voice-selection menus or input systems.

# UI
No layout changes. Retain Canvas_HUD/RadioIndicator, station label and speaking equalizer.
```
[existing counters and word panel]
[radio icon | CURRENT CITY RADIO | equalizer]
[unobstructed running track]
```
The equalizer reflects actual speech, regardless of presenter. No host portraits or new font assets.

# Key Asset & Context
- Verified Assets/Script/Audio/CityRadioBank.cs entries contain id, transcript and AudioClip; banks each contain six valid referenced clips. Add serialized presenter/voice provenance metadata without breaking existing entries or IDs.
- Existing bank assets: Assets/Audio/Radio/Banks/Lagos.asset, Abuja.asset and PortHarcourt.asset. AssetDatabase dependency inspection confirms each depends on CityRadioBank.cs and its six clips; CityRadioBank.cs has three direct asset dependents, CityRadioController.cs has Home as one direct asset dependent.
- New clips: Assets/Audio/Radio/{Lagos,Abuja,PortHarcourt}/{City}_07.wav through {City}_21.wav, keeping existing _01 through _06 untouched. Prefer entries _07–_10 for the four new male clips and _11–_21 for the eleven female clips; shuffle retains mixed playback.
- Modify Assets/Script/Audio/CityRadioBank.cs only if needed for stable presenter/voice metadata; initialize existing entries as Chris/male. No expected controller, BackgroundMusicManager or scene edits.
- Modify the three banks in place, preserving GUIDs and Home references. Record exact transcripts, speaker assignments and provenance under Assets/Audio/Radio/Content/; update RadioSamples.md without replacing its original transcripts.
- Current CityRadioController uses AudioClip playback and already requests music ducking through BackgroundMusicManager.SetRadioSpeaking. Keep this integration intact, including existing volume/settings gates.
- At implementation time verify the actual generation model/tool and both stock voices. Generation tooling is not verified for this planning turn; no promise of generated output until it is available. If unavailable, report the blocker and request supplied clips rather than fabricate silent placeholders.
- Generation account commercial/distribution rights remain a release prerequisite; user approval of a voice sample does not establish licensing.

# Implementation Steps
1. **Recheck assets and finalize 45 transcripts with fixed speaker assignments.**
   - Files: three banks, CityRadioBank.cs, existing Content notes and new reviewed transcript manifest.
   - Read all existing transcripts; avoid paraphrased duplicates. Cover varied everyday topics: Lagos neighbourhoods/markets/coast/creative life, Abuja parks/boulevards/communities/food, Port Harcourt greenery/waterfront/neighbourhoods/food. Avoid unverifiable live news/weather, stereotypes, unsafe running advice and celebrity voices.
   - Recheck dependencies and reverse dependents before each edit. Confirm stock voice availability and generation account usage rights status.
   - Dependency: approval of this extension.
2. **Audition one new female sample before bulk production.**
   - File: one intended female clip from the planned 45, with its final ID/transcript, not an additional duplicate asset.
   - Use the proposed stock voice consistently; verify nonempty waveform/duration and offer a listening preview to the user. Target approximately 8–15 seconds and clarity with the existing male voice/music. Stop for sample feedback before producing the remaining female batch; use a different fixed female voice if requested.
   - Dependency: Step 1 and available speech generation provider. Existing Chris samples remain the approved male reference.
3. **Generate the other 44 new clips and retain provenance.**
   - Files: _07–_21 clips in each city's folder and Content manifest/notes.
   - Generate twelve male and thirty-three female additions total, counting the female sample from Step 2. Use independent generation batches where supported; never overwrite existing clips. Record failed/unfinished jobs and retry only missing outputs.
   - Check duration, channel count, nonzero signal, peaks/clipping and relative loudness. Listen/review regional words and Pidgin; do not label waveform checks as listening approval. Keep compressed mono import settings appropriate for Android and measure expanded memory/load cost.
   - Dependencies: Steps 1–2; approval of female sample.
4. **Append new entries to the three existing banks.**
   - Files: three bank assets, optional CityRadioBank.cs metadata addition and Content notes.
   - Preserve six original entries, their IDs/transcripts/clips and GUIDs. Append 15 complete new entries, each with its final transcript, clip and fixed presenter assignment; do not register missing clips as complete.
   - Keep runtime shuffle behavior unchanged; metadata is for traceability, not strict host alternation. No new Resources loading, runtime TTS, packages, scene layout or mixer changes.
   - Dependencies: Step 3 and per-asset impact checks.
5. **Verify expanded radio and report.**
   - Files: new audio/data/metadata only unless a directly attributable issue warrants revised scope.
   - Check all counts and unique IDs/transcripts, audition both voices and verify city-specific playback in separate fresh runs. Rebind test references after Home scene reload rather than reuse destroyed AudioSources, which limited earlier multi-city tests.
   - Exercise music mute/unmute, Sound off, pause/resume countdown, failure/retry, home/reload and city selection; verify speech stops, icon hides as appropriate and ducking releases. Preserve gap timings and no overlap.
   - Dependencies: Step 4.

# Verification & Testing
- Data: exactly 21 complete entries per city and 63 total; 45 new audio files; each bank has ten male and eleven female assignments under the proposed allocation. Original 18 clips/references remain untouched.
- Content: unique city-relevant original transcripts, no repeat by light rewording; consistent two fixed voices, pronunciation review and clear relative loudness. No missing/empty clips, accidental doubled presenters or clipped peaks.
- Shuffle: all 21 usable entries appear before refill; no adjacent clip repeats across refills; correct-city pool only. Voice order remains shuffled, not strictly alternating.
- Runtime: observe a male and female clip during eligible runs; no audio outside running, no overlapping sources, music mute hides sign and stops voice, ducking restores after clips/stops. Test each city in a fresh Home run and reacquire references after reloads.
- Performance: compare audio memory/build size and first-play hitches before/after. No Android performance claim without a device capture; avoid loading all expanded uncompressed audio unnecessarily if measured costs require a narrowly scoped import change.
- Final report: list added clips/presenter allocation and actual verification. Distinguish generation complete, waveform checks, listening approval, Play Mode tests and Android checks. Report generation/licensing blockers explicitly.

---

# Current Extension: Optional Timed Radio Subtitles
This is the current requested scope. User selected **Short timed phrases** rather than estimated timing or whole-clip captions. Preserve historical work; do not rerun older builders or regenerate audio. This extension supersedes earlier no-subtitle UI constraints only.

# Project Overview
- Game Title: Ningo Run.
- High-Level Concept: Add readable optional captions for the fictional city radio during actual speech, allowing players to follow English/Pidgin dialogue without a large block of text distracting from running.
- Players: Single player.
- Inspiration / Reference Games: Existing Ningo radio/HUD; compact dialogue captions in action games.
- Tone / Art Direction: Existing emerald-accent uGUI styling, white text on a subtle dark backdrop; preserve the recorded English/Pidgin wording.
- Target Platform: Android; preserve other build compatibility.
- Screen Orientation / Resolution: Preserve existing portrait-responsive CanvasScaler and safe-area behavior; validate actual narrow/tall phone layouts.
- Render Pipeline: Built-in.

# Game Mechanics
## Core Gameplay Loop
- Running, lane switching, jumping, sliding and collection remain unchanged. Short caption phrases support accessibility and MDA atmosphere while minimizing interference with challenge and obstacle readability.
- Show only the cue belonging to the currently playing entry, based on the AudioSource playback position. Cue intervals are manually authored and listening-verified, not proportional word-count estimates.
- Preserve the scene's verified 10-second post-clip gaps, initial 2–4-second delay, shuffle, music ducking and existing audio eligibility/mute rules.
- Hide subtitles in quiet gaps, when disabled, when no valid cue exists, and immediately on stop, pause, countdown, failure, completion, backgrounding, city change or scene disable.
- This is captions for existing playback, not a new silent-radio scheduler. Sound/music mute currently stops radio; subtitles stop with it. Caption-only playback while muted is outside scope.
## Controls and Input Methods
- Preserve Legacy Input Manager and existing touch/swipe and keyboard controls.
- Add one Settings button-style pill toggle, Radio Subtitles. Persist RadioSubtitlesEnabled with an enabled default for new users; disabling captions must not change audio or other preferences.

# UI
```
Gameplay HUD (bottom portion)
  [ Short spoken phrase, at most two lines ]
  [ radio icon | CURRENT CITY RADIO | bars ]
  [ existing power-up progress bars       ]

Settings
  existing Sound / Music / Vibration rows
  [ Radio Subtitles             ON / OFF ]
  existing Language / other controls
```
- Add Canvas_HUD/RadioSubtitles above the existing bottom-center RadioIndicator (currently 220x30, bottom anchor/pivot .5/0, position 0/100). Begin with bottom-center placement above its top plus a small gap; use parent-width margins rather than a fixed oversized width.
- Inspect toast overlap: ToastUI_Background exists lower in the HUD. Move only the new caption panel to an inspected free band if needed; do not relocate unrelated HUD elements silently.
- Reuse a suitable existing TMP font/shared material; choose fixed readable font size, disable runtime AutoSize, wrap to at most two lines. Split long cues during authoring rather than truncate dialogue or shrink text excessively.
- Use non-raycasting graphics and CanvasGroup blocksRaycasts/interactable=false. Keep hidden by default outside speech. Update TMP text only on cue changes, never rebuild the same string every frame.
- Reuse existing Settings pill on/off sprites and row styling. Inspect row dimensions, fit and modal layering before adding/re-spacing a row. No Settings rebuild or new menus.

# Key Asset & Context
## Verified current state
- Assets/Script/Audio/CityRadioBank.cs has CityRadioEntry fields id, transcript, clip; there are no cue timings.
- Actual loaded bank assets each contain **6 entries**, 18 active clips total. Historical 21-entry claims do not match current serialized data. Additional _07–_21 WAV files are not in the current playlist. Do not append them as an incidental subtitle change.
- CityRadioController exposes IsRunning, IsSpeaking, StationLabel, CityKey and StatusChanged, but no current entry/playback-position accessor. It stops the AudioSource on ineligibility and mutes, and uses music ducking.
- Home scene _quietGap is (10,10); script default remains (15,25). Preserve actual scene timing rather than overwrite it from class defaults.
- Canvas_Settings uses SettingsController with direct SoundRow, MusicRow, VibrationRow, LanguageRow, GuestRow and other buttons. Existing NingoPillToggle uses a Button and Kind enum Sound/Music/Vibration with PlayerPrefs persistence.
- NingoPillToggle is also referenced by recovery scenes. Append a new enum value without changing existing numeric values; do not edit recovery scenes.
- Dependency/dependent inspection via AssetDatabase confirmed the three banks depend on CityRadioBank and their six clips; Home references the banks/controller. Recheck impact before each asset edit. No test assembly definitions found in Assets.
## Files to create/modify
- Modify Assets/Script/Audio/CityRadioBank.cs: serializable RadioSubtitleCue with startSeconds, endSeconds, text, and an optional cue array on each entry. Preserve existing IDs/transcripts/clip GUIDs and entry order.
- Modify Assets/Script/Audio/CityRadioController.cs minimally: expose read-only CurrentEntry and PlaybackTimeSeconds; clear entry on stop/city change/finished speech before notifying listeners, including Sound-off paths. Do not alter scheduler or music behavior.
- Create Assets/Script/Ui/CityRadioSubtitles.cs: cached references, cue lookup from actual source time, setting and status binding, immediate cleanup and text-on-change presentation.
- Modify Assets/Script/Ui/NingoPillToggle.cs: append RadioSubtitles kind, explicit preference mapping and a narrowly scoped setting-change notification. Preserve existing Sound/Music/Vibration behavior and serialization.
- Modify Assets/Scene/Home.unity: wire caption panel/controller and add RadioSubtitlesRow to existing Settings; keep indicator and all audio references/settings intact.
- Modify Assets/Audio/Radio/Banks/Lagos.asset, Abuja.asset, PortHarcourt.asset only to add verified subtitle cue arrays for their currently referenced 18 clips.
- Add Assets/Audio/Radio/Content/RadioSubtitles.md: per-clip cue provenance, listening verification status and timing-maintenance instructions. Additional unused clips remain explicitly outside coverage.
## Timing production boundary
- Manually audition each **current final recording**, including regenerated Abuja files; derive phrase start/end times from listening, supported waveform/audio review and optional supplied timestamped SRT/VTT.
- Automated duration/waveform checks cannot establish speech alignment. If available tools cannot audition or obtain verified timestamped audio, report the blocker and request user-supplied reviewed cues or an approved review workflow. Do not mark estimated timing as manually synchronized.
- Keep city spelling Abuja in visible captions; the speech-generation spelling Ah-BOO-jah is a pronunciation aid, not display text. No translation or Localization package installation in this scope.

# Implementation Steps
1. **Recheck integration, layout and cue-authoring capability.**
   - Files: Home, radio scripts/banks, NingoPillToggle, active recordings and Settings/HUD font references.
   - Recheck dependency impact, exact active playlist, source playback/load behavior, Settings layout, toast/powerup rectangles, TMP glyph coverage and test tooling. Establish an actual listening/timestamp review route before claiming synchronized captions.
   - Dependency: approval of this extension.
2. **Add cue schema and playback accessors.**
   - Files: CityRadioBank.cs and CityRadioController.cs.
   - Add backward-compatible cue data with positive sorted non-overlapping ranges bounded by clip duration. Expose current entry/time without exposing mutable AudioSource controls; reset state on every stop path, including completed playback and Sound-off.
   - Preserve scheduling, scene 10-second gaps, shuffle, volume, eligibility and ducking. Empty cue arrays safely mean no caption.
   - Dependency: Step 1.
3. **Implement optional subtitle presentation and preference.**
   - Files: new CityRadioSubtitles.cs, NingoPillToggle.cs.
   - Cache references; subscribe/unsubscribe cleanly. Initialize saved preference on enable; live setting changes clear/show the current valid cue without restarting audio.
   - Look up cues using actual PlaybackTimeSeconds; reset cue cursor on entry changes or backward playback time. Handle missing data, gaps between cues, stopped playback and disabled UI safely. No per-frame LINQ, string splitting, PlayerPrefs writes, scene searches or allocations.
   - Dependency: Step 2.
4. **Author and approve a synchronized sample, then complete the active playlist.**
   - Files: three banks and RadioSubtitles.md.
   - Begin with one actual Abuja clip to review phrase length, spelling, readability and alignment. Obtain listening/visual feedback before completing all 18 active entries.
   - Manually verify remaining cue timings against final clips. Reconstruct normalized cue text and compare against transcript for omissions/duplicates. Record unverified clips honestly; do not ship fabricated precise timestamps.
   - Dependencies: Steps 1–3 and working timing review route.
5. **Wire compact gameplay panel and Settings row.**
   - File: Home.unity.
   - Add caption panel above indicator with appropriate anchors/padding and existing TMP font; add saved toggle using existing pill assets and stable enum assignment. Adjust only necessary Settings spacing after checking fit.
   - Confirm Canvas/EventSystem/raycaster wiring for the toggle, non-interactive captions, hidden lifecycle state, safe-area fit, two-line readability and no toast/progress-bar overlap.
   - Dependencies: Steps 3–4, refreshed scene impact check.
6. **Verify full gameplay and report coverage.**
   - Files: scoped subtitle/data/settings assets; tests only if existing supported infrastructure is available. Otherwise use documented read-only and manual verification without introducing unrelated test packages.
   - Run each city in fresh Home flows; reacquire scene references after reload. Review timing against actual speech and test all lifecycle/settings transitions.
   - Dependencies: Steps 2–5.

# Verification & Testing
- Data: every active clip has sorted valid cue ranges, no unintended overlap or negative/zero durations, range end within clip duration, matching stable entry ID and reviewed wording. Confirm coverage is 18 current entries, not the 63 files on disk.
- Alignment: listen/view every covered recording, both start/end and phrase transitions; validate to roughly 0.2-second alignment where practical. Report exact listening-reviewed coverage separately from static validation. Any regenerated recording invalidates prior cue timing until re-reviewed.
- Presentation: cue changes once at boundary; silence between cues hides text, no last-phrase carryover into the 10-second gap; no whole-transcript block, truncation or mid-word wrapping caused by overflow.
- Lifecycle: stop/pause/resume countdown/fail/complete/home/scene disable/background/city change immediately clears captions. New clips cannot show old-city text; no duplicate event listeners after reload.
- Preferences: enabled default when key absent, persistent on/off across restart, live toggle behavior, existing Sound/Music/Vibration unchanged. Muted radio does not show fabricated captions or start its own timer.
- Layout: inspect narrow/tall portrait and safe-area layouts in actual gameplay; no track-critical, toast, powerup or counter obstruction; captions never intercept swipes. Settings row and modals fit and remain clickable.
- Performance: no steady-state managed allocations from subtitle logic; text changes only when necessary, fixed font size and shared font material. Verify cue lookup costs and Canvas rebuilds; only introduce a dedicated child Canvas if measured rebuild cost warrants it.
- Regression: radio retains 10-second post-clip gaps, first-play delay, one voice at a time, existing city shuffle, ducking and compressed audio import settings.
- Final report: list exact edited files, active caption coverage, timing-review limitations and tests actually run. Android playback/layout/performance require real-device checks; do not claim them from Editor inspection.

---

# Current Extension: Gentle Alternating Sideways Road Bend
This is the current requested scope. User chose a slowly alternating right/left visual bend. The subtitle extension above was cancelled by the user and must NOT be implemented. Preserve all existing audio, HUD, scenery, traffic and controller behavior.

# Project Overview
- Game Title: Ningo Run.
- High-Level Concept: Make the distant road and streetscape gently sway between right and left using the existing curved-world rendering system, while keeping nearby running lanes and collision gameplay straight.
- Players: Single player.
- Inspiration / Reference Games: Existing Ningo curved horizon and stylized endless-runner road presentation.
- Tone / Art Direction: Subtle stylized 3D bend, not sharp turns or camera shake.
- Target Platform: Android; preserve other build compatibility.
- Screen Orientation / Resolution: Preserve current portrait-responsive gameplay framing and resolution; test narrow/tall phone aspects.
- Render Pipeline: Built-in, existing owned curved shaders.

# Game Mechanics
## Core Gameplay Loop
- Existing run, lane switching, jump/slide and collection remain unchanged. Visual variation reinforces MDA atmosphere without adding steering demands or obscuring hazard reaction windows.
- Preserve downward curvature. Add a bounded horizontal vertex displacement beyond a separate near-player straight zone; all supported road/scenery/vehicles/pickups/world-space letter shaders use identical global parameters.
- Start at neutral and smoothly increase to right, return through neutral to left, then repeat. Proposed full right/left cycle: 40 seconds (20 seconds between opposite peaks); expose cycle duration and strength in Inspector.
- Initial proposed horizontal coefficient: 0.00025, with bend onset around 25 metres ahead and a smoothly bounded maximum displacement around 4 metres. These are starting tuning values, not final appearance claims. With quadratic distance shaping, displacement at 50m ahead is about 0.16m and at 100m about 1.4m before bounding.
- Keep local gameplay geometry/colliders and player/camera transforms unchanged. Near zone must cover the collision/reaction envelope at increased runner speeds; extend onset if high-speed testing requires it.
- Advance phase only during actual eligible running: moving active runner, active game, no countdown, positive time scale and focus. Freeze through pause/backgrounding; reset neutral on a new run/city/disable without causing a visible jump during an active run. Smoothly settle toward neutral outside gameplay where the street remains visible.
## Controls and Input Methods
- Preserve Legacy Input Manager and existing controls. No new steering controls, packages or input changes.

# UI
No new UI. Do not bend Canvas HUD, previews, counters or the radio indicator.
```
   distant road gently bends right / left
   shops, trees, vehicles and letters follow
               straight near lanes
                     runner
                unchanged HUD
```

# Key Asset & Context
## Verified context
- Assets/Script/Environment/CurvedWorldManager.cs uploads _CurveStrength, _CurveStartOffset and _CurveOrigin; Home has curveStrength=0.00047, curveStartOffset=15, referenceOrigin=Player. Preserve these scene values rather than replacing them with class defaults (0.0006).
- Current five owned shaders only change worldPos.y quadratically. Sideways curvature does not exist yet.
- CurvedStandard has generated surface-shader shadow support (addshadow); other shaders require pass-specific inspection. All vertex/shadow paths must remain coherent.
- RoadSpawner calls ApplyCurvatureToGameObject for new/replaced tiles. Manager caches shared material variants and excludes Canvas/player/preview objects. Preserve material sharing and GPU instancing.
- Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader is an Assets-owned shader already containing NINGO_CURVED_WORLD and downward curvature for world-space letters. Its opt-in path must receive the sideways offset; UI without the keyword stays untouched. No package-source edits.
- Manager reference-origin selection can retain an old character after runner changes; verify active runner binding for this feature and correct only if a demonstrated integration fault requires it.
- Manager is referenced by Home and recovery scenes. Modify Home configuration only, not recovery scene assets. Shader impacts are potentially project-wide; default new horizontal strength to zero in code so other scenes retain prior appearance until enabled.
- Dependency tool was unavailable in this planning turn; AssetDatabase reads inspected manager scene dependents. Before implementation edits, inspect dependencies/reverse dependents for each shader, include and Home; do not presume shader use is limited to roads.
## Approaches
- Selected: common shader-space bend driven by manager globals. Reuses current system, is lightweight and does not require rebuilding lane/collision architecture. Risks: shader coverage, displaced-renderer culling, mesh tessellation and visual/physics mismatch far ahead.
- Alternative: truly curved world paths and rotated road tiles. Provides physical turns but requires coordinated spawner, player, camera, vehicle and collision changes. Not selected for a subtle visual effect.
## Files to modify/create
- Modify Assets/Script/Environment/CurvedWorldManager.cs: Inspector enable/strength/start distance/cycle/max displacement controls; cached shader IDs, bounded phase and lifecycle handling; zero globals on disable/destroy without a duplicate manager clearing the active manager's globals.
- Create Assets/Shaders/CurvedWorldBend.cginc for shared downward and horizontal displacement math. Preserve original vertical expression exactly; use a smooth bounded horizontal expression with zero displacement and derivative at onset. Avoid hard distance clamps that produce a visible kink.
- Modify Assets/Shaders/CurvedDiffuse.shader, CurvedStandard.shader, CurvedStandardAlpha.shader, CurvedUnlit.shader and CurvedUnlitAlpha.shader to use shared math while preserving rendering, transparency, instancing and shadow semantics.
- Modify Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader only within its existing opted-in world-curvature path; keep mask/UI/font behavior unchanged.
- Modify Assets/Scene/Home.unity only to enable/tune this effect on its existing manager; no new scene or replacement manager.
- If required by demonstrated culling: add narrowly scoped renderer-bounds handling in manager or a small owned companion. Use the inspected Unity 6000.4.6f1-supported bounds API, preserve original bounds, cover both lateral extrema and existing vertical displacement, handle pooled/moving/skinned objects explicitly and restore on cleanup. Do not mutate shared mesh bounds or disable culling globally.
- No planned player, collision, road-spawner, traffic, lighting, audio or package changes. Additional shader/material compatibility issues require an explicit targeted plan refinement rather than broad material conversion.

# Implementation Steps
1. **Audit dependencies, shader coverage and baseline view.**
   - Files: manager, five curved shaders, opted-in TMP shader, Home; read-only RoadSpawner/player/game/UI/city lifecycle and actual camera.
   - Inspect dependencies/dependents before each edit. Capture actual gameplay views across cities and examine visible materials, world-space letters, renderer bounds and road tessellation. Record current downward bend and material sharing baseline.
   - Dependency: approval of this extension.
2. **Implement common displacement and safe manager controls.**
   - Files: new CurvedWorldBend.cginc and CurvedWorldManager.cs.
   - Cache new global IDs. Compute one slow phase per frame, not per renderer. Use sine or equivalent smooth cycle with bounded elapsed phase and gradual run transition envelope; clamp invalid cycle duration and amplitude inputs.
   - New effect disabled by default for existing scenes. Keep vertical global values unchanged; safe zero horizontal globals on active manager cleanup. No per-frame allocations, material clones, scene-wide searches or renderer rescans.
   - Dependencies: Step 1.
3. **Integrate all owned curved rendering paths.**
   - Files: five owned shaders and opted-in TMP shader.
   - Apply identical world-space offsets before converting to object/clip coordinates. Preserve existing properties, texture sampling, blending, masking, shader targets and instancing. Ensure shadow caster deformation is consistent where supported.
   - Compile in current Built-in target and check transparent foliage/glass, particle effects, road meshes, vehicles, pickups and world-space letters. Inspect lighting distortion before adding any normal correction; avoid unrelated rendering overhaul.
   - Dependencies: Step 2 and shader impact checks.
4. **Enable subtle Home configuration and validate extremes.**
   - File: Home.unity and, only if necessary, scoped bounds handling.
   - Start with 40-second cycle, coefficient 0.00025, 25m straight-zone onset and approximately 4m smooth maximum displacement; preserve vertical strength 0.00047 and onset 15m.
   - Preview right/neutral/left extremes and tune for readability at standard/increased speeds and glider camera. Avoid discontinuities at tile seams, collapsing visibility, hard edge kinks and noticeable deformation near collision range.
   - Test culling at both lateral extremes; if objects disappear, implement measured bounded overrides rather than marking everything visible or expanding shared meshes. If road topology looks angular, reduce bend before proposing mesh reauthoring.
   - Dependencies: Steps 2–3.
5. **Test lifecycle, fairness, visual result and device cost.**
   - Files: scoped manager/shaders/Home only; temporary supported verification harness if available, clean up afterwards.
   - Full actual Home run per city; inspect spawned and replaced tiles, collisions, moving junction taxis, letters and pickups. Test pauses/countdowns, backgrounding, crash/retry/continue, city/runner switching and scene reload.
   - Compare baseline/updated material counts and frame timings; test Android if a device is available. Show actual gameplay captures, not only shader-global values.
   - Dependencies: Step 4.

# Verification & Testing
- Mathematical checks: zero horizontal strength reproduces prior rendering; horizontal displacement zero at/before onset, smooth first derivative, bounded at configured maximum; opposite phase extrema give symmetric left/right offsets; invalid durations cannot divide by zero.
- Phase: slow continuous alternation, one full cycle around 40 seconds of active running; no phase accumulation while paused/backgrounded/counting down; gradual start/settle and neutral reset without a visible jump in eligible running.
- Rendering: correct downward curve preserved, all supported environment/pickups/letters move consistently, no seam tears or disconnected roadside objects; no shader errors/magenta objects. Test solid/transparent objects and shadows at both extremes.
- Culling: no road/scenery/vehicle/letter pop-out at extreme offsets; conservative overrides if needed do not leak across pooling or scene cleanup or mutate shared source meshes.
- Gameplay: player, camera, lane spacing, collision transforms, tile positions and traffic routes unchanged. Road stays visually straight in reaction/collision zone at increased speeds; no misleading nearby obstacle positions or hidden escape lanes.
- UI: HUD/previews/radio unchanged and uncurved; no subtitles added.
- Lifecycle: run, pause, resume countdown, hit/fail, revive/retry, completion, city/runner switch, app background and repeated reload; no stale origin, duplicate manager, leaked shared variants or nonzero globals after cleanup.
- Performance: constant-size shader-global updates, no new steady-state managed allocations/materials, no realtime lights; compare actual Android frame/culling cost if device available. Do not claim device performance from Editor checks.
- Deliverable: list exact files/Inspector tuning controls, show neutral/right/left gameplay views and distinguish static, Play Mode and Android verification actually completed.

---

# Current Extension: Full Babylon.js Web Rebuild with Unity Visual and Functional Parity
This is the current requested scope. Earlier extensions are historical context, not instructions to rerun builders or introduce unimplemented features. The user selected Babylon.js, requires the same game design, functionality and UI, and explicitly selected **React overlay** and **faster startup**. Preserve the Unity project as the reference and existing Android deliverable. Append this extension to the existing plan rather than replace historical records.

# Project Overview
- Game Title: Ningo Run.
- High-Level Concept: Rebuild the existing Nigerian-city 3D runner as a browser-native game, preserving its current artwork, screens, word-learning mechanics, progression and presentation. Remove the Unity runtime dependency from the web application, while preserving Unity as the comparison reference.
- Players: Single player; preserve existing account and leaderboard features where verified and browser-compatible. No new multiplayer.
- Inspiration / Reference Games: The actual current Ningo Run Unity build is authoritative; Subway Surfers/Temple Run are background references only, not permission to redesign.
- Tone / Art Direction: Same stylized 3D Nigerian cities, characters, UI art, colours, fonts, spacing and animations. No AI-generated replacements or simplified generic roads/characters as final assets.
- Target Platform: Browser-native web application, mobile-first Android Chrome and iOS Safari, with desktop keyboard/mouse support. PWA installation/caching is additive and browser-dependent; do not promise fully offline network features.
- Screen Orientation / Resolution: Current Unity orientation is Portrait. Most child canvases use a 390x844 design reference; root/completion include 390x840. Match each canvas's measured scaling/anchors instead of globally assuming one resolution. Wide displays retain portrait composition with an appropriate surrounding background rather than redesigning the game landscape-first.
- Render Pipeline: Source uses Unity Built-in, Unity 6000.4.6f1. Destination uses Babylon.js WebGL2 as the baseline, with no mandatory WebGPU dependency. Pin exact compatible Babylon/React/build-tool versions during implementation after verifying runtime/tooling requirements.

## Scope and acceptance contract
- Full migration is the final scope; a first vertical slice is a proof milestone, not permission to omit remaining screens/cities/functions.
- Preserve all current user-visible functions and reachable states, including loading, empty/error, locked/unlocked, selected, muted and modal states. Distinguish reachable features from legacy scripts and unused assets.
- User-approved exception: keep Splash/Welcome artwork and sequence, but advance based on readiness rather than reproduce intentional artificial waiting. Preserve gameplay countdowns, intro, jump/slide, radio and retry timing unless a specific deviation is separately approved.
- Exact parity means measured visual/functional equivalence against approved Unity captures and test flows, not a claim of mathematically identical pixels across browser text rasterization and different renderers. Any visible rendering compromise must be recorded and reviewed, not silently accepted.
- Do not add subtitles: the previous subtitle extension was cancelled. Only port subtitle functionality if current live discovery proves a later user-approved implementation exists.
- Do not auto-fix historical Unity bugs or add missing monetization features under the label of parity. Record issues and preserve supported intended behaviour; any change needs explicit review.
- Do not overwrite existing Unity scenes/prefabs/imported assets, modify package sources, change input/render settings, publish a site, spend money, provision services or migrate production data as part of local implementation approval.

# Game Mechanics
## Core Gameplay Loop
Preserve Home/city/character/language/level selection -> intro/countdown -> three-lane running -> jump/slide/dodge -> collect coins, gems, letters and powerups -> word/level progression -> completion or fall/retry -> next level/home. Preserve tutorial gating, star/unlock/reward calculations, high scores and completed word translations. MDA rationale: maintain the existing challenge, competence and cultural atmosphere rather than trade fidelity for a superficially faster but different game.

### Required systems
- Player movement/animation, camera intro/follow/glider views, fall/knockback, invulnerability and skateboard protection.
- Road spawning and obstacle density, city starters, authored scenery/housing, junction routes and other currently active vehicles, monkey interactions, birds and environmental animation.
- Coin magnet, skateboard/double tap, glider/sky pickups, other verified upgrades and inventory effects. A UI label alone is not proof a powerup is implemented; follow its actual handlers.
- Word dictionary/API, grapheme-sensitive collection, correct/wrong letter rules, category filtering, progression, translations and completion display.
- Coins/gems, inventory, daily gifts, daily/weekly missions, level stars/unlocks, runner selection, settings and saved session/progress.
- Music, SFX, city radio/transcripts/banks, shuffle/gaps, ducking, speaking equalizer, eligibility and mute/background gates.
- Backend-bound auth, dictionary and leaderboard functions, with explicit integration adapters and availability/error states.

## Controls and Input Methods
- Verified source activeInputHandler=0 (Legacy Input Manager); use its actual control behaviour as reference, do not change Unity's input backend.
- Destination: DOM Pointer Events/touch gestures and keyboard commands translated into game actions. Read actual source thresholds, key mappings and tutorial exceptions before implementation.
- Match lane switch/jump/slide/double-tap actions and disable actions in equivalent locked/countdown/modal states.
- Scope touch-action restrictions to the game surface, permit native scrolling in menus, support pointer cancellation, prevent duplicate touch/click processing, and avoid triggering tap-to-play through interactive controls.
- Pause on visibility/focus loss, cap time deltas on resumption, preserve explicit resume-countdown behaviour and prevent background progress/expired timers.
- Vibration/fullscreen/audio startup are browser-constrained: use feature detection, unlock audio on a user gesture and record unsupported behaviour. Do not fake successful browser haptics.

# UI
## Selected architecture and alternative
- Selected: React + TypeScript DOM overlay over a Babylon canvas. Reuse exact sprite assets/fonts and measured Unity RectTransform/layout rules. Benefits: forms, scrolling, accessibility, and a lightweight splash/onboarding shell before engine download. Costs: translate anchors/pivots, layering, render-texture previews and input interception carefully.
- Alternative considered: Babylon GUI for all screens. Benefits: one canvas coordinate/rendering system. Costs: engine-dependent menu startup and more work for native text input/accessibility. User selected React.
- Avoid React state updates for per-frame 3D transforms. Use a presentation adapter with events and bounded HUD updates; animate progress/preview elements independently where appropriate.

## Screen inventory discovered in Home
1. Canvas_Home: TopPanel, side buttons, bottom buttons; same tap-to-play surface and city/character backdrop.
2. Canvas_HUD: TopPanel, Countdown, toast/flying coins, magnet/glider/skateboard/monkey progress bars, RadioIndicator.
3. Canvas_Pause: level/language pill, word slots/count, resume/restart/home, sound/music controls.
4. Canvas_CompleteLevel: heading/stars, dancing preview, stats, word translations, next-level/bottom/audio controls, confetti.
5. Canvas_Daily_Gift: gift scroll viewport, heading and back.
6. Canvas_Fail: word progress/stats, language presentation, try again/home/audio controls.
7. Canvas_Activate_Mystery_Box: existing activation overlay and double-tap behaviour.
8. Canvas_Missions: daily/weekly tabs, mission cards, footer and scroll content.
9. Canvas_Shop: coins/gems tabs/cards, confirm/processing/success/failure modals.
10. Canvas_Activate_Rewards: existing activation overlay.
11. Canvas_Leaderboard: scroll/podium/current-user/header/back, real/empty/error states.
12. Canvas_Activate_Speedstar: existing activation overlay.
13. Canvas_Characters: stage/animated previews, thumbnails, info/footer and selection/unlock flow.
14. Canvas_PowerUps: currency header and magnet/shield/double-coin rows; inspect actual enabled effects.
15. Canvas_Settings: sound/music/vibration/language/guest, how-to-play, support/privacy/terms and language/tutorial modals.
16. Canvas_Level: city plaque, header, level map/nodes/scroll view and locked/unlocked/star states.
17. Canvas_Retry: countdown ring, video availability, coin/gem continue cards, end run.
- Additional reference scenes: Splash and UserInfo. UserInfoController currently describes login/guest/signup -> language -> welcome; inspect actual scene wiring before treating older username/age panels in UImanager as reachable onboarding.
- Capture high-score/new-score and dynamically generated dialogs if discovered; the 17 canvases are not a guarantee of complete state coverage.

## Reference composition (schematic only; measured Unity captures govern positions)
```
Splash/Welcome: [exact background / logo / welcome artwork / readiness bar]
Onboarding:    [exact heading / login or guest] -> [language selection] -> [welcome]
Home:          [profile / currencies / language]
               [side menu] [same 3D street + idle runner] [side menu]
               [same bottom controls / tap-to-play]
Gameplay:      [pause / counters / word objective]
               [same camera, curved road, hazards, runner]
               [powerup bars / city radio at measured positions]
Modal/menu:    [existing background / heading / back]
               [same cards, tabs, text and scroll content]
               [same footer and action buttons]
```
- Export all image states, colours/opacity, sprite rects/pivots/borders, sliced-image rules, font styles/weights/sizes/line spacing, alignment, transforms and active conditions.
- Capture post-layout and runtime-mutated state, not YAML alone; persistent button events and runtime-added listeners both matter.
- Match safe-area/masking/scroll physics where practical, z-order, overlay dismissal and transitions. Use native buttons/forms under identical styling; don't replace working interactions with flat screenshots.
- Inspect raw font source licensing/web formats; TMP SDF atlases are not web fonts. Match actual glyph coverage/diacritics and fallback, avoiding generic-font substitution without review.
- Preserve 3D character/celebration previews using Babylon cameras/render targets or a scoped viewport strategy; do not replace animated previews with static art in the final deliverable.

# Key Asset & Context
## Verified state and caveats
- Three scene assets and enabled build scenes: Assets/Scene/Splash.unity, UserInfo.unity, Home.unity. Home contains gameplay as well as menus. Legacy SceneLoader refers to GameScene, which is not among those enabled scenes; do not port that stale flow blindly.
- AssetDatabase recursive dependency counts inspected: Home=810, Splash=21, UserInfo=44. These are dependency counts, not network sizes or profiler findings.
- Earlier project totals: 140 prefabs, 500 textures, 156 meshes and 122 audio clips; re-inventory shipping references, not everything under Assets/Recovery or examples.
- Source player serialized settings inspected: speed=10, laneDistance=2.3, switchDuration=.25s, jumpForce=9, gravity=22, slideDuration=.8s, hitRadius=.5, collisionCheckInterval=2, magnetDuration=10s, magnetRadius=16, skateboardDuration=15s, gliderDuration=10s and gliderHeight=7.5. These are initial serialized baseline values, not proof of later runtime tuning or all runners' configurations.
- CityManager has Lagos, Abuja, PortHarcourt, each with five serialized tile references including the junction twice. Preserve duplicate reference weighting rather than deduplicate the random selection distribution; inspect actual starters and runtime assignments.
- CurvedWorldManager inspected with vertical strength .00047/onset15 and horizontal enabled, strength .00025/onset25/cycle40s/max4/transition2s. Preserve owned shader math and lifecycle after reading complete implementation.
- WordManager matches grapheme clusters with tone marks, duplicate counts and any-order/in-order logic; it requires the API dictionary, not invented substitute words.
- UserInfoController uses ningoafrica.app for native defaults and a same-origin web API option; preserve actual API contracts and test CORS/credentials on the eventual host.
- LeaderboardController attempts UGS through UGSManager; current serialized identity configuration and browser API availability remain to be inspected. Do not assume custom login identity equals Unity anonymous identity.
- NingoShopPanel supports real Unity IAP or simulation; useRealIAP is false by class default only. Confirm live scene configuration. ShopController also contains legacy TODO offers; follow actual button wiring.
- ContinuePanelController exposes RewardedAdProvider; actual provider binding is unverified. Simulated ads/purchases may be reproduced in isolated parity tests but never silently treated as real release transactions/rewards.
- Installed packages include glTFast 6.19.0, Addressables 2.7.4, Purchasing 5.4.4, uGUI 2.0.0, Test Framework 1.4.5. Installed importer does not establish a functioning GLB export pipeline; verify animation/skinning export capability before relying on it.
- No existing web-app/package root appeared in the inspected project-root directory listing. Node/package-manager/browser execution and model export tooling must be checked before implementation; unavailable tools are explicit blockers, not fabricated builds.

## Read-only Unity sources to trace fully
- Scenes and their referenced assets, including nested/dynamic UI.
- Assets/Script/Ui/{UImanager,UserInfoController,GameHUDController,NingoShopPanel,ContinuePanelController,CharactersController,SettingsController,DailyGiftController,MissionsController,LeaderboardController}.cs and all actual linked handlers.
- Assets/Script/SimplePlayerController.cs, SimpleCameraFollow.cs, GameStateController.cs; player/RunnerManager.cs and active input scripts.
- Assets/Script/Road/{RoadSpawner,RoadObstacleHider,...}.cs and all pickup/obstacle components; Assets/Script/Vehicles/ and Characters/Monkey/MonkeyThiefController.cs.
- Assets/Script/City/CityManager.cs, Levels/ and Words/ including TextUtils/dictionary/letter spawning and level difficulty formulas.
- Assets/Script/Environment/CurvedWorldManager.cs, Assets/Shaders/CurvedWorldBend.cginc and active curved shaders.
- Assets/Script/Audio/, Assets/Audio/Radio/Banks/ and referenced clips; actual scheduler settings over historical notes.
- Assets/Script/API/, UserSession.cs, SecureCredentials.cs, UGSManager.cs and IAP/IAPManager.cs. Never export secrets or local personal credentials into the web bundle.

## Proposed destination files (new, isolated; finalized after tooling audit)
- WebApp/ at project root, outside Unity Assets: package.json/lockfile, index.html, tsconfig.json, vite.config.ts, src/main.tsx, src/app/App.tsx.
- WebApp/docs/{parity-matrix,reference-captures,asset-provenance,api-contracts,performance-baseline,migration-status}.md.
- WebApp/src/content/: versioned schema, asset manifest, exported UI/layout/character/city/level/radio/game settings. Manifest maps Unity GUID + local fileID/object path to stable browser IDs.
- WebApp/public/assets/{ui,fonts,models,textures,audio}/: licensed extracted/exported copies only, original sources untouched; split shared/city/character/radio payloads.
- WebApp/src/ui/: layout/scale/safe-area bridge, screen router, exact-art reusable widgets and components for every inventoried screen/state.
- WebApp/src/game/: EngineHost, AssetLoader, GameState, InputController, RunnerController, CollisionSystem, AnimationController, CameraRig, RoadSpawner, CityController, TrafficSystem, PickupSystem, WordSystem, TutorialSystem, CurvedWorld, PreviewRenderer.
- WebApp/src/domain/: progression, level rules, missions, rewards, inventory, settings and currency transaction rules separated from rendering.
- WebApp/src/audio/: music/SFX/radio scheduling and browser lifecycle/audio unlocking.
- WebApp/src/services/: auth/dictionary/leaderboard/payment/ads adapters and versioned storage adapter; optional PWA/cache module.
- WebApp/tests/{unit,integration,e2e,visual}/ and separate test fixtures. Fixtures never replace production content or pretend to be live services.
- Only if required: new read-only-source export helpers under Assets/Editor/WebMigration/ and owned staging output under an explicitly named export location. Do not alter source scene/prefab/import settings to export. Check dependencies and reverse dependents via available dependency tooling or AssetDatabase before any existing-asset edit.

# Implementation Steps
1. **Freeze the actual reference, complete parity inventory and establish tooling.**
   - Files: WebApp/docs/, inventory/schema definitions and optional isolated editor exporter.
   - Verify Node/package manager, browser test tooling and glTF/skeletal-animation export; pin compatible versions and record commands. Inspect dependency/dependent impact before helper/config edits; no Unity package-source or project-setting changes.
   - Read all linked runtime scripts and persistent/runtime UI listeners; inspect Splash/UserInfo without leaving scene changes or user progress mutations. Record all reachable flows, exact serialized and live settings, data formulas, enabled service flags and missing integrations.
   - Capture every screen and key modal/error/selected state with identical fixture data; record font assets, sprites, coordinates, camera/render settings and animation timings. User reviews reference captures and parity matrix before broad UI replication.
   - Benchmark current web build if supplied: transferred bytes, first visible UI, first interactive menu, first playable run, steady/p95 frame timing and memory on named devices/network settings. Missing build/device capture is reported, not extrapolated from Editor.
   - Dependencies: approval of this extension; capture/export/tool capabilities verified.
2. **Create the reproducible asset and data pipeline.**
   - Files: content schemas/manifests, public/assets/, export helpers and provenance docs.
   - Extract exact sprite subrects/9-slice metadata and licensed raw fonts. Export existing character skeletons/clips, models, prefabs, scene starter compositions and animation attachments to GLB plus JSON behavioural metadata. If export unavailable, request supported tooling/source exports; don't substitute generated characters.
   - Preserve nested transforms, scales, coordinate handedness, camera FOV convention, collider shapes/offsets, layers/tags and spawn markers. Explicitly test skinned meshes, retargeted humanoid clips, root motion and bone attachment orientation.
   - Export runtime-generated level rules as equivalent tested TypeScript/data, preserve tile weighting and stable IDs. Export actual active radio playlists, not every unused WAV. Keep API dictionaries governed by their real contract.
   - Hash/version outputs, include provenance and validate missing references. Keep behavioural meshes/colliders separate from decorative geometry; don't bake moving traffic into static road assets.
   - Dependencies: Step 1.
3. **Build web shell and prove exact initial UI.**
   - Files: app/tooling, layout bridge, Splash/Welcome/UserInfo/Home screens, service/storage interfaces and initial asset groups.
   - Match reference scaling and sprite/fonts/transitions, native input fields, keyboard handling and safe areas. Implement user-approved readiness-driven splash/welcome; retain genuine progress/errors and don't show 100% before readiness.
   - Lazy-import Babylon after the lightweight shell where possible; preload required selected-city/character content when useful without blocking basic forms. Home's final interactive 3D backdrop is an acceptance requirement, not permanently replaced by a screenshot.
   - Establish canonical game/domain state and presentation events, versioned saves and test fixtures. No per-frame React transform updates.
   - Compare Splash/onboarding/Home screenshots and button routes; fix before expanding all screens.
   - Dependencies: Steps 1–2.
4. **Implement one complete faithful gameplay slice.**
   - Files: game/input/player/animation/collision/camera/road/word/pickup systems, HUD/Pause/Fail/Retry/Complete screens, test cases.
   - Use one actual city, character and authored ordinary/junction content. Match movement integration, timing, collisions, animation transitions, camera intro and home tap-to-play rules. Build a timestep strategy matching the source behaviour; test varying frame rates instead of imposing a different feel.
   - Implement data-driven word/letter rules including grapheme segmentation, normalization/case handling, tone marks, repeats, wrong-letter and tutorial exceptions. Prefer Intl.Segmenter with a verified fallback; compare with C# TextUtils fixtures.
   - Implement coins/gems/magnet/skateboard/glider, ordinary hazards, pause/resume countdown, fall, currency continue, level completion, retry and clean reset. Swept/cached collision queries must preserve hit semantics without high-speed misses.
   - Port curvature to Babylon shader material hooks with matching world-space displacement, transparent/skinned coverage and conservative displaced bounds. Preserve straight nearby collision geometry and no curved UI/previews.
   - Benchmark actual exported assets on mobile and review side-by-side gameplay recordings. Exit gate: visible/functional slice accepted and performance measured before bulk migration.
   - Dependencies: Steps 2–3 and reference settings/behaviour fixtures.
5. **Complete all UI screens and domain systems.**
   - Files: remaining screen components, domain modules, content/manifests, storage tests.
   - Port every inventoried Home canvas and dynamic dialog: gifts/missions/shop/modals/leaderboard/characters/previews/upgrades/settings/level map/activation/high-score flows. No decorative nonworking buttons.
   - Match costs, balances, defaults, timers, claim eligibility, locks/stars, upgrade effects, scroll/modal states and existing delays. Prevent duplicate claims/purchases from double taps or retries.
   - Preserve language selection and word translation presentation. Validate authenticated/guest and empty/error state transitions.
   - Use versioned browser saves for local prefs and dictionary caches. Browser storage cannot automatically read Android PlayerPrefs or another origin's Unity WebGL IndexedDB. Preserve existing progress only through verified same-origin/export/backend migration; request a separate reviewed migration if needed, never assume it is automatic.
   - Dependencies: Steps 1–4; UI/domain parity gates.
6. **Complete all cities, characters, dynamic environment and audio.**
   - Files: all city/character manifests/models, traffic/monkey/bird/powerup modules, previews, audio/radio.
   - Preserve Lagos/Abuja/PortHarcourt starters, weighted tiles, junction taxi routes/coordination, current active chase/traffic features and character accessories. Don't implement historical proposed police features if absent in the current game.
   - Match active runner-specific animation settings, completion/high-score previews, world letters and environmental animation. Keep full current visual content; optimize copies with visual regression checks instead of deleting detail silently.
   - Reuse current music/SFX and voiced radio; preserve actual playlists/gaps, shuffle, station label/equalizer, mute/ducking and focus/countdown/fail/city gates. No new TTS, radio content, or cancelled subtitles.
   - Load selected assets progressively, unload/dispose unused resources on swaps, pool repeated content and share/instance compatible geometry/materials. Account for transparency, animated residents, compressed-texture support and shadow fidelity.
   - Dependencies: Steps 2,4–5.
7. **Connect and verify browser-compatible services.**
   - Files: service adapters, api-contracts.md, env example with public configuration only, auth/integration tests.
   - Verify existing custom API schemas, signup/login/guest/refresh/logout, dictionary endpoints and permissions; use same-origin routing or explicitly configured CORS. Do not make remote writes during discovery or ship admin/service-account credentials.
   - Trace Unity identity/UGS leaderboard mapping and supported client/server APIs for exact installed/service versions. Browser integration must preserve player identity and leaderboard semantics; if a trusted server bridge is required, propose its deployment/security changes and obtain approval before provisioning.
   - Confirm source real/simulated IAP and rewarded-ad states. Native Unity store billing is not portable to browser checkout. Production payments/ads require user-approved compatible providers, merchant credentials, server verification and confirmed entitlement/reward flow; do not silently add a provider or report simulation as completion.
   - Match service failures/loading/availability UI. Keep test simulation explicitly labelled and excluded/disabled from release money/reward routes.
   - Do not claim full live-service parity while a required provider or identity bridge remains blocked. Report blockers distinctly from completed local game parity.
   - Dependencies: Steps 1,3,5; approved access/configuration and any additional deployment/provider decisions.
8. **Optimize delivery, test full parity and prepare a local release build.**
   - Files: asset pipeline, engine quality settings, lazy chunks/caching, tests, build config and docs.
   - Measure before optimizing; use shared meshes/materials, compatible instancing/pooling, compressed GLBs/textures/audio and CDN-ready hashed paths. Compression decoders must be bundled/hosted correctly and measured; KTX2 support must be tested on target devices.
   - Keep shell and selected city/runner boot separate from optional content. PWA caches versioned public assets and explicit safe content only; don't cache credentials/private API responses or promise account services offline. Handle updates, cache eviction/quota and missing assets.
   - Retain fidelity-first defaults; any reduced effects/device tier with visible differences is shown for approval, not used to claim exact parity silently.
   - Run full screen/flow matrix, compare current Unity and Babylon captures, and benchmark with the same devices/network/content. Initial performance goal: 60fps/16.7ms on the agreed representative device, with measured slower-device results; set first-play and transfer-byte budgets from real baseline, not unsupported guarantees.
   - Produce local production build and hosting requirements. Actual hosting/domain/service deployments require explicit follow-up approval. Check public-base paths, compression/MIME/CORS/HTTPS and SPA routing in local staging.
   - Dependencies: Steps 1–7; required service blockers resolved or explicitly documented as outstanding.

# Verification & Testing
## Automated
- Proposed commands after tooling is available: npm ci, npm run typecheck, npm run test, npm run build, npm run test:e2e; configure package scripts and pin tools before claiming these commands ran. Use Vitest-equivalent pure/domain tests and Playwright-equivalent browser tests if environment supports them.
- Export validation: all referenced assets/clip names/IDs resolve, skeletons and skin weights valid, transform/handedness/unit conversion correct, repeated tile weighting preserved, source files unchanged and no credential leakage.
- Movement: lane-change/jump/slide/double-tap thresholds and trajectories under replayed inputs at 30/60/120fps and a hitch; collision boundaries, skateboard protection, glider height, magnet radius and fail order match reference.
- Words: NFC/NFD forms per source normalization policy, combining tone marks, repeated graphemes, uppercase/language handling, in-order/any-order collection, wrong-letter/duplicate behaviour, missing dictionary and stale language loads. No stripping diacritics to make tests pass.
- Domain: level/star/unlock formulas, coins/gems/reward/inventory deltas, daily rollover/claim/reset rules, default preferences and corrupt/versioned storage; duplicate action protection and clean logout/account switch.
- State: Home -> intro/countdown -> playing -> pause/resume countdown -> fail/revive/retry/complete/home. Assert timers/audio/input gates, reset subscriptions, city/runner switches and no orphaned assets/taxis/listeners.
- UI: screenshot matrix at 390x844 and measured additional phone aspects, consistent fixture data/animation phase, pixel-diff triage, safe-area/scroll/modal/tab/text states; all expected buttons tested. Initial static layout alignment target within about 2 CSS pixels at reference size; text antialiasing and 3D shading differences reviewed separately rather than hidden by huge diff tolerance.
- Services: mocked tests are separate from live integration tests; auth expiry/network timeout/CORS/dictionary errors/leaderboard absence; no double rewards or fake payment success on release adapters.

## Manual and device acceptance
- Review every Unity reference screen against browser reproduction, including splash/login/language/welcome, all 17 canvases and discovered dynamic dialogs. Check exact art/font/glyphs, pressed/disabled states and animated previews, not only Home screenshots.
- Compare synchronized gameplay video for each city and every selectable character: intro, lane/jump/slide, authored scenery, road seams/curvature, traffic/taxis, letters, monkey and every active powerup, death/completion/retry.
- Listen to music/SFX/radio against source; test audio unlock, mute/ducking, pause/countdown, focus loss and city switching. Do not treat an AudioContext existing as proof of correct audible output.
- Touch device tests: scrolling vs swiping, input fields/mobile keyboard, multitap, modal interception, tab background/restore, resizing/orientation, browser navigation and supported PWA installation/update.
- Performance: named Android Chrome/iOS Safari devices, cold/warm cache and throttled network; record compressed transfer sizes, first UI/menu/gameplay timings, p50/p95 frame time/spawn hitches and supported memory/resource counts. Run extended gameplay and repeated city/character/retry cycles to detect leaks; report unsupported memory instrumentation honestly.
- Existing-player saves/accounts: test only via approved fixtures or safe staging identity; no Android-to-browser save migration claim without demonstrated transport and mapping.
- Release gate: parity matrix complete, user-reviewed visual deviations explicitly accepted, no missing/dummy final assets, no silent service substitutions, supported browser tests complete and blockers listed. A local slice, successful compilation or a screenshot is not evidence the entire game is migrated.
- Final report per milestone: exact changed/new files, feature/state coverage, screenshots/video, actual commands/tests/devices used, measured performance and remaining blockers. Preserve Unity's working build throughout.


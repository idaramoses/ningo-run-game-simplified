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

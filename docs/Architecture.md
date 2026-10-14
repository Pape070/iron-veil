# Iron Veil — Architecture

## Assembly layout

| Assembly | Contents | Unity dependency |
|---|---|---|
| `IronVeil.Rules` | Stats, XP, damage, difficulty, boss pattern runner, enemies, equipment, loot, quests, chapters, campaign content, save format, JSON | **none** (`noEngineReferences: true`) |
| `IronVeil.Game` | Boot and flow, settings, saves, input, gameplay, story, UI | `UnityEngine`, `UnityEngine.UI`, `Unity.InputSystem` |
| `IronVeil.Editor` | Project setup, scene factory, validators, balance report | `UnityEditor` |
| `IronVeil.Tests.EditMode` | NUnit tests over the rules and the campaign content | `UnityEngine.TestRunner` |

The split is deliberate: everything that decides **numbers, pacing and structure** is Unity-free, so it
compiles and runs under the plain .NET SDK in CI, and is covered by tests rather than by hope.

## Rules layer (Unity-free)

- `StatBlock` / `StatCatalog` / `StatComputation` — stats, per-point value, per-level growth
  (delta-based for multiplier stats), point budgets and refunds.
- `XpCurve` — XP to next, total XP, level from total XP, kill/boss/quest rewards.
- `DifficultyProfile` — every difficulty multiplier in one place, including pattern tier and telegraph
  scaling.
- `DamageRules` — outgoing damage (power scaling, crit, mitigation, armour pierce, weak points) and
  incoming damage (shield then health), shield recharge, healing clamps.
- `BossPatternRunner` — phase transitions, per-pattern cooldowns, anti-repeat weighting, telegraph /
  active / recovery states, enrage. Pure logic, deterministic with a seeded `Rng`.
- `SpawnRules` — turns an encounter definition into a timestamped spawn schedule honouring wave delays,
  elite budgets and difficulty spawn multipliers.
- `QuestRules` — objective reporting, requirement gating, timed objectives, no-damage objectives,
  completion percentage, reward summaries.
- `Campaign` (partial, split by domain) — builds the entire `CampaignContent`: 2 playable characters,
  12 weapons, gear, 19 enemy archetypes, 36 bosses with 3 phases and 10 patterns each, 36 quests,
  encounters, 12 chapters with beats/comic pages/savepoints/checkpoints/arenas, drop tables and perks.
- `ContentValidator` — fails the campaign if a chapter lacks a boss, a boss has no phases, pattern tiers
  are missing per difficulty, boss time-to-kill leaves the target band, quests reference unknown
  content, savepoint/checkpoint wiring is broken, or comic pages are unreferenced.
- `SaveCodec` / `SaveGame` — versioned save format with a checksum, migrations, sanitisation and pruning
  of ids that no longer exist after a balance patch.

## Game layer

### Boot and flow

`GameRoot` creates itself with `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`, registers every
service in `Services`, builds the camera and UI, and enters `MainMenu`. `GameStateService` is the single
source of truth for the flow state: input routing, `Time.timeScale`, pause behaviour and the world
update all read from it, so they cannot disagree.

### Services

| Service | Responsibility |
|---|---|
| `ContentDatabase` | Builds and validates the campaign once; lookups by id |
| `SettingsService` | Four-tab settings, JSON persistence, applies display/quality/audio |
| `AudioService` | Channel volumes, mute, mono/stereo output, synthesised placeholder clips |
| `InputService` | Owns the actions asset, tracks the active device, resolves aim, formats prompts |
| `InputRebinder` | Interactive rebinding, conflict detection, override persistence |
| `SaveService` | Three manual slots + autosave + quick resume, backups, corruption recovery by checksum |
| `ProgressionService` | XP, levels, stat points, perks, salvage, upgrades, loot, respec |
| `QuestService` | Quest state, reporting, level/chapter unlock, turn-in |
| `StoryService` | Flags, comic unlocks, gallery/bestiary listings, barks |
| `ScreenManager` | Screen stack, overlays, widget focus navigation, HUD |

### Gameplay

- `LevelGeometry` — arena geometry generated deterministically from arena data and chapter index; solid
  floors with pits and hazards, one-way platforms, tagged spawn points.
- `CharacterBody` — hand-rolled AABB movement and collision. There is **no Rigidbody2D or Collider2D in
  the project**: physics is resolved per axis against platform rectangles, which keeps movement
  identical across machines and easy to reason about.
- `PlayerActor`, `EnemyActor`, `BossActor` — plain classes (not MonoBehaviours) that own a GameObject
  for rendering and are ticked explicitly by the chapter runtime, so update order is deterministic.
- `ProjectileSystem` — pooled projectiles for player weapons, enemy fire and boss patterns, with pierce,
  gravity, homing and explosions.
- `EffectSystem` / `FloatingTextSystem` — pooled sparks, explosions, beams, telegraph rings and damage
  numbers.
- `PatternExecutor` — one implementation of all twenty authored attack patterns, shared by bosses and
  elite enemies through `IPatternHost`. A boss is therefore just a body with phases and a scheduler; the
  attacks themselves are written once.
- `ChapterRuntime` — loads a chapter, places markers, walks the beat list, spawns encounters and bosses,
  writes checkpoints, handles savepoints, hazards, beams, summons, rewards and respawns.
- `SpawnDirector` / `CameraRig` — wave scheduling from `SpawnRules`, and a clamped side-scrolling camera
  with boss framing and screen shake.

### UI

`UiKit` builds every widget at runtime: sliced panels, labels, buttons, option rows, sliders, toggles and
stat rows. `ScreenBase` focuses widgets **spatially** (nearest widget in the pressed direction), so one
navigation implementation serves lists, tab rows and grids, and the mouse focuses whatever the cursor is
over. There is no `EventSystem` or `Selectable` in the project: mouse clicks are hit-tested with
`RectTransformUtility` and routed through the same focus model as the gamepad, which guarantees the two
input paths cannot diverge.

`ComicPlayer` renders a comic page panel by panel: backdrop gradient per palette, actors placed from
data, speech bubbles, captions, onomatopoeia, typewriter text at the configured speed, panel effects
(flash and shake, both suppressed by Reduced Flashing), and skip/advance in every input scheme.

## Assets and audio

Art is generated: `PlaceholderArt` creates textures and sprites for actors, silhouettes, panels, circles,
gradients and UI chrome, cached by key. `FontProvider` resolves the Unity 6 built-in font with fallbacks.
`AudioService` synthesises short clips (shots, hits, deaths, pickups, comic pages) so the game has audio
without imported assets. Replacing placeholders with real art is a matter of assigning sprites in
`PlaceholderArt`/`Palette`; the rest of the game is unchanged.

## Save format

`settings.json` and `bindings.json` live beside the save slots in `Application.persistentDataPath`:
`settings.json` (four-tab options), `bindings.json` (input overrides), `saves/*.json` (three manual
slots plus autosave and quick resume, each with a `.bak`). Every save is written through `SaveCodec`:

1. Serialise the payload to deterministic JSON (insertion-ordered keys).
2. Compute an FNV-1a checksum over that payload and store it alongside the version.
3. Write to a temporary file, decode it back to verify, then replace the previous file (kept as `.bak`).

On load, a checksum mismatch or a version newer than the build is reported, and the service falls back to
the backup. Unknown ids (weapons, gear, perks, quests, comics) are pruned so a balance patch cannot brick
a save, and invested stat points are clamped to the legal budget.

## Input

`GameInput.inputactions` is **generated**, not hand-edited, by
`tools/inputmap/generate_inputactions.py`. It authors two control schemes — `KeyboardMouse` and
`Gamepad` — over a `Gameplay` map (Move, Aim, AimPoint, Fire, Reload, AbilityPrimary, AbilitySecondary,
SwapWeapon, NextWeapon, PreviousWeapon, UseFlask, Brace, Interact, Pause) and a `UI` map (Navigate,
Submit, Cancel, Point, Click, ScrollWheel, TabLeft, TabRight). GUIDs are derived from a namespace plus
key, so regenerating the asset never invalidates saved binding overrides. Aiming is the one asymmetric
action: the mouse provides `AimPoint` (screen position) and the pad provides `Aim` (right stick);
`InputService.ResolveAimDirection` folds both into a single world-space direction and optionally bends it
with aim assist.

## Verification

| Check | Command | What it proves |
|---|---|---|
| Rules and content tests | `dotnet test tools/RulesTests/IronVeil.RulesTests.csproj` | 93 tests: stats, XP, damage, difficulty, boss scheduling, spawn planning, quests, save round-trip and migrations, and full-campaign content assertions |
| Content validation | `dotnet run --project tools/BalanceReport/...` (also `Iron Veil ▸ Validate ▸ Campaign Content`) | Structural and balance contract of the whole campaign |
| Balance report | `dotnet run --project tools/BalanceReport/BalanceReport.csproj` | Per-chapter enemy/boss HP, time-to-kill and pacing |
| Syntax check | `dotnet run --project tools/SyntaxCheck/SyntaxCheck.csproj .` | Every C# file parses (catches malformed types and stray tokens) |
| Unity session | Iron Veil menus | Package restore, input backend, scene creation, Play mode, rebinding |

The game layer cannot be compiled outside Unity because the engine assemblies are not redistributable,
so its verification is: Roslyn syntax checking here, structural checks in the editor menu, and the
in-editor Test Runner for anything that needs the engine.

## Extending

- **Add an enemy**: append to `Campaign.Enemies.cs`, give it a class, tier, patterns and first chapter;
  the validator checks it is reachable and `EnemyActor` handles the behaviour class.
- **Add a boss**: append to `Campaign.Bosses.cs` with phases and patterns; `BossPatternRunner` handles
  scheduling and `PatternExecutor` handles the attacks.
- **Add a chapter or beat**: append to `Campaign.ChaptersAct*.cs`; the runtime walks beats in order, and
  the validator enforces that beats reference real encounters, bosses, quests and comic pages.
- **Retune**: edit numbers in the campaign files, then run the tests and the balance report — the
  validator fails if a chapter leaves the intended time-to-kill band.

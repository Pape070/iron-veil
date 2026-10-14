# Iron Veil

A complete **2D side-scrolling run-and-gun** built for **Unity 6.3 LTS (6000.3.26f1)** with full
**keyboard + mouse** and **gamepad** support, a twelve-chapter campaign of roughly two hours each,
twelve chapter bosses plus twenty-four mini-bosses, side quests, a three-phase difficulty system,
four tabbed options menus, and **comic-book cutscenes** rendered from your data.

Everything in this repository is authored content and code: no art, audio, or scene assets are
required to run it. Characters, enemies, bosses, effects, UI and comic panels are generated at
runtime, and sounds are synthesised, so the whole campaign is playable the moment the project opens.

---

## Quick start

1. Install **Unity 6.3 LTS** (`6000.3.26f1` or a newer 6000.3 patch) through Unity Hub and open this
   folder as a project. Unity will restore packages from `Packages/manifest.json`.
2. In Unity, run **Iron Veil ▸ Setup ▸ Apply Project Settings**. This sets the product name, screen
   defaults, and switches **Active Input Handling** to *Input System Package (New)*. Unity may ask to
   restart — say yes.
3. Run **Iron Veil ▸ Setup ▸ Create Scenes**. It creates `Boot`, twelve chapter scenes and a
   `Sandbox` scene, and adds them to the build list.
4. Press **Play**. The game boots itself from any scene: `GameRoot` is created before the first scene
   loads, so there is nothing to wire in the inspector.

The **Sandbox** scene starts chapter 1 with a level-60 character so every weapon, ability and boss can
be tested immediately.

### Menus available in the editor

| Menu | What it does |
|---|---|
| Iron Veil ▸ Setup ▸ Apply Project Settings | Product name, resolution defaults, input backend |
| Iron Veil ▸ Setup ▸ Create Scenes | Builds Boot, Chapter01-12 and Sandbox scenes into the build list |
| Iron Veil ▸ Validate ▸ Campaign Content | Runs the content validator (structure, balance, pacing, references) |
| Iron Veil ▸ Validate ▸ Project Structure | Checks the input asset, assemblies, scripts and content are present and valid |
| Iron Veil ▸ Report ▸ Balance Report | Prints the per-chapter pacing table (enemy HP, boss HP, recommended level) |

### Tests

The whole balance and content layer is plain C# with **no Unity dependency**, so it runs in the editor
*and* from the command line:

- In Unity: **Window ▸ General ▸ Test Runner ▸ EditMode ▸ Run All** (93 tests).
- Without Unity, using the .NET SDK:

```bash
dotnet test tools/RulesTests/IronVeil.RulesTests.csproj   # 93 rules + content tests
dotnet run --project tools/BalanceReport/BalanceReport.csproj   # pacing table
dotnet run --project tools/SyntaxCheck/SyntaxCheck.csproj .     # Roslyn syntax check over every script
```

---

## What is in the box

- **Twelve chapters**, ~2 hours each (~24 hours total), each with its own location, comic page, arena,
  savepoints, checkpoints, side quests and a chapter boss with three phases.
- **36 bosses** (12 chapter bosses + 24 mini-bosses) and **19 enemy archetypes** across 15 behaviour
  classes, all scaled by chapter and difficulty.
- **Two playable characters** with different weapons, abilities, passives, growth curves, dialogue and
  level-up barks: Kade "Anvil" Rourke and Vera "Wisp" Voss.
- **Six weapons each**, five upgrade levels per weapon, gear with rarities, salvage economy, respec.
- **36 side quests** (kills, bounties, collections, escorts, no-damage runs, timed survival).
- **Three difficulties** that change enemy pressure, telegraph windows, pattern counts, rewards and
  even which boss attacks exist — never story content.
- **Comic cutscenes** at every story beat, replayable in the gallery.
- **Accessibility**: reduced flashing, high-contrast UI, colour-blind palette, large text, HUD scaling,
  subtitle scaling and speed, extra lives, reduced damage, slower boss patterns, aim snapping.
- **Full input rebinding** for both control schemes, with conflicts reported and overrides saved to JSON.

## Repository layout

```
Assets/_Project/Scripts/Rules/      pure C# rules: stats, XP, damage, difficulty, boss patterns,
                                   quests, loot, campaign content, save format, JSON
Assets/_Project/Scripts/Core/       game root, flow state machine, settings, saves, meta services
Assets/_Project/Scripts/Input/       input service (device, aim, prompts) and interactive rebinding
Assets/_Project/Scripts/Gameplay/    world geometry, actors, player, enemies, bosses, patterns,
                                   chapter runtime, spawn director, camera
Assets/_Project/Scripts/Story/       comic cutscene player
Assets/_Project/Scripts/UI/          UI toolkit, screens, HUD, comic panels
Assets/_Project/Editor/              project setup, scene factory, validators, balance report
Assets/_Project/Tests/EditMode/      NUnit tests for the rules and content layer
tools/                               .NET projects: rules tests, balance report, syntax check,
                                   input-asset generator
docs/                                design, story, controls, boss and architecture documents
```

Documents: [Design](docs/Design.md) · [Story](docs/Story.md) · [Bosses](docs/Bosses.md) ·
[Controls](docs/Controls.md) · [Architecture](docs/Architecture.md).

## Verification status

Verified in this repository (no Unity editor available here):

- The rules and content layer compiles and passes **93 tests** against the real .NET SDK.
- The content validator reports **0 errors** for the full campaign (12 chapters, 36 bosses, 19
  enemies, 36 quests) including boss time-to-kill inside the target band on every difficulty.
- Every C# file in the repository parses cleanly under Roslyn (51 files, 0 syntax errors).
- `tools/inputmap/generate_inputactions.py` regenerates `GameInput.inputactions` with deterministic
  GUIDs and self-checks for duplicate ids and dangling action references.

Not verifiable here, and therefore **unverified**: opening the project in the Unity editor, Play mode
behaviour, input rebinding in a real player, and rendering. The editor menus above exist so the first
Unity session validates all of it in a few clicks.

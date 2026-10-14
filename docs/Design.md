# Iron Veil — Design

## Pitch

Helion's Directorate built a machine that projects a second sky over the coast: the Iron Veil. It was
sold as weather control and used as erasure. Two operatives who helped build it are sent to break it.
**Iron Veil** is a 2D side-scrolling run-and-gun: precise platforming, broad weapon choice, readable
enemy telegraphs, and boss fights that change shape as they lose health.

## Pillars

1. **Readable pressure.** Every attack telegraphs in geometry before it lands, on every difficulty.
   Difficulty changes the size of the windows, not whether they exist.
2. **Two ways to play, one feel.** Keyboard + mouse and gamepad are the same game: mouse aims at the
   cursor, the right stick aims in a direction, and both feed one aim vector into one code path.
3. **Chapters that end.** Twelve chapters, ~2 hours each, each with a comic opening, three or more
   beats of play, a savepoint, a boss, and a closing card that records your run.
4. **Systems you can inspect.** Balance, difficulty, boss scheduling and content structure live in a
   Unity-free assembly that is unit tested and scriptable, so tuning never requires playing blind.

## Genre and framing

Side-scrolling run-and-gun with free 360° aim (Contra / Metal Slug framing).

- Movement: WASD / left stick, one jump with variable height, coyote time and jump buffering.
- Aiming: 360° free aim, no aim mode toggle required.
- Combat: six weapons per character, each with distinct cadence, spread, reload and feel; explosive and
  piercing options; a flask for healing; two abilities with cooldowns.
- Structure: each chapter is a sequence of authored beats — comic, dialogue, exploration, arena
  encounters, puzzle nodes, mini-boss, savepoint/shop, boss, chapter-complete — walked in order by the
  chapter runtime.

## Core loop

1. Fight through an arena: 5-14 enemies, elites from Normal upward, waves gated on wave-clear.
2. Collect salvage, ammo, flasks and the occasional gear drop.
3. Reach a savepoint: rest (heal, refill flasks, save), spend stat points, upgrade weapons, respec.
4. Fight the chapter boss: three phases, escalating pattern sets, enrage timers at the top end.
5. Watch the chapter's comic page, take the completion card, advance.

## Progression

- **Level cap 60**, tuned so a full campaign lands near the cap on Normal.
- **XP** from kills, objectives, quests and boss clears, multiplied by chapter pacing and difficulty.
- **Stat points**: two per level, spent on Power, Defense, Shield, Shield regen, Health, Fire rate,
  Crit chance, Crit damage, Move speed, Ability power, Reload speed and Luck. Refundable at any
  savepoint for salvage, so a build mistake is recoverable but not free.
- **Perks** unlock automatically at their level for the character who can use them.
- **Weapons**: six per character, upgradeable to +5 with salvage; damage scales with upgrade level.
- **Gear**: armor, boots, trinket, charm and weapon slots with rarity tiers; awarded by chapter, boss
  or quest.
- **Salvage**: earned from kills, bosses and crates; spent on upgrades and respecs.

## Difficulty

| | Easy | Normal | Hard |
|---|---|---|---|
| Boss patterns available | Core only | Core + Extended | Core + Extended + Full |
| Enemy health / damage | lower | baseline | higher |
| Telegraph windows | longer | baseline | shorter |
| Player damage taken | reduced | baseline | increased |
| XP and loot | slightly reduced | baseline | increased |
| Extra lives | 1 assist option | — | — |

Difficulty can be **lowered** at any savepoint and never raised mid-run; records remember the highest
difficulty a boss was cleared on. Extra lives, reduced incoming damage, slower boss patterns and aim
snapping are accessibility options independent of difficulty.

## Chapter pacing (12 × ~2 h)

| Ch | Title | Location | Boss | Est. |
|---|---|---|---|---|
| 1 | The Rust Gate | Rust Gate Checkpoint | Warden Skarn | 2 h |
| 2 | Sunken Docks | Sunken Docks | Tidewalker | 2 h |
| 3 | Rooftop Watch | Watch District Rooftops | Overseer Halden | 2 h |
| 4 | The Foundry | Helion Foundry Prime | Foundry Prime | 2 h |
| 5 | The Siren's Stage | Coast Opera House | The Siren | 2 h |
| 6 | Hollow Cathedral | Hollow Cathedral | The Hollow Choir | 2 h |
| 7 | Glass District | Glass District | Nightglass | 2 h |
| 8 | Bone Yards | Bone Yards | Ironmaw | 2 h |
| 9 | Runoff Cathedral | Runoff Cathedral | Sump | 2 h |
| 10 | Ossuary | Veil Ossuary | Veilwarden Ossuary | 2 h |
| 11 | Undercity | Veil Undercity | The Conductor | 2 h |
| 12 | The Iron Veil | Veil Core | The Iron Veil | 2 h |

Act 1 (chapters 1-4) introduces Helion, the Veil and the crew's former colleagues. Act 2 (5-8) turns
the coast against itself. Act 3 (9-12) goes down and through: the machine, the Directorate and the
final decision.

Recommended levels rise through the campaign; each chapter also records a minimum level, and the
validator fails the build if a chapter is unreachable or overtuned.

## UIs

- **Main menu** — Continue (newest save), New Campaign, Options, Controls, Gallery, Codex, Quit, plus a
  live progress summary.
- **Character select** — the two operatives with their full kit and stat block, plus difficulty.
- **Options** — four tabs: Display (resolution, mode, V-Sync, frame cap, quality, brightness), Audio
  (master/music/effects/voice/ambience, mute, mono output), Gameplay (difficulty, shake, hit stop,
  damage numbers, aim source/assist, dead zone, autosaves, enemy bars), Accessibility (flashing, contrast,
  colour-blind palette, text size, HUD size, subtitles, dialogue speed, extra lives, reduced damage,
  slower bosses, aim snap).
- **Controls** — every action rebindable on both schemes; conflicts are named; overrides persist.
- **HUD** — health/shield, flasks, weapon and ammo, ability cooldowns, XP, objective, quest tracker,
  interact prompt, boss bar with phase name, autosave confirmation, low-health vignette.
- **Pause, savepoint, level-up, quest log, gallery, codex, chapter card, game over, ending.**

## Accessibility summary

Reduced flashing (dampens comic and hit flashes), high-contrast UI, colour-blind palette option, large
text and HUD scaling, subtitle scaling and speed, adjustable screen shake, damage numbers, aim guide,
dead zone, extra lives, reduced incoming damage, slower boss patterns, aim snapping to targets, and
auto-advancing comic panels. None of these change story content or block completion.

## Out of scope

Art and audio production (all assets are generated placeholders), online play, and mobile input. The
content model supports adding real assets by swapping sprite keys in `PlaceholderArt` and the palette.

# Iron Veil — Story and campaign

> Eleven months ago the Directorate sealed the sky over the coast and called it the Iron Veil. It was
> sold as weather control. It is used as erasure: whatever the Veil records as "unregistered" stops
> existing to anyone standing under it.

Two people built the machine and two people are sent to break it. **Kade "Anvil" Rourke** was a Helion
breacher who carried the charges that anchored the projector ring; **Vera "Wisp" Voss** charted the
coastline the Veil now draws over. Both were paid in a promise that the Veil would only ever look, never
act. Both have spent eleven months finding out otherwise.

## Cast

| Name | Role | In the story |
|---|---|---|
| Kade "Anvil" Rourke | Heavy gunner, ex-Helion breacher | Believes the Veil should have been destroyed from the outside, with charges, on day one |
| Vera "Wisp" Voss | Scout, ex-cartographer | Mapped the coast for Helion and knows the Veil's ground truth better than the Directorate does |
| Ivo Marchetti | Quartermaster, ex-supply clerk | Runs the scrap truck, the salvage economy and most of the side quests |
| Warden Skarn, Tidewalker, Halden, Foundry Prime | Directorate assets | The first four chapter bosses; Helion's own former security |
| The Siren, The Hollow Choir, Nightglass, Ironmaw | The Veil's instruments | The middle act: the machine begins using the coast against itself |
| Sump, Veilwarden Ossuary, The Conductor | The Directorate's plumbing | The descent under the coast into the structure of the Veil |
| The Iron Veil | The machine itself | The finale: three phases of something that has been listening the whole time |

Story is told entirely in **comic pages** at story beats (four per chapter: intro, mid-chapter story,
boss, outro), with dialogue barks during play and boss intro lines from the character you picked. Both
characters share every story beat and speak differently in it: Kade pushes, Vera reads the room.

## Act structure

### Act I — Chapters 1-4: Helion's own gates

| Ch | Title | What happens |
|---|---|---|
| 1 | The Rust Gate | The crew breaches the outer checkpoint. The "Wall of Names" comic reveals what the Veil does to unregistered coast towns; Warden Skarn dies holding a gate that he knows is pointless. |
| 2 | Sunken Docks | Shipping has stopped because the Directorate flooded the harbour to cover a projector failure. Tidewalker is the siege frame that did the flooding. |
| 3 | Rooftop Watch | Over the rooftops, Vera finds her own mapping ledger in Helion hands — proof her survey is what the Veil uses to decide what to erase. |
| 4 | The Foundry | The foundry that cast the Veil runs around the clock. Foundry Prime is a production line that fights like one, and the line finally stops. |
| | | *Act close: the crew has the coast. The Veil is still up, and now it knows their names.* |

### Act II — Chapters 5-8: the Veil uses the coast

| Ch | Title | What happens |
|---|---|---|
| 5 | The Siren's Stage | The opera house was the last building registered before the sky closed; the Siren is what the Veil made of its acoustics. |
| 6 | Hollow Cathedral | The archive of the dead is an input, not a memorial. The Choir Engine is fed by names. |
| 7 | Glass District | Nightglass wears Vera's face and Kade's record; the district's reflections are used as cover by something that has been watching them since chapter 1. |
| 8 | Bone Yards | Everything the Veil threw away is piled here, and Ironmaw feeds on it. |
| | | *Act close: the crew finds the outflow. Everything the Veil deletes goes down.* |

### Act III — Chapters 9-12: down and through

| Ch | Title | What happens |
|---|---|---|
| 9 | Runoff Cathedral | Under the coast, the Veil's runoff is the town it was told to forget. Sump guards the vat. |
| 10 | Ossuary | The Directorate's honoured dead keep their names, which is why the Ossuary is the only place in the country the Veil cannot read. Veilwarden Ossuary is the first of them, still on duty. |
| 11 | Undercity | The office that never closed. The Conductor speaks with the whole Directorate's voice, and every attack in the fight is one the crew has already survived. |
| 12 | The Iron Veil | The Veil Core. Three phases, a machine that has been listening the entire campaign, and a final choice about who gets to be registered when the sky comes back down. |

**Ending.** The Veil falls. Both operatives walk out; what the Directorate keeps on paper, and what the
coast remembers, depends on the choices recorded in your save flags across the run.

## Beats per chapter

Each chapter runs 12-16 beats, in order, as authored data:

`Comic` (intro) → `Exploration` → `Arena` → `Savepoint` → `MiniBossFight` (optional) → `Comic` (story) →
`Arena` → `MiniBossFight` → `Savepoint` (pre-boss) → `Comic` (boss) → `BossFight` → `Comic` (outro) →
`ChapterComplete`, with `Puzzle`, `Choice`, `Dialogue` and `SideQuestHook` beats mixed in where the
chapter calls for them.

Chapter completion writes a checkpoint, an autosave, an unlock flag, and unlocks the next chapter's quest
and encounter content.

## Side content

Thirty-six quests across the campaign: three per chapter, mixing kills, tagged bounties, collections,
escorts, no-damage runs, timed survival, ability usage and weak-point hits. Rewards are XP, salvage,
weapons, gear or perks, and each quest grants story flags that later content can read. Quests are
accepted through `SideQuestHook` beats, tracked in the HUD and the quest log, and turned in at any
savepoint.

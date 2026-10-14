# Iron Veil — Bosses

## How boss fights work

Every boss is data: a `BossDefinition` with **three phases** and **ten authored patterns**, run by
`BossPatternRunner` (phase transitions, cooldowns, anti-repeat weighting, telegraph / active / recovery
timing, enrage) and performed by `PatternExecutor` (all twenty attack pattern kinds).

- **Phases** trigger on health ratios (100%, ~65%, ~30%) with their own speed, damage, incoming damage
  and cooldown multipliers, and can summon adds or announce themselves on screen.
- **Pattern tiers** decide which attacks exist on a given difficulty:
  **Core** (always), **Extended** (Normal and up), **Full** (Hard only).
- **Telegraphs**: every pattern has a warning window. Entering damage, telegraph length and cooldowns are
  all scaled by the difficulty profile, and the accessibility option "slower boss patterns" adds 25%.
- **Weak points** take 55% reduced defence and force a critical hit; they expose during recovery windows
  and on patterns tagged `IsWeakPointExposed`.
- **Enrage**: top-end bosses gain a shorter cooldown multiplier after a set time, so a stalled fight
  escalates instead of looping.
- **Adds**: `SummonAdds` and `TurretDeploy` patterns spawn chapter-appropriate enemies, kept in budget by
  the difficulty profile's elite and add counts.

Best-effort dry runs across the whole campaign put every chapter boss at a **2-4 minute fight** for a
character at the recommended level, using the chapter's intended weapon; the content validator fails the
build if a boss leaves that band.

## Chapter bosses

| Ch | Boss | Title | Location | Notes |
|---|---|---|---|---|
| 1 | Warden Skarn | Keeper of the Rust Gate | Rust Gate Checkpoint | Charge dashes, riot volleys, sentry turrets; teaches telegraph reading |
| 2 | Tidewalker | Harbour Siege Frame | Sunken Docks | Harpoon salvos, water stomps, rising-tide hazards |
| 3 | Overseer Halden | The Long Watch | Watch District Rooftops | Long-range suppression, para-drops, laser grids |
| 4 | Foundry Prime | Assembly Line One | Helion Foundry Prime | Deploys production-line adds, molten hazards, beam crosses |
| 5 | The Siren | Voice of the Veil | Coast Opera House | Sound-based sweeps, homing swarms, mirrored patterns |
| 6 | The Hollow Choir | Swarm Engine | Hollow Cathedral | Spawns swarms in waves, choir-static pulses, hazard carpets |
| 7 | Nightglass | The Face You Know | Glass District | Cloak-and-strike, reflective shields, mirrored doubles |
| 8 | Ironmaw | Siege Beast | Bone Yards | Grapple slams, tail sweeps, heavy radial bursts |
| 9 | Sump | Runoff Cathedral | Runoff Cathedral | Toxic hazards, drill charges, arena flooding |
| 10 | Veilwarden Ossuary | First of the Honoured | Veil Ossuary | Bone armour phases, penitent adds, beam lattices |
| 11 | The Conductor | Voice of the Directorate | Veil Undercity | Pattern conduction: copies earlier bosses' signature attacks |
| 12 | The Iron Veil | Last Machine of the Directorate | Veil Core | Three-phase finale, enrages, summons, the full pattern set |

Each chapter also has **two mini-bosses**, fought mid-chapter as their own beat:

| Ch | Mini-bosses |
|---|---|
| 1 | Rustpan (Scrap Scavenger), Turnkey (Gate Operator) |
| 2 | Wreck Diver (Salvage Suit), Salt Tongue (Dock Enforcer) |
| 3 | Signal Thief (Comms Interceptor), Dust Devil (Storm Runner) |
| 4 | Rangefinder (Foundry Targeting Rig), Lockjaw (Press Operator) |
| 5 | Lesser Siren (Cantor of the Choir), Cinder (Veil Burnout) |
| 6 | Brood Keeper (Keeper of the Mites), Static Priest (Veil Liturgist) |
| 7 | Glass Twin (Second Reflection), Hooks (Butcher of the Docks) |
| 8 | Ironjaw (Alpha of the Yard), Hollow Cantor (Echo of the Choir) |
| 9 | Drillmaster (Vein Bore), Hound Mother (Den Keeper) |
| 10 | Tollkeeper (Bridge Warden), The Penitent (Repentant Warden) |
| 11 | The Gutterking (Lord of the Undercity), Signal Adjunct (Voice of the Office) |
| 12 | Veil Hand (Curator's Reach), The Last Archivist (Keeper of Names) |

Mini-bosses use the same runner and executor with two phases and tighter arenas, and are skippable the
way side content should be: they are their own beat, and the beat completes if the mini-boss was already
defeated.

## Pattern library

Twenty pattern kinds are implemented once and shared by every boss and elite enemy:

`RadialBurst`, `AimedVolley`, `SweepingBeam`, `ChargeDash`, `GroundPound`, `MissileBarrage`, `SummonAdds`,
`PhaseBlink`, `ArenaHazard`, `HomingSwarm`, `ShieldRecharge`, `TurretDeploy`, `ShockwaveTrap`,
`CloakAndStrike`, `BeamCross`, `Minefield`, `GrappleSlam`, `ReflectShield`, `LaserGrid`, `TailSweep`.

Each pattern is authored with cooldown, telegraph, active duration, recovery, damage, projectile count,
spread, range, radius, tracking and tags. Two data fields matter for readability:

- `EasyHint` — the line the fight teaches you, surfaced to the player as a callout.
- `IsWeakPointExposed` — marks the windows where the weak point opens.

## Fighting them well

1. Learn the telegraph, not the attack: every pattern warns in the same geometry it will hit.
2. Use the recovery window — most bosses are only staggerable and weak-point-open after a pattern ends.
3. Break line of sight with verticality: floating platforms are placed to dodge GroundPound, LaserGrid and
   ArenaHazard patterns.
4. Bring the right tool: armour-piercing weapons (Breach Cutter, Verdict, Needle) strip high-defence
   phases; explosives clear adds; Overclock and Momentum passives shorten phase 3 substantially.
5. Save scum shamelessly — a savepoint sits before every boss, and retries are free.

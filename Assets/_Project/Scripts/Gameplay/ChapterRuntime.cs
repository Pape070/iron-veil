using System;
using System.Collections.Generic;
using IronVeil.Rules;
using UnityEngine;

namespace IronVeil
{
    /// <summary>An interactive point in the world: savepoint, checkpoint or pickup.</summary>
    public sealed class WorldMarker
    {
        public string Id = string.Empty;
        public string Label = string.Empty;
        public Vector2 Position;
        public Rect TriggerBox;
        public SavepointDefinition Savepoint;
        public CheckpointDefinition Checkpoint;
        public SpriteRenderer Renderer;
        public SpriteRenderer Glyph;
        public bool Used;
        public float BobPhase;
        public bool IsSavepoint => Savepoint != null;

        public bool Contains(Vector2 point) => TriggerBox.Contains(point);
    }

    /// <summary>A telegraphed area-of-effect that damages the player while it is alive.</summary>
    public sealed class Hazard
    {
        public Vector2 Position;
        public float Radius;
        public float TimeRemaining;
        public float Damage;
        public string Tag = string.Empty;
        public float TickTimer;
        public SpriteRenderer Renderer;
    }

    /// <summary>An active beam: a line that damages whatever it crosses for a moment.</summary>
    public sealed class BeamInstance
    {
        public Vector2 From;
        public Vector2 Direction;
        public float Length;
        public float Width;
        public float Damage;
        public float TimeRemaining;
        public Team Owner;
        public string Tag = string.Empty;
        public float TickTimer;
    }

    /// <summary>
    /// Runs one chapter: builds the arena, places markers, walks the beat list,
    /// spawns encounters and bosses, tracks checkpoints and handles respawns. The
    /// beat list is authored data, so the chapter structure is content rather than
    /// code.
    /// </summary>
    public sealed class ChapterRuntime : MonoBehaviour
    {
        private const float InteractRadius = 2.2f;

        private ChapterDefinition _chapter;
        private LevelGeometry _level;
        private ChapterBeat _currentBeat;
        private int _beatIndex;
        private bool _loaded;
        private bool _beatCompletePending;
        private bool _waitingForBeatInput;

        private PlayerActor _player;
        private BossActor _boss;
        private readonly List<EnemyActor> _enemies = new List<EnemyActor>();
        private readonly List<IDamageable> _damageables = new List<IDamageable>();
        private readonly List<WorldMarker> _markers = new List<WorldMarker>();
        private readonly List<Hazard> _hazards = new List<Hazard>();
        private readonly List<BeamInstance> _beams = new List<BeamInstance>();
        private GameObject _hazardRoot;

        private ProjectileSystem _projectiles;
        private EffectSystem _effects;
        private FloatingTextSystem _text;
        private Feedback _feedback;
        private CameraRig _cameraRig;

        private SpawnDirector _spawner;
        private Rng _rng = new Rng(90210);
        private float _autosaveTimer;
        private float _levelTimer;
        private float _bossFightTimer;
        private int _chapterIndex = 1;
        private int _deathsThisChapter;
        private bool _puzzleInteracted;

        public event Action<ChapterBeat> BeatChanged;
        public event Action<string> InteractPromptChanged;

        // ------------------------------------------------------------- accessors

        public ChapterDefinition Chapter => _chapter;
        public LevelGeometry Level => _level;
        public PlayerActor Player => _player;
        public BossActor Boss => _boss;
        public List<EnemyActor> Enemies => _enemies;
        public List<IDamageable> Damageables => _damageables;
        public ProjectileSystem Projectiles => _projectiles;
        public EffectSystem Effects => _effects;
        public FloatingTextSystem Text => _text;
        public Feedback Feedback => _feedback;
        public Rng Rng => _rng;
        public Transform ActorRoot { get; private set; }
        public Camera Camera { get; private set; }
        public bool IsLoaded => _loaded;
        public int ChapterIndex => _chapterIndex;
        public ChapterBeat CurrentBeat => _currentBeat;
        public string InteractPrompt { get; private set; } = string.Empty;
        public float PlayerRadius => _player == null ? 0.5f : _player.Body.HalfWidth + 0.3f;

        public ContentDatabase Content => Services.Get<ContentDatabase>();
        public SaveService Saves => Services.Get<SaveService>();
        public SettingsService Settings => Services.Get<SettingsService>();
        public InputService Input => Services.Get<InputService>();
        public AudioService Audio => Services.Get<AudioService>();
        public ScreenManager Screens => Services.Get<ScreenManager>();
        public ProgressionService Progression => Services.Get<ProgressionService>();
        public QuestService Quests => Services.Get<QuestService>();
        public StoryService Story => Services.Get<StoryService>();
        public DifficultyProfile Difficulty =>
            Services.Get<ProgressionService>()?.Difficulty ?? DifficultyProfile.For(IronVeil.Rules.Difficulty.Normal);

        public Vector2 PlayerPosition => _player != null && _player.Body != null ? _player.Body.Position : Vector2.zero;
        public string CurrentMusicId => _chapter?.Arenas != null && _chapter.Arenas.Count > 0 && !string.IsNullOrEmpty(_chapter.Arenas[0].MusicId)
            ? _chapter.Arenas[0].MusicId
            : "mx_ch" + _chapterIndex.ToString("00");

        // ------------------------------------------------------------- initialization

        public void Initialize(Camera gameCamera)
        {
            Camera = gameCamera;

            GameObject systemsRoot = new GameObject("World");
            systemsRoot.transform.SetParent(transform, false);
            ActorRoot = systemsRoot.transform;

            _projectiles = new ProjectileSystem();
            _projectiles.Initialize(ActorRoot);

            _effects = new EffectSystem();
            _effects.Initialize(ActorRoot);

            _text = new FloatingTextSystem();
            _text.Initialize(ActorRoot);

            _feedback = new Feedback(Settings);

            GameObject hazardRoot = new GameObject("Hazards");
            hazardRoot.transform.SetParent(ActorRoot, false);
            _hazardRoot = hazardRoot;

            _spawner = new SpawnDirector(this);
            _cameraRig = new CameraRig(gameCamera, this);

            _projectiles.PlayerHitCallback = HandlePlayerProjectileHit;
            _loaded = false;
        }

        /// <summary>Uses the arena geometry for the chapter and positions the player.</summary>
        public void LoadChapter(int chapterIndex, string beatId, bool fromStart)
        {
            Unload();

            _chapterIndex = Mathf.Clamp(chapterIndex, 1, ChapterRules.ChapterCount);
            _chapter = Content?.Chapter(_chapterIndex);
            if (_chapter == null)
            {
                Debug.LogError("[IronVeil] Chapter " + _chapterIndex + " is missing from the content.");
                return;
            }

            ArenaDefinition arena = _chapter.Arenas.Count > 0 ? _chapter.Arenas[0] : null;
            _level = LevelGeometry.Build(arena, _chapterIndex);
            _rng = new Rng(1000 + _chapterIndex * 37);

            _beatIndex = 0;
            if (!string.IsNullOrEmpty(beatId))
            {
                for (int i = 0; i < _chapter.Beats.Count; i++)
                {
                    if (_chapter.Beats[i].Id == beatId)
                    {
                        _beatIndex = i;
                        break;
                    }
                }
            }

            _player = new PlayerActor(this, Progression?.Character, Progression);
            Vector2 spawn = _level.PickSpawn("start", _rng, 4f);
            _player.Spawn(_level, spawn);
            _player.Died += HandlePlayerDied;

            BuildMarkers();
            _levelTimer = 0f;
            _deathsThisChapter = 0;
            _loaded = true;

            _cameraRig.SnapTo(spawn);
            Saves?.SetCheckpoint(_chapter.Checkpoints.Count > 0 ? _chapter.Checkpoints[0].Id : string.Empty, beatId);

            BeginBeat(0);
        }

        public void Unload()
        {
            if (_player != null)
            {
                _player.Died -= HandlePlayerDied;
                _player.Despawn();
                _player = null;
            }

            for (int i = 0; i < _enemies.Count; i++) _enemies[i]?.Dispose();
            _enemies.Clear();

            _boss?.Dispose();
            _boss = null;

            for (int i = 0; i < _markers.Count; i++)
            {
                if (_markers[i].Renderer != null) Destroy(_markers[i].Renderer.gameObject);
                if (_markers[i].Glyph != null) Destroy(_markers[i].Glyph.gameObject);
            }
            _markers.Clear();

            for (int i = 0; i < _hazards.Count; i++)
            {
                if (_hazards[i].Renderer != null) Destroy(_hazards[i].Renderer.gameObject);
            }
            _hazards.Clear();
            _beams.Clear();
            _damageables.Clear();

            _projectiles?.Clear();
            _effects?.Clear();
            _text?.Clear();
            _feedback?.Clear();
            _spawner?.Reset();

            _loaded = false;
            _currentBeat = null;
        }

        // ------------------------------------------------------------- markers

        private void BuildMarkers()
        {
            // Savepoints from the chapter definition, laid out along the run.
            for (int i = 0; i < _chapter.Savepoints.Count; i++)
            {
                SavepointDefinition savepoint = _chapter.Savepoints[i];
                float t = _chapter.Savepoints.Count <= 1 ? 0.5f : i / (float)(_chapter.Savepoints.Count - 1);
                float x = Mathf.Lerp(12f, _level.Bounds.xMax - 14f, t);
                AddMarker(savepoint.Id, string.IsNullOrEmpty(savepoint.Label) ? "Savepoint" : savepoint.Label, new Vector2(x, _level.GroundHeightAt(x)), savepoint, null);
            }

            // Pickups: flasks, ammo and salvage crates along the route.
            int pickups = 3 + _chapterIndex / 3;
            for (int i = 0; i < pickups; i++)
            {
                float x = _rng.Range(16f, _level.Bounds.xMax - 16f);
                float y = _level.GroundHeightAt(x);
                string[] kinds = { "flask", "ammo", "salvage" };
                string kind = kinds[i % kinds.Length];
                AddMarker("pickup_" + kind + "_" + i, kind, new Vector2(x, y), null, null);
            }
        }

        private void AddMarker(string id, string label, Vector2 position, SavepointDefinition savepoint, CheckpointDefinition checkpoint)
        {
            WorldMarker marker = new WorldMarker
            {
                Id = id,
                Label = label,
                Position = position,
                Savepoint = savepoint,
                Checkpoint = checkpoint,
                BobPhase = UnityEngine.Random.Range(0f, 6f),
                TriggerBox = new Rect(position.x - InteractRadius, position.y - InteractRadius, InteractRadius * 2f, InteractRadius * 2f)
            };

            GameObject root = new GameObject("Marker_" + id);
            root.transform.SetParent(ActorRoot, false);
            marker.Renderer = root.AddComponent<SpriteRenderer>();
            bool interactive = savepoint != null;
            marker.Renderer.sprite = interactive
                ? PlaceholderArt.Rect("marker_savepoint", new Color(0.35f, 0.75f, 0.95f, 0.85f), 10, 26, 16f)
                : PlaceholderArt.Rect("marker_pickup", new Color(0.9f, 0.8f, 0.4f, 0.85f), 14, 14, 16f);
            marker.Renderer.sortingOrder = 11;
            root.transform.position = new Vector3(position.x, position.y + 0.8f, 0f);

            GameObject glyph = new GameObject("Glyph");
            glyph.transform.SetParent(root.transform, false);
            marker.Glyph = glyph.AddComponent<SpriteRenderer>();
            marker.Glyph.sprite = PlaceholderArt.Circle("marker_glyph", new Color(1f, 1f, 1f, 0.5f), 12, 16f);
            marker.Glyph.sortingOrder = 12;
            glyph.transform.localPosition = new Vector3(0f, 1.1f, 0f);

            _markers.Add(marker);
        }

        // ------------------------------------------------------------- beats

        private void BeginBeat(int index)
        {
            if (_chapter == null || index >= _chapter.Beats.Count)
            {
                CompleteChapter();
                return;
            }

            _beatIndex = index;
            _currentBeat = _chapter.Beats[index];
            _beatCompletePending = false;
            _waitingForBeatInput = false;
            _puzzleInteracted = false;

            BeatChanged?.Invoke(_currentBeat);
            Screens?.SetObjective(_currentBeat.Title, _currentBeat.Objective);

            switch (_currentBeat.Kind)
            {
                case BeatKind.Comic:
                    PlayComicForBeat(_currentBeat);
                    break;

                case BeatKind.Dialogue:
                    PlayDialogueForBeat(_currentBeat);
                    break;

                case BeatKind.SideQuestHook:
                    HookSideQuest(_currentBeat);
                    break;

                case BeatKind.Choice:
                    ShowChoiceForBeat(_currentBeat);
                    break;

                case BeatKind.Arena:
                    StartArenaBeat(_currentBeat);
                    break;

                case BeatKind.MiniBossFight:
                case BeatKind.BossFight:
                    StartBossBeat(_currentBeat);
                    break;

                case BeatKind.Puzzle:
                    StartPuzzleBeat(_currentBeat);
                    break;

                case BeatKind.Savepoint:
                case BeatKind.Shop:
                    // Savepoint beats are resolved when the player interacts.
                    break;

                case BeatKind.ChapterComplete:
                    CompleteBeat();
                    break;

                case BeatKind.Exploration:
                default:
                    // Exploration resolves by reaching the far end of the arena.
                    break;
            }
        }

        private void PlayComicForBeat(ChapterBeat beat)
        {
            ComicPageDefinition page = FindComicPage(beat);
            if (page == null)
            {
                CompleteBeat();
                return;
            }

            Story?.MarkComicViewed(page);
            Story?.Grant("comic_" + page.BeatId);
            Screens?.ShowComic(page, () =>
            {
                WriteCheckpoint(CheckpointTrigger.AfterCutscene, beat.Id);
                CompleteBeat();
            });
            GameRoot.Instance?.ToState(GameFlowState.Comic);
        }

        private ComicPageDefinition FindComicPage(ChapterBeat beat)
        {
            if (_chapter == null || string.IsNullOrEmpty(beat.ReferenceId)) return null;
            for (int i = 0; i < _chapter.ComicPages.Count; i++)
            {
                if (_chapter.ComicPages[i].Id == beat.ReferenceId) return _chapter.ComicPages[i];
            }
            return Content?.ComicPage(beat.ReferenceId);
        }

        private void PlayDialogueForBeat(ChapterBeat beat)
        {
            CharacterDefinition character = Progression?.Character;
            List<string> lines = new List<string>();
            if (character != null)
            {
                string bark = Story?.RandomBossIntroLine(character);
                if (!string.IsNullOrEmpty(bark)) lines.Add(bark);
                for (int i = 0; i < character.LevelUpBarks.Length && lines.Count < 3; i++)
                {
                    if (!lines.Contains(character.LevelUpBarks[i])) lines.Add(character.LevelUpBarks[i]);
                }
            }
            if (lines.Count == 0) lines.Add("Move up.");

            Screens?.ShowDialogue(beat.Title, lines, () =>
            {
                GameRoot.Instance?.ToState(GameFlowState.Playing);
                CompleteBeat();
            });
            GameRoot.Instance?.ToState(GameFlowState.Comic);
        }

        private void HookSideQuest(ChapterBeat beat)
        {
            QuestDefinition quest = null;
            if (!string.IsNullOrEmpty(beat.ReferenceId)) quest = Content?.Quest(beat.ReferenceId);
            if (quest == null)
            {
                List<QuestDefinition> available = Quests?.QuestsInChapter(_chapterIndex) ?? new List<QuestDefinition>();
                for (int i = 0; i < available.Count; i++)
                {
                    if (Quests.StateOf(available[i].Id) == QuestState.Available)
                    {
                        quest = available[i];
                        break;
                    }
                }
            }

            if (quest != null)
            {
                Quests?.Accept(quest.Id);
                Screens?.ShowMessage("Side quest accepted", quest.Title + " - " + quest.Summary);
            }
            CompleteBeat();
        }

        private void ShowChoiceForBeat(ChapterBeat beat)
        {
            bool usingKade = Progression != null && Progression.Character != null && Progression.Character.Id == CharacterId.Kade;
            string[] options = usingKade
                ? new string[] { "Hold the line", "Push through" }
                : new string[] { "Slip past them", "Take the shot" };

            Screens?.ShowChoice(beat.Title, beat.Objective, options, index =>
            {
                Story?.Grant("choice_" + beat.Id + "_" + index);
                GameRoot.Instance?.ToState(GameFlowState.Playing);
                CompleteBeat();
            });
            GameRoot.Instance?.ToState(GameFlowState.Comic);
        }

        private void StartArenaBeat(ChapterBeat beat)
        {
            EncounterDefinition encounter = null;
            if (!string.IsNullOrEmpty(beat.ReferenceId)) encounter = Content?.Encounter(beat.ReferenceId);
            if (encounter == null)
            {
                List<EncounterDefinition> all = Content?.Content?.Encounters ?? new List<EncounterDefinition>();
                for (int i = 0; i < all.Count; i++)
                {
                    if (all[i].ChapterIndex == _chapterIndex && !all[i].IsBossArena)
                    {
                        encounter = all[i];
                        break;
                    }
                }
            }

            if (encounter == null)
            {
                // Authored content gap: still give the player something to fight.
                SummonAdds(PlayerPosition + new Vector2(9f, 1f), 3, string.Empty);
                _beatCompletePending = true;
                return;
            }

            _spawner.Begin(encounter);
            _beatCompletePending = true;
            WriteCheckpoint(CheckpointTrigger.EnterArena, beat.Id);
        }

        private void StartBossBeat(ChapterBeat beat)
        {
            BossDefinition definition = null;
            if (!string.IsNullOrEmpty(beat.ReferenceId)) definition = Content?.Boss(beat.ReferenceId);

            if (definition == null)
            {
                List<BossDefinition> bosses = Content?.Content?.MiniBossesInChapter(_chapterIndex) ?? new List<BossDefinition>();
                for (int i = 0; i < bosses.Count; i++)
                {
                    if (Story == null || !Story.HasDefeatedBoss(bosses[i].Id))
                    {
                        definition = bosses[i];
                        break;
                    }
                }
            }

            if (definition == null && !string.IsNullOrEmpty(_chapter.BossId)) definition = Content?.Boss(_chapter.BossId);
            if (definition == null)
            {
                CompleteBeat();
                return;
            }

            if (definition.IsMiniBoss && Story != null && Story.HasDefeatedBoss(definition.Id))
            {
                CompleteBeat();
                return;
            }
            if (!definition.IsMiniBoss && Story != null && Story.HasDefeatedBoss(definition.Id))
            {
                CompleteBeat();
                return;
            }

            // Pre-boss savepoint: give the player a full heal and an autosave.
            _player.RefillFlasks(3);
            _player.GrantInvulnerability(1.5f);
            WriteCheckpoint(CheckpointTrigger.PreBoss, beat.Id);
            Saves?.SaveAutosave("pre_boss");

            float spawnX = _level.Bounds.xMax - 20f;
            Vector2 spawn = new Vector2(spawnX, _level.GroundHeightAt(spawnX));
            _boss = new BossActor(this, definition, spawn);
            _bossFightTimer = 0f;
            _cameraRig.SetBossMode(true);
            if (Audio != null && !string.IsNullOrEmpty(definition.MusicId)) Audio.PlayMusic(definition.MusicId);
        }

        private void StartPuzzleBeat(ChapterBeat beat)
        {
            // Three interactable power nodes placed along the arena.
            for (int i = 0; i < 3; i++)
            {
                float x = Mathf.Lerp(20f, _level.Bounds.xMax - 24f, i / 2f);
                AddMarker("node_" + beat.Id + "_" + i, "Power Node", new Vector2(x, _level.GroundHeightAt(x)), null, null);
            }
            _puzzleInteracted = false;
        }

        /// <summary>Marks the current beat done, writes its checkpoint and advances.</summary>
        public void CompleteBeat()
        {
            if (_currentBeat == null) return;

            if (_currentBeat.AutosaveOnComplete && _currentBeat.Kind != BeatKind.Comic)
            {
                WriteCheckpoint(CheckpointTrigger.MidChapter, _currentBeat.Id);
            }

            int next = _beatIndex + 1;
            if (_chapter == null || next >= _chapter.Beats.Count)
            {
                CompleteChapter();
                return;
            }
            BeginBeat(next);
        }

        private void CompleteChapter()
        {
            if (_chapter != null)
            {
                Saves?.Current?.CompleteChapter("ch" + _chapter.Index.ToString("00"));
                if (!string.IsNullOrEmpty(_chapter.UnlockFlagOnComplete)) Story?.Grant(_chapter.UnlockFlagOnComplete);
            }
            WriteCheckpoint(CheckpointTrigger.ChapterComplete, string.Empty);
            Saves?.SaveAutosave("chapter_complete");
            Quests?.RefreshAvailability();
            _spawner?.Reset();
            GameRoot.Instance?.RequestChapterComplete();
        }

        private void WriteCheckpoint(CheckpointTrigger trigger, string beatId)
        {
            string checkpointId = string.Empty;
            if (_chapter != null)
            {
                for (int i = 0; i < _chapter.Checkpoints.Count; i++)
                {
                    CheckpointDefinition checkpoint = _chapter.Checkpoints[i];
                    if (checkpoint.Trigger != trigger) continue;
                    if (!string.IsNullOrEmpty(checkpoint.BeatId) && checkpoint.BeatId != beatId) continue;
                    checkpointId = checkpoint.Id;
                    break;
                }
            }

            Saves?.SetCheckpoint(checkpointId, string.IsNullOrEmpty(beatId) ? _currentBeat?.Id : beatId);
            if (Settings == null || Settings.Settings.AutoSaveOnCheckpoint) Saves?.SaveAutosave("checkpoint");
        }

        // ------------------------------------------------------------- tick

        public void Tick(float deltaTime)
        {
            if (!_loaded) return;

            float scaled = _feedback != null ? _feedback.Tick(deltaTime) : 1f;
            float dt = deltaTime * Mathf.Clamp(scaled, 0.02f, 1f);

            _levelTimer += deltaTime;
            _autosaveTimer += deltaTime;
            if (_autosaveTimer > 180f)
            {
                _autosaveTimer = 0f;
                Saves?.SaveAutosave("interval");
            }

            _player?.Tick(dt);

            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                EnemyActor enemy = _enemies[i];
                if (enemy == null || !enemy.IsAlive) continue;
                enemy.Tick(dt);
            }

            _boss?.Tick(dt);

            _spawner.Tick(dt);
            TickHazards(dt);
            TickBeams(dt);
            TickMarkers(dt);

            _projectiles.Tick(dt, _level, _damageables, _effects);
            _effects.Tick(dt);
            _text.Tick(dt);

            Quests?.TickTimed(dt);
            UpdateBeatProgress(dt);
            _cameraRig.Tick(deltaTime);
        }

        /// <summary>Non-gameplay frame: keeps particles and the camera alive for menus.</summary>
        public void TickIdle(float deltaTime)
        {
            if (!_loaded) return;
            _effects.Tick(deltaTime);
            _text.Tick(deltaTime);
            _projectiles.Tick(deltaTime, _level, _damageables, _effects);
            _cameraRig.Tick(deltaTime);
        }

        public void TickComic(float deltaTime)
        {
            if (!_loaded) return;
            _effects.Tick(deltaTime);
            _text.Tick(deltaTime);
            _cameraRig.Tick(deltaTime);
        }

        private void UpdateBeatProgress(float deltaTime)
        {
            if (_currentBeat == null) return;

            switch (_currentBeat.Kind)
            {
                case BeatKind.Exploration:
                    if (PlayerPosition.x > _level.Bounds.xMax - 8f)
                    {
                        CompleteBeat();
                    }
                    break;

                case BeatKind.Arena:
                    if (_spawner.IsFinished && AliveEnemyCount() == 0 && _beatCompletePending)
                    {
                        CompleteBeat();
                    }
                    break;

                case BeatKind.Puzzle:
                    break;

                case BeatKind.BossFight:
                case BeatKind.MiniBossFight:
                    _bossFightTimer += deltaTime;
                    break;
            }
        }

        public int AliveEnemyCount()
        {
            int alive = 0;
            for (int i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i] != null && _enemies[i].IsAlive) alive++;
            }
            return alive;
        }

        // ------------------------------------------------------------- hazards, beams, markers

        private void TickHazards(float deltaTime)
        {
            for (int i = _hazards.Count - 1; i >= 0; i--)
            {
                Hazard hazard = _hazards[i];
                hazard.TimeRemaining -= deltaTime;
                if (hazard.TimeRemaining <= 0f)
                {
                    if (hazard.Renderer != null) Destroy(hazard.Renderer.gameObject);
                    _hazards.RemoveAt(i);
                    continue;
                }

                hazard.TickTimer -= deltaTime;
                if (hazard.TickTimer <= 0f && _player != null && _player.IsAlive)
                {
                    if (Aabb2.CircleOverlaps(hazard.Position, hazard.Radius, _player.HitBox))
                    {
                        hazard.TickTimer = 0.5f;
                        DamagePlayer(hazard.Damage, hazard.Position, hazard.Tag);
                    }
                    else
                    {
                        hazard.TickTimer = 0.2f;
                    }
                }

                if (hazard.Renderer != null && !hazard.Renderer.enabled) hazard.Renderer.enabled = true;
            }
        }

        private void TickBeams(float deltaTime)
        {
            for (int i = _beams.Count - 1; i >= 0; i--)
            {
                BeamInstance beam = _beams[i];
                beam.TimeRemaining -= deltaTime;
                if (beam.TimeRemaining <= 0f)
                {
                    _beams.RemoveAt(i);
                    continue;
                }

                beam.TickTimer -= deltaTime;
                if (beam.TickTimer > 0f) continue;
                beam.TickTimer = 0.15f;

                IDamageable target = beam.Owner == Team.Enemy ? (IDamageable)_player : null;
                if (target == null || !target.IsAlive) continue;

                Vector2 toTarget = target.Center - beam.From;
                float along = Vector2.Dot(toTarget, beam.Direction);
                if (along < 0f || along > beam.Length) continue;
                Vector2 closest = beam.From + beam.Direction * along;
                float distance = (target.Center - closest).magnitude;
                if (distance > beam.Width + target.HitBox.Width * 0.5f) continue;

                if (beam.Owner == Team.Enemy) DamagePlayer(beam.Damage, closest, beam.Tag);
            }
        }

        private void TickMarkers(float deltaTime)
        {
            string bestPrompt = string.Empty;
            WorldMarker bestMarker = null;

            for (int i = 0; i < _markers.Count; i++)
            {
                WorldMarker marker = _markers[i];
                if (marker.Used) continue;

                marker.BobPhase += deltaTime * 2.4f;
                if (marker.Renderer != null)
                {
                    marker.Renderer.transform.position = new Vector3(marker.Position.x, marker.Position.y + 0.8f + Mathf.Sin(marker.BobPhase) * 0.18f, 0f);
                }

                if (_player == null || !_player.IsAlive) continue;
                if (!marker.Contains(_player.Body.Position)) continue;

                bestMarker = marker;
                bestPrompt = marker.IsSavepoint
                    ? "Use Savepoint - " + marker.Label
                    : marker.Id.StartsWith("node_", StringComparison.Ordinal)
                        ? "Activate Power Node"
                        : "Pick up " + marker.Label;
            }

            if (bestPrompt != InteractPrompt)
            {
                InteractPrompt = bestPrompt;
                InteractPromptChanged?.Invoke(bestPrompt);
                Screens?.SetInteractPrompt(bestPrompt);
            }

            _nearbyMarker = bestMarker;
        }

        private WorldMarker _nearbyMarker;

        /// <summary>Called when the player presses the interact key.</summary>
        public void TryInteract(PlayerActor player)
        {
            if (_nearbyMarker == null || _nearbyMarker.Used) return;

            WorldMarker marker = _nearbyMarker;
            if (marker.IsSavepoint)
            {
                Saves?.SetCheckpoint(marker.Id, _currentBeat?.Id);
                if (Settings == null || Settings.Settings.AutoSaveOnSavepoint) Saves?.SaveAutosave("savepoint");
                GameRoot.Instance?.OpenSavepoint(marker.Savepoint);
                return;
            }

            marker.Used = true;
            if (marker.Renderer != null) marker.Renderer.enabled = false;
            if (marker.Glyph != null) marker.Glyph.enabled = false;
            _nearbyMarker = null;
            Audio?.PlaySfx("sfx_pickup");

            if (marker.Id.StartsWith("node_", StringComparison.Ordinal))
            {
                HandlePuzzleNode(marker);
                return;
            }

            switch (marker.Id.StartsWith("pickup_flask") ? "flask" : marker.Id.StartsWith("pickup_ammo") ? "ammo" : "salvage")
            {
                case "flask":
                    player?.RefillFlasks(1);
                    Text?.Callout(marker.Position + new Vector2(0f, 1f), "Flask +1", Palette.Success);
                    break;
                case "ammo":
                    player?.GiveAmmo(120);
                    Text?.Callout(marker.Position + new Vector2(0f, 1f), "Ammo", Palette.AccentCool);
                    break;
                default:
                    Progression?.AddSalvage(40 + _chapterIndex * 6);
                    Text?.Callout(marker.Position + new Vector2(0f, 1f), "Salvage", Palette.Accent);
                    break;
            }
        }

        private void HandlePuzzleNode(WorldMarker marker)
        {
            int remaining = 0;
            for (int i = 0; i < _markers.Count; i++)
            {
                if (_markers[i].Id.StartsWith("node_", StringComparison.Ordinal) && !_markers[i].Used) remaining++;
            }

            Text?.Callout(marker.Position + new Vector2(0f, 1f), "Node online (" + (3 - remaining) + "/3)", Palette.AccentCool);
            if (remaining == 0) CompleteBeat();
        }

        public void ToggleWeaponWheel()
        {
            if (_player == null || _player.Weapons.Count <= 1) return;
            _player.CycleWeapon(1);
        }

        // ------------------------------------------------------------- combat plumbing

        public void RegisterDamageable(IDamageable damageable)
        {
            if (damageable == null || _damageables.Contains(damageable)) return;
            _damageables.Add(damageable);
        }

        public void UnregisterDamageable(IDamageable damageable)
        {
            _damageables.Remove(damageable);
        }

        public void NotifyStun(IDamageable target, float seconds)
        {
            if (target is EnemyActor enemy) enemy.ApplyStun(seconds);
            else if (target is BossActor boss) boss.ApplyStun(seconds);
        }

        public void NotifyPlayerDamaged()
        {
            Quests?.NoteDamageTaken();
        }

        private void HandlePlayerProjectileHit(Vector2 position)
        {
            _player?.NoteHit();
        }

        /// <summary>Damages the player from an enemy source and returns what happened.</summary>
        public DamageOutcome DamagePlayer(float damage, Vector2 sourcePosition, string tag)
        {
            if (_player == null || !_player.IsAlive) return new DamageOutcome();
            DamageOutcome outcome = _player.TakeEnemyDamage(damage, sourcePosition, tag);
            if (outcome.AnyDamage)
            {
                _text?.Damage(_player.Body.Position + new Vector2(0f, 1f), outcome, false);
                Quests?.Report(ObjectiveType.SurviveSeconds, tag, 0);
            }
            return outcome;
        }

        public void PushPlayer(Vector2 impulse)
        {
            if (_player?.Body == null) return;
            _player.Body.Velocity += impulse;
        }

        public void SpawnEnemyExplosion(Vector2 position, float radius, float damage, string tag)
        {
            _effects?.Explosion(position, radius);
            Feedback?.Shake(0.3f);
            if (_player == null || !_player.IsAlive) return;
            if (Aabb2.CircleOverlaps(position, radius, _player.HitBox))
            {
                float scaled = damage * (1f - Mathf.Clamp01((_player.Body.Position - position).magnitude / Mathf.Max(0.1f, radius)) * 0.5f);
                DamagePlayer(scaled, position, tag);
            }
        }

        public void SpawnHazard(Vector2 position, float radius, float duration, float damage, string tag)
        {
            GameObject root = new GameObject("Hazard");
            root.transform.SetParent(_hazardRoot.transform, false);
            root.transform.position = new Vector3(position.x, position.y, 0f);
            root.transform.localScale = Vector3.one * radius * 2f;

            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = PlaceholderArt.Circle("fx_hazard", new Color(1f, 0.45f, 0.3f, 0.45f), 32, 16f);
            renderer.sortingOrder = 10;

            _hazards.Add(new Hazard
            {
                Position = position,
                Radius = radius,
                TimeRemaining = duration,
                Damage = damage,
                Tag = tag,
                TickTimer = 0.3f,
                Renderer = renderer
            });
        }

        public void SpawnBeam(Vector2 from, Vector2 direction, float length, float width, float damage, float duration, string tag, Team owner)
        {
            Vector2 normal = direction.sqrMagnitude < 0.0001f ? Vector2.up : direction.normalized;
            _effects?.Beam(from, from + normal * length, width, Mathf.Max(0.12f, duration), owner == Team.Enemy
                ? new Color(1f, 0.4f, 0.35f, 0.8f)
                : new Color(0.5f, 0.85f, 1f, 0.8f));

            _beams.Add(new BeamInstance
            {
                From = from,
                Direction = normal,
                Length = length,
                Width = width,
                Damage = damage,
                TimeRemaining = Mathf.Max(0.1f, duration),
                Owner = owner,
                Tag = tag,
                TickTimer = 0f
            });
        }

        public void SummonAdds(Vector2 position, int count, string archetypeId)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 spawn = position + new Vector2(_rng.Range(-4f, 4f), 1f);
                spawn.x = Mathf.Clamp(spawn.x, _level.Bounds.xMin + 2f, _level.Bounds.xMax - 2f);
                spawn.y = Mathf.Max(spawn.y, _level.GroundHeightAt(spawn.x) + 1f);
                SpawnEnemy(archetypeId, spawn, false);
            }
        }

        public EnemyActor SpawnEnemy(string archetypeId, Vector2 position, bool forceElite)
        {
            EnemyArchetype archetype = ResolveArchetype(archetypeId);
            if (archetype == null) return null;

            EnemyActor enemy = new EnemyActor(this, archetype, position, _chapterIndex, forceElite, _rng);
            _enemies.Add(enemy);
            return enemy;
        }

        /// <summary>
        /// Maps a spawn request onto a real archetype: an explicit id, otherwise a
        /// chapter-appropriate one, otherwise the first enemy in the game. Keeps the
        /// content honest while still guaranteeing something spawns.
        /// </summary>
        private EnemyArchetype ResolveArchetype(string archetypeId)
        {
            CampaignContent content = Content?.Content;
            if (content == null) return null;

            if (!string.IsNullOrEmpty(archetypeId))
            {
                bool special = archetypeId == "summon" || archetypeId == "turret";
                if (!special)
                {
                    EnemyArchetype direct = content.Enemy(archetypeId);
                    if (direct != null) return direct;
                }
                else
                {
                    EnemyClass wanted = archetypeId == "turret" ? EnemyClass.Turret : EnemyClass.Swarm;
                    for (int i = 0; i < content.Enemies.Count; i++)
                    {
                        EnemyArchetype candidate = content.Enemies[i];
                        if (candidate.Class == wanted && candidate.FirstChapter <= _chapterIndex) return candidate;
                    }
                }
            }

            if (_chapter != null && _chapter.NewEnemyIds.Count > 0)
            {
                for (int i = 0; i < _chapter.NewEnemyIds.Count; i++)
                {
                    EnemyArchetype candidate = content.Enemy(_chapter.NewEnemyIds[i]);
                    if (candidate != null) return candidate;
                }
            }

            for (int i = 0; i < content.Enemies.Count; i++)
            {
                if (content.Enemies[i].FirstChapter <= _chapterIndex) return content.Enemies[i];
            }
            return content.Enemies.Count > 0 ? content.Enemies[0] : null;
        }

        // ------------------------------------------------------------- rewards and events

        public void OnEnemyKilled(EnemyActor enemy)
        {
            Saves?.RegisterKill(enemy.ArchetypeId, enemy.Archetype.Tier, enemy.IsElite);
            string gearDrop;
            Progression?.AwardKill(enemy.Archetype, _chapterIndex, enemy.IsElite, out gearDrop);

            Quests?.Report(ObjectiveType.KillCount, enemy.ArchetypeId, 1);
            if (enemy.IsElite) Quests?.Report(ObjectiveType.KillTagged, enemy.ArchetypeId, 1);

            if (!string.IsNullOrEmpty(gearDrop))
            {
                GearDefinition gear = Content?.Gear(gearDrop);
                Text?.Callout(enemy.Center + new Vector2(0f, 1f), gear == null ? "Gear found" : gear.DisplayName, Palette.Accent);
                Audio?.PlaySfx("sfx_loot");
            }

            if (!enemy.IsElite) _enemies.Remove(enemy);
        }

        public void OnBossIntro(BossActor boss)
        {
            BossDefinition definition = boss.Definition;
            Text?.Callout(boss.Center + new Vector2(0f, 2.4f), definition.DisplayName, Palette.Danger, 1.8f, 2.4f);
            Screens?.ShowBossIntro(definition);
            Feedback?.Shake(0.5f);
            if (!string.IsNullOrEmpty(definition.IntroComicId))
            {
                ComicPageDefinition page = Content?.ComicPage(definition.IntroComicId);
                if (page != null) Story?.MarkComicViewed(page);
            }
        }

        public void OnBossKilled(BossActor boss, float elapsedSeconds)
        {
            BossDefinition definition = boss.Definition;
            Progression?.AwardBoss(definition.Id, _chapterIndex, !definition.IsMiniBoss, elapsedSeconds);
            Quests?.Report(ObjectiveType.DefeatBoss, definition.Id, 1);
            Story?.Grant("boss_" + definition.Id);

            for (int i = 0; i < definition.UpgradeDrops.Count; i++)
            {
                Progression?.GrantWeapon(definition.UpgradeDrops[i]);
                WeaponDefinition weapon = Content?.Weapon(definition.UpgradeDrops[i]);
                if (weapon != null) Screens?.ShowMessage("Weapon acquired", weapon.DisplayName + " - " + weapon.Description);
            }
            for (int i = 0; i < definition.WeaponDrops.Count; i++)
            {
                Progression?.GrantWeapon(definition.WeaponDrops[i]);
                WeaponDefinition weapon = Content?.Weapon(definition.WeaponDrops[i]);
                if (weapon != null) Screens?.ShowMessage("Weapon acquired", weapon.DisplayName + " - " + weapon.Description);
            }

            _player?.RefreshStats();
            _boss = null;
            _cameraRig.SetBossMode(false);

            if (!string.IsNullOrEmpty(definition.OutroComicId))
            {
                ComicPageDefinition page = Content?.ComicPage(definition.OutroComicId);
                if (page != null)
                {
                    Story?.MarkComicViewed(page);
                    Screens?.ShowComic(page, CompleteBeat);
                    GameRoot.Instance?.ToState(GameFlowState.Comic);
                    Saves?.SaveAutosave("boss_clear");
                    return;
                }
            }

            Saves?.SaveAutosave("boss_clear");
            CompleteBeat();
        }

        private void HandlePlayerDied(PlayerActor player)
        {
            _deathsThisChapter++;
            GameRoot.Instance?.HandlePlayerDeath();
        }

        /// <summary>Restores the player to full health. Used by savepoints and rest beats.</summary>
        public void HealPlayer(float amount = -1f)
        {
            _player?.Heal(amount);
        }

        public void RespawnAtCheckpoint()
        {
            if (_player == null) return;

            Vector2 spawn = _level.PickSpawn("start", _rng, 4f);
            if (!string.IsNullOrEmpty(Saves?.Current?.CheckpointId))
            {
                for (int i = 0; i < _markers.Count; i++)
                {
                    if (_markers[i].Id == Saves.Current.CheckpointId) spawn = _markers[i].Position;
                }
            }

            for (int i = 0; i < _enemies.Count; i++) _enemies[i]?.Dispose();
            _enemies.Clear();
            _boss?.Dispose();
            _boss = null;
            _cameraRig.SetBossMode(false);
            _spawner?.Reset();
            _hazards.ClearInPlace(DestroyHazard);
            _beams.Clear();
            _projectiles?.Clear();

            _player.Spawn(_level, spawn);
            _player.RefillFlasks(1);
            _player.GrantInvulnerability(2f);
            _cameraRig.SnapTo(spawn);

            // Replay the current beat so checkpoints always restart at a known state.
            BeginBeat(_beatIndex);
        }

        private void DestroyHazard(Hazard hazard)
        {
            if (hazard.Renderer != null) Destroy(hazard.Renderer.gameObject);
        }

        public string ChapterSummary()
        {
            string elapsed = Mathf.FloorToInt(_levelTimer / 60f) + "m " + Mathf.FloorToInt(_levelTimer % 60f) + "s";
            return "Time " + elapsed + "   Deaths " + _deathsThisChapter +
                   "   Kills " + (Saves?.Current?.TotalKills ?? 0) +
                   "   Salvage " + (Progression?.Salvage ?? 0);
        }
    }

    /// <summary>Small helper so hazard cleanup reads clearly.</summary>
    internal static class HazardListExtensions
    {
        public static void ClearInPlace(this List<Hazard> list, Action<Hazard> onEach)
        {
            for (int i = 0; i < list.Count; i++) onEach?.Invoke(list[i]);
            list.Clear();
        }
    }
}

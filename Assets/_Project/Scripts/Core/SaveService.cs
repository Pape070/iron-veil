using System;
using System.Collections.Generic;
using System.IO;
using IronVeil.Rules;
using UnityEngine;

namespace IronVeil
{
    /// <summary>Summary shown in the load/continue list without decoding the whole save.</summary>
    public sealed class SaveSlotInfo
    {
        public string SlotId;
        public bool Exists;
        public bool IsAutosave;
        public bool Corrupt;
        public string Error;
        public string Summary;
        public string Character;
        public string Chapter;
        public int Level;
        public string Difficulty;
        public string PlayTime;
        public string SavedAt;
    }

    /// <summary>
    /// Owns the save files: three manual slots plus autosave and quick resume. Each
    /// write keeps the previous file as a backup, and a corrupted or hand edited
    /// save is detected by the checksum in the rules layer rather than crashing.
    /// </summary>
    public sealed class SaveService
    {
        public const string SaveFolder = "saves";
        public const string Extension = ".json";
        public const string BackupSuffix = ".bak";

        private readonly ContentDatabase _content;
        private SaveGame _current;
        private float _autosaveCooldown;
        private float _lastAutosaveTime = -999f;

        public SaveGame Current => _current;
        public bool HasCurrent => _current != null;

        /// <summary>Raised after any successful write, with the slot id.</summary>
        public event Action<string> Saved;

        /// <summary>Raised after an autosave so the HUD can flash the icon.</summary>
        public event Action<string> AutoSaved;

        public SaveService(ContentDatabase content)
        {
            _content = content;
        }

        public void Initialize()
        {
            try
            {
                Directory.CreateDirectory(SaveDirectory);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[IronVeil] Could not create the save folder: " + exception.Message);
            }
        }

        public static string SaveDirectory => Path.Combine(Application.persistentDataPath, SaveFolder);

        public static string PathFor(string slotId) => Path.Combine(SaveDirectory, slotId + Extension);

        public static string BackupPathFor(string slotId) => PathFor(slotId) + BackupSuffix;

        public void SetCurrent(SaveGame save)
        {
            _current = save;
            if (_current != null)
            {
                _current.Sanitize();
                _current.PruneUnknownContent(_content?.Content);
            }
        }

        public void ClearCurrent() => _current = null;

        public void TickPlayTime(float deltaTime)
        {
            if (_current == null) return;
            _current.PlayTimeSeconds += deltaTime;
        }

        // ------------------------------------------------------------- writing

        public bool SaveCurrent(bool isAutosave)
        {
            if (_current == null) return false;
            string slot = _current.IsAutosave
                ? SaveFormat.AutosaveSlotId
                : string.IsNullOrEmpty(_current.SlotId) ? "slot1" : _current.SlotId;
            return Save(slot, _current, isAutosave);
        }

        public bool Save(string slotId, SaveGame save, bool isAutosave)
        {
            if (save == null) return false;
            save.SlotId = slotId;
            save.IsAutosave = isAutosave;
            save.SavedAtUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            save.SaveCounter++;
            save.Sanitize();

            try
            {
                string path = PathFor(slotId);
                Directory.CreateDirectory(SaveDirectory);

                // Keep the previous file so a bad write cannot destroy progress.
                if (File.Exists(path))
                {
                    File.Copy(path, BackupPathFor(slotId), true);
                }

                string text = SaveCodec.Encode(save);
                string temporary = path + ".tmp";
                File.WriteAllText(temporary, text);

                // Verify before replacing: a truncated write would be worse than none.
                if (!SaveCodec.TryDecode(text, out _, out string error))
                {
                    Debug.LogWarning("[IronVeil] Refusing to write an unreadable save: " + error);
                    return false;
                }

                if (File.Exists(path)) File.Delete(path);
                File.Move(temporary, path);

                Saved?.Invoke(slotId);
                if (isAutosave) AutoSaved?.Invoke(slotId);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[IronVeil] Save failed for slot '" + slotId + "': " + exception.Message);
                return false;
            }
        }

        /// <summary>Autosave with a short cooldown so a run of checkpoints is not throttled to death.</summary>
        public bool SaveAutosave(string reason)
        {
            if (_current == null) return false;
            float now = Time.realtimeSinceStartup;
            if (now - _lastAutosaveTime < 1.5f) return false;
            _lastAutosaveTime = now;
            return Save(SaveFormat.AutosaveSlotId, _current, true);
        }

        public bool SaveQuickResume()
        {
            if (_current == null) return false;
            return Save(SaveFormat.QuickResumeSlotId, _current, true);
        }

        // ------------------------------------------------------------- reading

        public SaveGame Load(string slotId, out string error)
        {
            error = null;
            string path = PathFor(slotId);

            if (TryReadFile(path, out SaveGame save, out string primaryError))
            {
                save.PruneUnknownContent(_content?.Content);
                save.Sanitize();
                return save;
            }

            string backup = BackupPathFor(slotId);
            if (File.Exists(backup) && TryReadFile(backup, out SaveGame backupSave, out string backupError))
            {
                Debug.LogWarning("[IronVeil] Recovered slot '" + slotId + "' from its backup (" + primaryError + ").");
                backupSave.PruneUnknownContent(_content?.Content);
                backupSave.Sanitize();
                error = "Recovered from backup: " + primaryError;
                return backupSave;
            }

            error = primaryError ?? backupError ?? "save file not found";
            return null;
        }

        private static bool TryReadFile(string path, out SaveGame save, out string error)
        {
            save = null;
            error = null;
            try
            {
                if (!File.Exists(path))
                {
                    error = "save file not found";
                    return false;
                }
                string text = File.ReadAllText(path);
                return SaveCodec.TryDecode(text, out save, out error);
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
        }

        public bool Exists(string slotId) => File.Exists(PathFor(slotId));

        public bool Delete(string slotId)
        {
            try
            {
                if (File.Exists(PathFor(slotId))) File.Delete(PathFor(slotId));
                if (File.Exists(BackupPathFor(slotId))) File.Delete(BackupPathFor(slotId));
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[IronVeil] Could not delete slot '" + slotId + "': " + exception.Message);
                return false;
            }
        }

        public List<SaveSlotInfo> ListSlots()
        {
            List<SaveSlotInfo> slots = new List<SaveSlotInfo>();
            for (int i = 1; i <= SaveFormat.ManualSlotCount; i++) slots.Add(Describe("slot" + i));
            slots.Add(Describe(SaveFormat.AutosaveSlotId));
            slots.Add(Describe(SaveFormat.QuickResumeSlotId));
            return slots;
        }

        public SaveSlotInfo Describe(string slotId)
        {
            SaveSlotInfo info = new SaveSlotInfo
            {
                SlotId = slotId,
                IsAutosave = slotId == SaveFormat.AutosaveSlotId || slotId == SaveFormat.QuickResumeSlotId,
                Exists = Exists(slotId)
            };

            if (!info.Exists) return info;

            SaveGame save = Load(slotId, out string error);
            if (save == null)
            {
                info.Corrupt = true;
                info.Error = error;
                info.Summary = "Corrupted save";
                return info;
            }

            CharacterDefinition character = _content?.Character(save.Character);
            ChapterDefinition chapter = _content?.Chapter(save.ChapterIndex);
            info.Character = character == null ? save.Character.ToString() : character.Name;
            info.Chapter = chapter == null ? "Chapter " + save.ChapterIndex : "Ch " + save.ChapterIndex + " - " + chapter.Title;
            info.Level = save.Level;
            info.Difficulty = DifficultyProfile.For(save.Difficulty).DisplayName;
            info.PlayTime = save.FormatPlayTime();
            info.SavedAt = save.SavedAtUnixSeconds > 0
                ? DateTimeOffset.FromUnixTimeSeconds(save.SavedAtUnixSeconds).ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                : "unknown";
            info.Summary = save.Summary(_content?.Content);
            return info;
        }

        /// <summary>The newest readable save, used by "Continue" on the main menu.</summary>
        public string NewestSlotId()
        {
            string best = null;
            long bestTime = -1;
            List<string> candidates = new List<string> { SaveFormat.AutosaveSlotId, SaveFormat.QuickResumeSlotId };
            for (int i = 1; i <= SaveFormat.ManualSlotCount; i++) candidates.Add("slot" + i);

            for (int i = 0; i < candidates.Count; i++)
            {
                SaveGame save = Load(candidates[i], out _);
                if (save == null) continue;
                if (save.SavedAtUnixSeconds > bestTime)
                {
                    bestTime = save.SavedAtUnixSeconds;
                    best = candidates[i];
                }
            }
            return best;
        }

        // ------------------------------------------------------------- run stats

        public void RegisterDeath()
        {
            if (_current == null) return;
            _current.Deaths++;
            _current.RecordFor(_current.BeatId).Deaths++;
            SaveAutosave("death");
        }

        public void RegisterKill(string archetypeId, int tier, bool elite)
        {
            if (_current == null) return;
            _current.TotalKills++;
            if (elite) _current.EliteKills++;
        }

        public void RegisterShot() => _current?.ShotsFired++;

        public void RegisterBossAttempt(string bossId) => _current?.RegisterBossAttempt(bossId);

        public void RegisterBossClear(string bossId, float seconds)
        {
            _current?.RegisterBossClear(bossId, seconds);
        }

        public void RegisterMiniBossClear(string bossId) => _current?.DefeatMiniBoss(bossId);

        public void SetCheckpoint(string checkpointId, string beatId)
        {
            if (_current == null) return;
            _current.CheckpointId = checkpointId;
            if (!string.IsNullOrEmpty(beatId)) _current.BeatId = beatId;
        }
    }
}

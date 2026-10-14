using System;
using System.Collections.Generic;

namespace IronVeil.Rules
{
    public enum QuestKind
    {
        MainQuest = 0,
        SideQuest = 1,
        Bounty = 2,
        Collection = 3,
        Escort = 4
    }

    public enum ObjectiveType
    {
        KillCount = 0,
        KillTagged = 1,
        CollectItem = 2,
        ReachArea = 3,
        SurviveSeconds = 4,
        InteractWith = 5,
        DestroyTarget = 6,
        DefeatBoss = 7,
        EscortTo = 8,
        NoDamageRun = 9,
        UseAbilityCount = 10,
        HitWeakPointCount = 11
    }

    public enum QuestState
    {
        Locked = 0,
        Available = 1,
        Active = 2,
        ReadyToTurnIn = 3,
        Completed = 4,
        Failed = 5
    }

    [Serializable]
    public sealed class QuestObjective
    {
        public string Id = string.Empty;
        public ObjectiveType Type = ObjectiveType.KillCount;

        /// <summary>Enemy archetype, item, area, boss or npc id, depending on Type.</summary>
        public string TargetId = string.Empty;

        public int RequiredCount = 1;
        public string Description = string.Empty;
        public bool Optional = false;
        public bool HiddenUntilProgress = false;

        public string ProgressText(int current)
        {
            int capped = MathLite.Clamp(current, 0, RequiredCount);
            if (RequiredCount <= 1) return Description;
            return Description + " (" + capped + "/" + RequiredCount + ")";
        }
    }

    [Serializable]
    public sealed class QuestDefinition
    {
        public string Id = string.Empty;
        public string Title = string.Empty;
        public string Summary = string.Empty;
        public QuestKind Kind = QuestKind.SideQuest;
        public int ChapterIndex = 1;
        public string GiverName = string.Empty;
        public string LocationHint = string.Empty;

        public List<QuestObjective> Objectives = new List<QuestObjective>();

        public int XpReward = 500;
        public int SalvageReward = 60;
        public string GearRewardId = string.Empty;
        public string WeaponRewardId = string.Empty;
        public string PerkRewardId = string.Empty;

        /// <summary>Story flags required before the quest appears.</summary>
        public string[] PrerequisiteFlags = Array.Empty<string>();

        /// <summary>Other quests that must be completed first.</summary>
        public string[] PrerequisiteQuestIds = Array.Empty<string>();

        /// <summary>Flags granted on completion, which can unlock doors or dialogue.</summary>
        public string[] GrantedFlags = Array.Empty<string>();

        public bool IsRepeatable = false;
        public int EstimatedMinutes = 12;
        public string TurnInName = string.Empty;

        /// <summary>Fails automatically if the player leaves the chapter area.</summary>
        public bool FailsOnChapterExit = false;

        public QuestObjective PrimaryObjective => Objectives == null || Objectives.Count == 0 ? null : Objectives[0];
    }

    /// <summary>Live progress for one accepted quest.</summary>
    [Serializable]
    public sealed class QuestProgress
    {
        public string QuestId = string.Empty;
        public QuestState State = QuestState.Locked;
        public List<int> ObjectiveCounts = new List<int>();
        public float ElapsedSeconds = 0f;
        public bool TookDamage = false;

        public void SyncObjectiveCount(int index)
        {
            while (ObjectiveCounts.Count <= index)
            {
                ObjectiveCounts.Add(0);
            }
        }

        public int CountAt(int index)
        {
            if (index < 0 || index >= ObjectiveCounts.Count) return 0;
            return ObjectiveCounts[index];
        }
    }

    /// <summary>
    /// Quest rules and the runtime tracker. All progress reporting funnels through
    /// here so the UI, HUD and save system never disagree about quest state.
    /// </summary>
    public static class QuestRules
    {
        public static bool RequirementsMet(
            QuestDefinition quest,
            ICollection<string> completedQuestIds,
            ICollection<string> storyFlags)
        {
            if (quest == null) return false;

            if (quest.PrerequisiteQuestIds != null)
            {
                for (int i = 0; i < quest.PrerequisiteQuestIds.Length; i++)
                {
                    string id = quest.PrerequisiteQuestIds[i];
                    if (string.IsNullOrEmpty(id)) continue;
                    if (completedQuestIds == null || !completedQuestIds.Contains(id)) return false;
                }
            }

            if (quest.PrerequisiteFlags != null)
            {
                for (int i = 0; i < quest.PrerequisiteFlags.Length; i++)
                {
                    string flag = quest.PrerequisiteFlags[i];
                    if (string.IsNullOrEmpty(flag)) continue;
                    if (storyFlags == null || !storyFlags.Contains(flag)) return false;
                }
            }

            return true;
        }

        /// <summary>True when every non-optional objective is satisfied.</summary>
        public static bool IsComplete(QuestDefinition quest, QuestProgress progress)
        {
            if (quest == null || progress == null || quest.Objectives == null) return false;

            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                QuestObjective objective = quest.Objectives[i];
                if (objective == null || objective.Optional) continue;
                if (progress.CountAt(i) < objective.RequiredCount) return false;
            }
            return quest.Objectives.Count > 0;
        }

        /// <summary>Completion ratio in [0, 1], used by the quest log progress bar.</summary>
        public static float CompletionPercent(QuestDefinition quest, QuestProgress progress)
        {
            if (quest == null || progress == null || quest.Objectives == null || quest.Objectives.Count == 0) return 0f;

            float total = 0f;
            int counted = 0;
            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                QuestObjective objective = quest.Objectives[i];
                if (objective == null || objective.RequiredCount <= 0) continue;
                counted++;
                total += MathLite.Clamp01(progress.CountAt(i) / (float)objective.RequiredCount);
            }
            return counted == 0 ? 0f : total / counted;
        }

        /// <summary>Adds progress to matching objectives. Returns true when the state changed.</summary>
        public static bool Report(
            QuestDefinition quest,
            QuestProgress progress,
            ObjectiveType type,
            string targetId,
            int amount,
            out List<int> changedObjectives)
        {
            changedObjectives = new List<int>();
            if (quest == null || progress == null || amount <= 0) return false;
            if (progress.State != QuestState.Active) return false;

            bool changed = false;
            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                QuestObjective objective = quest.Objectives[i];
                if (objective == null || objective.Type != type) continue;
                if (!TargetMatches(objective.TargetId, targetId)) continue;

                progress.SyncObjectiveCount(i);
                int before = progress.ObjectiveCounts[i];
                int after = MathLite.Min(objective.RequiredCount, before + amount);
                if (after == before) continue;

                progress.ObjectiveCounts[i] = after;
                changedObjectives.Add(i);
                changed = true;
            }

            if (!changed) return false;

            if (IsComplete(quest, progress))
            {
                progress.State = QuestState.ReadyToTurnIn;
            }
            return true;
        }

        /// <summary>Advances timed objectives (survive / no-damage runs).</summary>
        public static bool TickTimed(QuestDefinition quest, QuestProgress progress, float deltaTime)
        {
            if (quest == null || progress == null || progress.State != QuestState.Active) return false;
            progress.ElapsedSeconds += deltaTime;

            bool changed = false;
            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                QuestObjective objective = quest.Objectives[i];
                if (objective == null || objective.Type != ObjectiveType.SurviveSeconds) continue;

                progress.SyncObjectiveCount(i);
                int target = objective.RequiredCount;
                int now = MathLite.Clamp((int)progress.ElapsedSeconds, 0, target);
                if (now != progress.ObjectiveCounts[i])
                {
                    progress.ObjectiveCounts[i] = now;
                    changed = true;
                }
            }

            if (changed && IsComplete(quest, progress)) progress.State = QuestState.ReadyToTurnIn;
            return changed;
        }

        /// <summary>Marks a no-damage objective failed without failing the whole quest.</summary>
        public static void NoteDamageTaken(QuestDefinition quest, QuestProgress progress)
        {
            if (quest == null || progress == null) return;
            progress.TookDamage = true;
            if (quest.Objectives == null) return;

            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                QuestObjective objective = quest.Objectives[i];
                if (objective == null || objective.Type != ObjectiveType.NoDamageRun) continue;
                if (objective.Optional) continue;
                progress.State = QuestState.Failed;
                return;
            }
        }

        public static bool TargetMatches(string expected, string actual)
        {
            if (string.IsNullOrEmpty(expected)) return true; // wildcard objective
            if (string.IsNullOrEmpty(actual)) return false;
            return string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);
        }

        public static QuestProgress NewProgress(QuestDefinition quest)
        {
            QuestProgress progress = new QuestProgress { QuestId = quest == null ? string.Empty : quest.Id };
            int count = quest == null || quest.Objectives == null ? 0 : quest.Objectives.Count;
            for (int i = 0; i < count; i++) progress.ObjectiveCounts.Add(0);
            return progress;
        }

        /// <summary>Reward summary used by the quest log and the turn-in screen.</summary>
        public static string RewardSummary(QuestDefinition quest)
        {
            if (quest == null) return string.Empty;
            List<string> parts = new List<string>();
            if (quest.XpReward > 0) parts.Add(quest.XpReward + " XP");
            if (quest.SalvageReward > 0) parts.Add(quest.SalvageReward + " salvage");
            if (!string.IsNullOrEmpty(quest.WeaponRewardId)) parts.Add("weapon");
            if (!string.IsNullOrEmpty(quest.GearRewardId)) parts.Add("gear");
            if (!string.IsNullOrEmpty(quest.PerkRewardId)) parts.Add("perk");
            return string.Join(", ", parts.ToArray());
        }
    }
}

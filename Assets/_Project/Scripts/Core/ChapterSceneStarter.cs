using IronVeil.Rules;
using UnityEngine;

namespace IronVeil
{
    /// <summary>
    /// Drop this on a GameObject in a chapter scene to boot straight into that
    /// chapter. GameRoot creates itself automatically before the scene loads, so a
    /// chapter scene needs nothing else in it.
    /// </summary>
    public sealed class ChapterSceneStarter : MonoBehaviour
    {
        [Tooltip("1-12. Which chapter this scene plays.")]
        public int ChapterIndex = 1;

        [Tooltip("Start a fresh run for this chapter when no save is loaded.")]
        public bool StartNewGameIfNeeded = true;

        [Tooltip("Character used when starting a fresh run from this scene.")]
        public CharacterId Character = CharacterId.Kade;

        [Tooltip("Difficulty used when starting a fresh run from this scene.")]
        public Difficulty Difficulty = Difficulty.Normal;

        [Tooltip("Level granted when starting a fresh run, so a late chapter is playable immediately.")]
        public int StartingLevel = 1;

        private void Start()
        {
            GameRoot root = GameRoot.Instance;
            if (root == null) return;
            if (root.Chapter != null && root.Chapter.IsLoaded) return;

            if (StartNewGameIfNeeded && !(root.Saves != null && root.Saves.HasCurrent))
            {
                root.StartNewGame(Character, Difficulty);

                SaveGame save = root.Saves?.Current;
                if (save != null && StartingLevel > 1)
                {
                    save.Level = Mathf.Clamp(StartingLevel, 1, StatComputation.MaxLevel);
                    save.Sanitize();
                    root.Progression?.Rebuild();
                }
            }

            if (ChapterIndex <= 1) return;

            SaveGame current = root.Saves?.Current;
            if (current == null) return;

            current.ChapterIndex = Mathf.Clamp(ChapterIndex, 1, ChapterRules.ChapterCount);
            ChapterDefinition chapter = root.Content?.Chapter(current.ChapterIndex);
            current.BeatId = chapter != null && chapter.Beats.Count > 0 ? chapter.Beats[0].Id : current.BeatId;
            current.CheckpointId = string.Empty;
            root.Chapter?.LoadChapter(current.ChapterIndex, current.BeatId, true);
        }
    }
}

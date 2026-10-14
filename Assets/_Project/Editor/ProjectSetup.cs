using System.Collections.Generic;
using System.IO;
using System.Text;
using IronVeil.Rules;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IronVeil.EditorTools
{
    /// <summary>
    /// One-stop setup and validation tooling. Everything the project needs to open
    /// and play is generated from here, so no manual scene or inspector wiring is
    /// required after cloning.
    /// </summary>
    public static class ProjectSetup
    {
        private const string ScenesFolder = "Assets/_Project/Scenes";
        private const string ProductName = "Iron Veil";

        [MenuItem("Iron Veil/Setup/Apply Project Settings", false, 10)]
        public static void ApplyProjectSettings()
        {
            bool changed = false;

            if (PlayerSettings.productName != ProductName)
            {
                PlayerSettings.productName = ProductName;
                changed = true;
            }
            if (string.IsNullOrEmpty(PlayerSettings.companyName) || PlayerSettings.companyName == "DefaultCompany")
            {
                PlayerSettings.companyName = "Iron Veil";
                changed = true;
            }

            changed |= SetActiveInputHandler(2); // 2 = Input System Package (New)

            QualitySettings.vSyncCount = 1;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.runInBackground = true;

            AssetDatabase.SaveAssets();
            Debug.Log("[IronVeil] Project settings applied" + (changed ? " (a restart may be required for the input backend)." : "."));
            EditorUtility.DisplayDialog("Iron Veil",
                changed
                    ? "Project settings applied.\n\nIf Unity asks to restart for the new input backend, say yes."
                    : "Project settings were already correct.",
                "OK");
        }

        /// <summary>
        /// Switches Active Input Handling. Unity exposes this only through the
        /// serialised PlayerSettings object, hence the reflection-free serialized
        /// property lookup.
        /// </summary>
        private static bool SetActiveInputHandler(int mode)
        {
            try
            {
                Object playerSettings = Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings");
                if (playerSettings == null) return false;

                SerializedObject serialized = new SerializedObject(playerSettings);
                SerializedProperty property = serialized.FindProperty("activeInputHandler");
                if (property == null) return false;
                if (property.intValue == mode) return false;

                property.intValue = mode;
                serialized.ApplyModifiedProperties();
                return true;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("[IronVeil] Could not set the input backend automatically: " + exception.Message +
                                 "\nSet Edit > Project Settings > Player > Active Input Handling to 'Input System Package (New)'.");
                return false;
            }
        }

        [MenuItem("Iron Veil/Setup/Create Scenes", false, 11)]
        public static void CreateScenes()
        {
            if (!EditorUtility.DisplayDialog("Iron Veil",
                    "Create and rebuild the project scenes?\n\nThis overwrites scenes in " + ScenesFolder +
                    " and switches the open scene.", "Create", "Cancel"))
            {
                return;
            }

            Directory.CreateDirectory(ScenesFolder);

            List<string> created = new List<string>();
            EditorBuildSettingsScene[] existing = EditorBuildSettings.scenes;
            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();

            string bootPath = ScenesFolder + "/Boot.unity";
            CreateScene(bootPath, 0, 1, "Boot");
            created.Add(bootPath);
            buildScenes.Add(new EditorBuildSettingsScene(bootPath, true));

            CampaignContent content = CampaignContent.Build();
            for (int i = 1; i <= ChapterRules.ChapterCount; i++)
            {
                ChapterDefinition chapter = content.Chapter(i);
                string safeTitle = chapter == null ? "Chapter" : Sanitise(chapter.Title);
                string path = ScenesFolder + "/Chapter" + i.ToString("00") + "_" + safeTitle + ".unity";
                CreateScene(path, i, ChapterRules.RecommendedLevel(i), "Chapter " + i.ToString("00"));
                created.Add(path);
                buildScenes.Add(new EditorBuildSettingsScene(path, true));
            }

            string sandboxPath = ScenesFolder + "/Sandbox.unity";
            CreateScene(sandboxPath, 1, 60, "Sandbox");
            created.Add(sandboxPath);
            buildScenes.Add(new EditorBuildSettingsScene(sandboxPath, true));

            // Keep any scenes the project already had, without duplicating ours.
            for (int i = 0; i < existing.Length; i++)
            {
                if (!created.Contains(existing[i].path)) buildScenes.Add(existing[i]);
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
            AssetDatabase.Refresh();

            Debug.Log("[IronVeil] Created " + created.Count + " scenes and added them to the build list.");
            EditorUtility.DisplayDialog("Iron Veil", "Created " + created.Count + " scenes.", "OK");
        }

        private static void CreateScene(string path, int chapterIndex, int startingLevel, string name)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject root = new GameObject(name);
            if (chapterIndex > 0)
            {
                ChapterSceneStarter starter = root.AddComponent<ChapterSceneStarter>();
                starter.ChapterIndex = chapterIndex;
                starter.StartingLevel = Mathf.Max(1, startingLevel);
                starter.StartNewGameIfNeeded = true;
            }
            else
            {
                // The boot scene only needs the game root, which also self-creates.
                root.AddComponent<BootMarker>();
            }

            EditorSceneManager.SaveScene(scene, path);
        }

        private static string Sanitise(string title)
        {
            if (string.IsNullOrEmpty(title)) return "Chapter";
            StringBuilder builder = new StringBuilder(title.Length);
            for (int i = 0; i < title.Length; i++)
            {
                char c = title[i];
                builder.Append(char.IsLetterOrDigit(c) ? c : '_');
            }
            return builder.ToString();
        }

        [MenuItem("Iron Veil/Validate/Campaign Content", false, 30)]
        public static void ValidateContent()
        {
            CampaignContent content = CampaignContent.Build();
            ValidationReport report = ContentValidator.Validate(content);

            string message = report.Summary();
            Debug.Log("[IronVeil] Validation:\n" + message);

            string title = report.IsValid ? "Content valid" : "Content has errors";
            EditorUtility.DisplayDialog(title,
                message.Length > 3000 ? message.Substring(0, 3000) + "\n..." : message, "OK");
        }

        [MenuItem("Iron Veil/Validate/Project Structure", false, 31)]
        public static void ValidateStructure()
        {
            StringBuilder builder = new StringBuilder();
            int problems = 0;

            problems += CheckAsset(builder, "Assets/_Project/Resources/GameInput.inputactions", "input actions asset");
            problems += CheckAsset(builder, "Assets/_Project/Scripts/IronVeil.Game.asmdef", "game assembly definition");
            problems += CheckAsset(builder, "Assets/_Project/Scripts/Rules/IronVeil.Rules.asmdef", "rules assembly definition");
            problems += CheckAsset(builder, "Assets/_Project/Tests/EditMode/IronVeil.Tests.EditMode.asmdef", "test assembly definition");
            problems += CheckAsset(builder, "Packages/manifest.json", "package manifest");
            problems += CheckAsset(builder, "ProjectSettings/ProjectVersion.txt", "editor version file");

            string[] scripts =
            {
                "Assets/_Project/Scripts/Core/GameRoot.cs",
                "Assets/_Project/Scripts/Input/InputService.cs",
                "Assets/_Project/Scripts/Gameplay/ChapterRuntime.cs",
                "Assets/_Project/Scripts/UI/ScreenManager.cs",
                "Assets/_Project/Scripts/Story/ComicPlayer.cs"
            };
            for (int i = 0; i < scripts.Length; i++)
            {
                problems += CheckAsset(builder, scripts[i], "core script");
            }

            CampaignContent content = CampaignContent.Build();
            ValidationReport report = ContentValidator.Validate(content);
            if (!report.IsValid) problems += report.Errors.Count;

            builder.Insert(0, problems == 0
                ? "Project structure looks good.\n\n"
                : problems + " problem(s) found.\n\n");

            Debug.Log("[IronVeil] Structure check:\n" + builder);
            EditorUtility.DisplayDialog(problems == 0 ? "Structure OK" : "Structure problems", builder.ToString(), "OK");
        }

        private static int CheckAsset(StringBuilder builder, string path, string label)
        {
            bool exists = File.Exists(path) || Directory.Exists(path);
            builder.Append(exists ? "  OK   " : "  MISSING   ").Append(label).Append(" (").Append(path).Append(")\n");
            return exists ? 0 : 1;
        }

        [MenuItem("Iron Veil/Report/Balance Report", false, 50)]
        public static void BalanceReport()
        {
            CampaignContent content = CampaignContent.Build();
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Chapter pacing (Normal difficulty)");
            builder.AppendLine("Ch   Name                          Enemy HP   Boss HP   Boss/Player HP   Recommended level");

            DifficultyProfile difficulty = DifficultyProfile.For(Difficulty.Normal);
            for (int i = 0; i < content.Chapters.Count; i++)
            {
                ChapterDefinition chapter = content.Chapters[i];
                float enemyHealth = 45f * ChapterRules.EnemyHealthMultiplier(chapter.Index) * difficulty.EnemyHealthMultiplier;

                BossDefinition boss = content.Boss(chapter.BossId);
                float bossHealth = boss == null ? 0f : boss.BaseHealth * ChapterRules.EnemyHealthMultiplier(chapter.Index) * difficulty.EnemyHealthMultiplier;

                CharacterDefinition character = content.Characters.Count > 0 ? content.Characters[0] : null;
                float playerHealth = character == null
                    ? 1f
                    : StatComputation.Compute(character.BaseStats, character.PerLevelGrowth,
                        ChapterRules.RecommendedLevel(chapter.Index), new Dictionary<StatId, int>()).MaxHealth;

                builder.AppendFormat("{0,-4} {1,-28} {2,-10:0} {3,-9:0} {4,-16:0.0} {5}\n",
                    chapter.Index,
                    Truncate(chapter.Title, 28),
                    enemyHealth,
                    bossHealth,
                    bossHealth / Mathf.Max(1f, playerHealth),
                    chapter.RecommendedLevel);
            }

            builder.AppendLine();
            builder.AppendLine("Total main path: " + ChapterRules.TotalMinutes() + " minutes (" +
                               (ChapterRules.TotalMinutes() / 60f).ToString("0.0") + " hours).");

            Debug.Log("[IronVeil] Balance report:\n" + builder);
            EditorUtility.DisplayDialog("Balance report", builder.ToString(), "OK");
        }

        private static string Truncate(string text, int length)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Length <= length ? text : text.Substring(0, length - 1) + "…";
        }
    }

    /// <summary>Marks the boot scene so it is obvious that GameRoot self-creates.</summary>
    public sealed class BootMarker : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("[IronVeil] Boot scene: GameRoot created itself; the main menu is live.");
        }
    }
}

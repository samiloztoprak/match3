using System;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tests.EditMode
{
    public class CollectionWheelTest
    {
        private const string SaveKey = "Meta_Progress_v1";
        private const string WheelPath = "Assets/Data/VisualConfig/WheelConfig.asset";

        private sealed class PackRandom : IRandom
        {
            private readonly int _wheelRoll;
            private bool _selectingWheel = true;
            public PackRandom(int wheelRoll) => _wheelRoll = wheelRoll;

            public int Next(int maxExclusive)
            {
                if (!_selectingWheel) return 0;
                _selectingWheel = false;
                return _wheelRoll;
            }
        }

        [Test]
        public void Last_card_spin_persists_rewards_and_next_pack_gives_stars()
        {
            // The menu scripts live in Assembly-CSharp while these tests use an asmdef.
            var wheel = AssetDatabase.LoadMainAssetAtPath(WheelPath);
            Assert.IsNotNull(wheel);
            var config = new SerializedObject(wheel);
            SerializedProperty segments = config.FindProperty("_segments");
            int packRoll = 0;
            int packIndex = -1;
            for (int i = 0; i < segments.arraySize; i++)
            {
                SerializedProperty segment = segments.GetArrayElementAtIndex(i);
                int weight = segment.FindPropertyRelative("Weight").intValue;
                if (segment.FindPropertyRelative("Type").enumValueIndex == 1 && weight > 0)
                {
                    var icon = segment.FindPropertyRelative("Icon").objectReferenceValue as Sprite;
                    Assert.IsNotNull(icon);
                    packIndex = i;
                    break;
                }
                packRoll += Math.Max(0, weight);
            }
            Assert.GreaterOrEqual(packIndex, 0);

            var catalogAsset = config.FindProperty("_collection").objectReferenceValue;
            Assert.IsNotNull(catalogAsset);
            var catalog = new SerializedObject(catalogAsset);
            var sets = catalog.FindProperty("_sets");
            Assert.AreEqual(1, sets.arraySize);
            var set = sets.GetArrayElementAtIndex(0);
            var cards = set.FindPropertyRelative("Cards");
            string setId = set.FindPropertyRelative("Id").stringValue;
            var initial = new MetaProgressState { Spins = 2, StarBank = 8 };
            for (int i = 0; i < cards.arraySize; i++)
            {
                var card = cards.GetArrayElementAtIndex(i);
                Assert.IsNotNull(card.FindPropertyRelative("Art").objectReferenceValue);
                if (i > 0) initial.OwnedCards.Add(card.FindPropertyRelative("Id").stringValue);
            }

            Assembly presentation = wheel.GetType().Assembly;
            FieldInfo stateField = presentation.GetType("MetaProgress")
                .GetField("_state", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo trySpin = presentation.GetType("WheelService").GetMethod("TrySpin");
            MethodInfo rewardAt = presentation.GetType("WheelService").GetMethod("RewardAt");
            object cached = stateField.GetValue(null);
            bool hadSave = PlayerPrefs.HasKey(SaveKey);
            string previous = PlayerPrefs.GetString(SaveKey);
            const string levelKey = "CurrentLevelIndex";
            bool hadLevel = PlayerPrefs.HasKey(levelKey);
            int previousLevel = PlayerPrefs.GetInt(levelKey);

            try
            {
                stateField.SetValue(null, initial);
                PlayerPrefs.SetInt(levelKey, 9);
                object lockedReward = rewardAt.Invoke(null, new object[] { wheel, packIndex });
                Assert.AreEqual("Stars", lockedReward.GetType().GetField("Type").GetValue(lockedReward).ToString());
                PlayerPrefs.SetInt(levelKey, 10);
                var args = new object[] { wheel, new PackRandom(packRoll), null };
                Assert.IsTrue((bool)trySpin.Invoke(null, args));
                var saved = JsonUtility.FromJson<MetaProgressState>(PlayerPrefs.GetString(SaveKey));
                Assert.AreEqual(cards.arraySize, saved.OwnedCards.Count);
                CollectionAssert.Contains(saved.ClaimedSets, setId);
                Assert.AreEqual(2, saved.TotalStars);
                Assert.AreEqual(2, saved.Spins);
                VictoryBoosters(saved);

                stateField.SetValue(null, null);
                args[1] = new PackRandom(packRoll);
                Assert.IsTrue((bool)trySpin.Invoke(null, args));
                saved = JsonUtility.FromJson<MetaProgressState>(PlayerPrefs.GetString(SaveKey));
                Assert.AreEqual(7, saved.TotalStars);
                Assert.AreEqual(1, saved.Spins);
                Assert.AreEqual(cards.arraySize, saved.OwnedCards.Count);
                VictoryBoosters(saved);
            }
            finally
            {
                stateField.SetValue(null, cached);
                if (hadSave) PlayerPrefs.SetString(SaveKey, previous);
                else PlayerPrefs.DeleteKey(SaveKey);
                if (hadLevel) PlayerPrefs.SetInt(levelKey, previousLevel);
                else PlayerPrefs.DeleteKey(levelKey);
                PlayerPrefs.Save();
            }
        }

        [Test]
        public void Collection_page_scene_uses_bound_collection_prefabs()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity", OpenSceneMode.Additive);
            try
            {
                Transform page = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    page = Find(root.transform, "ui_page_collection");
                    if (page != null) break;
                }

                Assert.IsNotNull(page);
                string[] requiredObjects =
                {
                    "ui_collection_scroll", "ui_collection_viewport", "ui_collection_content",
                    "ui_collection_detail"
                };
                foreach (string objectName in requiredObjects)
                    Assert.IsNotNull(Find(page, objectName), objectName);

                Component controller = null;
                foreach (Component component in page.GetComponents<Component>())
                {
                    if (component.GetType().Name == "CollectionPage") controller = component;
                }
                Assert.IsNotNull(controller);

                var serialized = new SerializedObject(controller);
                string[] references =
                {
                    "_catalog", "_content", "_setPrefab"
                };
                foreach (string propertyName in references)
                    Assert.IsNotNull(serialized.FindProperty(propertyName).objectReferenceValue, propertyName);

                AssertPrefabReferences("Assets/Prefabs/Meta/CollectionCardView.prefab",
                    "_art", "_lockedArt");
                AssertPrefabReferences("Assets/Prefabs/Meta/CollectionSetView.prefab",
                    "_title", "_cardsRoot", "_cardPrefab");
                AssertPrefabReferences("Assets/Prefabs/Meta/CardPackReveal.prefab",
                    "_pack", "_lid", "_input", "_prompt", "_star");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void AssertPrefabReferences(string path, params string[] fields)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.IsNotNull(prefab, path);

            Component view = null;
            foreach (Component component in prefab.GetComponents<Component>())
            {
                if (component is RectTransform) continue;
                if (component.GetType().Name == prefab.name) view = component;
            }
            Assert.IsNotNull(view, path);

            var serialized = new SerializedObject(view);
            foreach (string field in fields)
                Assert.IsNotNull(serialized.FindProperty(field).objectReferenceValue, path + " " + field);
        }

        private static Transform Find(Transform root, string objectName)
        {
            if (root.name == objectName) return root;
            foreach (Transform child in root)
            {
                Transform found = Find(child, objectName);
                if (found != null) return found;
            }
            return null;
        }

        private static void VictoryBoosters(MetaProgressState state)
        {
            foreach (BoosterType type in Enum.GetValues(typeof(BoosterType)))
                Assert.AreEqual(6, state.GetBooster(type));
        }

        [Test]
        public void Level_best_and_earned_stars_are_saved_together()
        {
            const int levelIndex = 987654;
            const string legacyKey = "Stars_987654";
            var wheel = AssetDatabase.LoadMainAssetAtPath(WheelPath);
            Assembly presentation = wheel.GetType().Assembly;
            FieldInfo stateField = presentation.GetType("MetaProgress")
                .GetField("_state", BindingFlags.NonPublic | BindingFlags.Static);
            Type levelProgress = presentation.GetType("LevelProgress");
            MethodInfo saveStars = levelProgress.GetMethod("SaveStars",
                new[] { typeof(int), typeof(int), typeof(int) });
            MethodInfo legacySaveStars = levelProgress.GetMethod("SaveStars",
                new[] { typeof(int), typeof(int) });
            MethodInfo getStars = levelProgress.GetMethod("GetStars");
            Assert.IsNotNull(saveStars);
            Assert.IsNotNull(legacySaveStars);

            object cached = stateField.GetValue(null);
            bool hadSave = PlayerPrefs.HasKey(SaveKey);
            string previous = PlayerPrefs.GetString(SaveKey);
            bool hadLegacy = PlayerPrefs.HasKey(legacyKey);
            int previousLegacy = PlayerPrefs.GetInt(legacyKey);

            try
            {
                PlayerPrefs.SetInt(legacyKey, 1);
                stateField.SetValue(null, new MetaProgressState { Spins = 0, StarBank = 9 });

                Assert.AreEqual(0, saveStars.Invoke(null, new object[] { levelIndex, 1, 10 }));
                Assert.AreEqual(2, saveStars.Invoke(null, new object[] { levelIndex, 3, 10 }));
                var saved = JsonUtility.FromJson<MetaProgressState>(PlayerPrefs.GetString(SaveKey));
                Assert.AreEqual(3, saved.LevelBests[0].Stars);
                Assert.AreEqual(2, saved.TotalStars);
                Assert.AreEqual(2, saved.PendingFly);
                Assert.AreEqual(1, saved.StarBank);
                Assert.AreEqual(1, saved.Spins);
                Assert.AreEqual(1, PlayerPrefs.GetInt("Wheel_Spins"));
                Assert.AreEqual(1, PlayerPrefs.GetInt("Wheel_StarBank"));
                Assert.AreEqual(3, PlayerPrefs.GetInt(legacyKey));

                stateField.SetValue(null, null);
                Assert.AreEqual(3, getStars.Invoke(null, new object[] { levelIndex }));
                Assert.AreEqual(0, saveStars.Invoke(null, new object[] { levelIndex, 3, 10 }));
            }
            finally
            {
                stateField.SetValue(null, cached);
                if (hadSave) PlayerPrefs.SetString(SaveKey, previous);
                else PlayerPrefs.DeleteKey(SaveKey);
                if (hadLegacy) PlayerPrefs.SetInt(legacyKey, previousLegacy);
                else PlayerPrefs.DeleteKey(legacyKey);
                PlayerPrefs.Save();
            }
        }
    }
}

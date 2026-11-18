using NUnit.Framework;

namespace Tests.EditMode
{
    public class LevelJsonTest
    {
        [Test]
        public void ToJsonAndBack_KeepsAllFields()
        {
            var level = new LevelData
            {
                width = 3, height = 2, moves = 15,
                colors = new[] { "Red", "Blue", "Green" },
                stars = new LevelStars { two = 2, three = 5 },
                cells = new[] { "#B.", "O.r" },
                overlays = new[] { "...", ".L." },
                floors = new[] { "g..", "..." },
            };
            level.goals.Add(new LevelGoalData { type = "ClearBox", count = 1 });

            LevelData copy = LevelJson.FromJson(LevelJson.ToJson(level));

            Assert.AreEqual(3, copy.width);
            Assert.AreEqual(15, copy.moves);
            CollectionAssert.AreEqual(level.colors, copy.colors);
            Assert.AreEqual(5, copy.stars.three);
            CollectionAssert.AreEqual(level.cells, copy.cells);
            CollectionAssert.AreEqual(level.overlays, copy.overlays);
            CollectionAssert.AreEqual(level.floors, copy.floors);
            Assert.AreEqual("ClearBox", copy.goals[0].type);
        }

        [Test]
        public void FromJson_EmptyText_ReturnsNull()
        {
            Assert.IsNull(LevelJson.FromJson(""));
        }
    }
}
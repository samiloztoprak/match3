using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.EditMode
{
    public class LevelGeneratorTest
    {
        private static LevelGeneratorSettings Settings(params (LevelLayer layer, char code, int count)[] pieces)
        {
            var settings = new LevelGeneratorSettings { width = 8, height = 8, colorCount = 4, mirror = true };
            foreach (var (layer, code, count) in pieces)
            {
                settings.pieces.Add(new GeneratorPiece { layer = layer, code = code.ToString(), count = count });
            }
            return settings;
        }

        [Test]
        public void Generate_MakesAValidLevel_WithTheRequestedPieces()
        {
            LevelGeneratorSettings settings = Settings(
                (LevelLayer.Cell, 'B', 6), (LevelLayer.Floor, 'g', 6), (LevelLayer.Overlay, 'L', 4));

            LevelData level = LevelGenerator.Generate(settings, 42);

            Assert.IsEmpty(LevelValidator.Validate(level));
            var counts = LevelEditing.CountAutoGoals(level);
            Assert.AreEqual(6, counts[GoalType.ClearBox]);
            Assert.AreEqual(6, counts[GoalType.ClearGrass]);
            Assert.AreEqual(4, counts[GoalType.ClearLock]);
        }

        [Test]
        public void Generate_SameSeed_GivesTheSameLevel()
        {
            LevelGeneratorSettings settings = Settings((LevelLayer.Cell, 'B', 6), (LevelLayer.Overlay, 'C', 2));

            Assert.AreEqual(
                LevelJson.ToJson(LevelGenerator.Generate(settings, 7)),
                LevelJson.ToJson(LevelGenerator.Generate(settings, 7)));
        }

        [Test]
        public void Generate_WithMirror_IsSymmetric()
        {
            LevelData level = LevelGenerator.Generate(Settings((LevelLayer.Cell, '#', 4), (LevelLayer.Cell, 'B', 6)), 3);

            for (int y = 0; y < level.height; y++)
            {
                for (int x = 0; x < level.width; x++)
                {
                    Assert.AreEqual(level.At(LevelLayer.Cell, x, y), level.At(LevelLayer.Cell, level.width - 1 - x, y),
                        $"Cell ({x},{y}) has no mirror twin");
                }
            }
        }

        [Test]
        public void MovesFor_PicksTheLimitForTheDifficulty()
        {
            var runs = new List<LevelBot.RunResult>();
            for (int moves = 5; moves <= 14; moves++) runs.Add(new LevelBot.RunResult { MovesNeeded = moves });

            Assert.AreEqual(12, LevelBot.MovesFor(runs, LevelDifficulty.Normal));   // 80% finish within 12
            Assert.AreEqual(14, LevelBot.MovesFor(runs, LevelDifficulty.Easy));     // 95% → the last run
        }
    }
}
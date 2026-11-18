using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.EditMode
{
    public class LevelBotTest
    {
        private static LevelData EasyLevel()
        {
            var level = new LevelData
            {
                width = 6, height = 6, moves = 20,
                colors = new[] { "Red", "Yellow", "Blue" },
            };
            LevelEditing.EnsureRows(level);
            LevelEditing.AddColorGoal(level, PieceColor.Red, 6);
            return level;
        }

        [Test]
        public void BoardClone_IsIndependentOfTheOriginal()
        {
            LevelData level = EasyLevel();
            LevelEditing.Paint(level, LevelElements.Get(LevelLayer.Cell, 'B'), 1, 1, out _);
            LevelEditing.Paint(level, LevelElements.Get(LevelLayer.Overlay, 'L'), 3, 3, out _);
            Board board = LevelBoardBuilder.Build(level, new SystemRandom(1));

            Board copy = board.Clone();
            copy.ClearCell(0, 0);
            copy.GetCell(1, 1).ApplyDamage(1);

            Assert.IsNotNull(board.GetCell(0, 0));
            Assert.AreEqual(3, board.GetCell(1, 1).Health, "Damage on the copy must not reach the original");
            Assert.AreNotSame(board.OverlayPieces[0], copy.OverlayPieces[0]);
        }

        [Test]
        public void Bot_FinishesAnEasyLevel()
        {
            LevelData level = EasyLevel();
            for (int seed = 0; seed < 5; seed++)
            {
                LevelBot.RunResult run = LevelBot.Play(level, seed);
                Assert.IsTrue(run.Finished && run.MovesNeeded <= level.moves, $"Seed {seed}: {run.MovesNeeded} moves");
            }
        }

        [Test]
        public void Bot_SameSeed_GivesTheSameResult()
        {
            LevelData level = EasyLevel();
            Assert.AreEqual(LevelBot.Play(level, 7).MovesNeeded, LevelBot.Play(level, 7).MovesNeeded);
        }

        [Test]
        public void Analyze_UsesPercentilesOfMovesNeeded()
        {
            var runs = new List<LevelBot.RunResult>();
            for (int moves = 5; moves <= 14; moves++)
            {
                runs.Add(new LevelBot.RunResult { MovesNeeded = moves, LastGoal = "ClearBox" });
            }

            LevelBot.Report report = LevelBot.Analyze(runs, 12);

            Assert.AreEqual(0.8f, report.WinRate, 0.001f);
            Assert.AreEqual(12, report.SuggestedMoves); 
            Assert.AreEqual(6, report.SuggestedThreeStars);
            Assert.AreEqual(3, report.SuggestedTwoStars);
            Assert.AreEqual("ClearBox", report.HardestGoal);
        }
        
        [Test]
        public void Rate_MapsWinRateToDifficulty()
        {
            Assert.AreEqual(LevelDifficulty.Easy, LevelBot.Rate(0.95f));
            Assert.AreEqual(LevelDifficulty.Normal, LevelBot.Rate(0.7f));
            Assert.AreEqual(LevelDifficulty.Hard, LevelBot.Rate(0.5f));
            Assert.AreEqual(LevelDifficulty.VeryHard, LevelBot.Rate(0.3f));
            Assert.AreEqual(LevelDifficulty.TooHard, LevelBot.Rate(0.1f));
        }
    }
}
using System.Linq;
using NUnit.Framework;
using LogicLibrary;

namespace Tests.EditMode
{
    public class LevelBoardBuilderTest
    {
        private sealed class SeqRandom : IRandom
        {
            private int _i;
            public int Next(int maxExclusive) => (_i++ * 7 + 3) % maxExclusive;
        }

        private static LevelData Level(string[] cells, string[] overlays = null, string[] floors = null, params string[] colors)
        {
            return new LevelData
            {
                width = cells[0].Length,
                height = cells.Length,
                moves = 10,
                colors = colors.Length > 0 ? colors : new[] { "Red", "Yellow", "Blue", "Green" },
                cells = cells,
                overlays = overlays,
                floors = floors,
            };
        }

        [Test]
        public void Build_PlacesCellElementsAndFillsTheRest()
        {
            LevelData level = Level(new[] { "#B.", "...", "r.O" });

            Board board = LevelBoardBuilder.Build(level, new SeqRandom());

            Assert.IsInstanceOf<HolePiece>(board.GetCell(0, 0));
            Assert.IsInstanceOf<BoxPiece>(board.GetCell(1, 0));
            Assert.IsInstanceOf<ToyPiece>(board.GetCell(2, 2));
            Assert.AreEqual(PieceColor.Red, board.GetCell(0, 2).Color);
            Assert.IsNotNull(board.GetCell(1, 1), "Cell under the box must be filled");
        }

        [Test]
        public void Build_UsesOnlyLevelColors()
        {
            LevelData level = Level(new[] { "....", "....", "....", "...." }, null, null, "Red", "Blue");

            Board board = LevelBoardBuilder.Build(level, new SeqRandom());

            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    PieceColor c = board.GetCell(x, y).Color;
                    Assert.IsTrue(c == PieceColor.Red || c == PieceColor.Blue, $"Unexpected color {c}");
                }
            }
        }

        [Test]
        public void Build_LockAndCageLockThePiecesUnderThem()
        {
            LevelData level = Level(
                new[] { "....", "....", "...." },
                new[] { "L...", ".C..", "...." });

            Board board = LevelBoardBuilder.Build(level, new SeqRandom());

            Assert.IsTrue(BoardLogic.IsPieceLocked(board.GetCell(0, 0)));
            Assert.IsTrue(BoardLogic.IsPieceLocked(board.GetCell(1, 1)));
            Assert.IsTrue(BoardLogic.IsPieceLocked(board.GetCell(2, 2)));
            Assert.IsFalse(BoardLogic.IsPieceLocked(board.GetCell(3, 0)));
            Assert.AreEqual(2, board.OverlayPieces.Count);
        }

        [Test]
        public void Build_PlacesFloors()
        {
            LevelData level = Level(new[] { "...", "..." }, null, new[] { "g..", "..." });

            Board board = LevelBoardBuilder.Build(level, new SeqRandom());

            Assert.IsInstanceOf<GrassPiece>(board.GetFloor(0, 0));
            Assert.IsNull(board.GetFloor(1, 0));
        }

        [Test]
        public void BuildGoals_ConvertsGoalData()
        {
            LevelData level = Level(new[] { "..." });
            level.goals.Add(new LevelGoalData { type = "CollectColor", color = "Red", count = 12 });
            level.goals.Add(new LevelGoalData { type = "ClearBox", count = 3 });

            var goals = LevelBoardBuilder.BuildGoals(level);

            Assert.AreEqual(2, goals.Count);
            Assert.AreEqual(GoalType.CollectColor, goals[0].Type);
            Assert.AreEqual(PieceColor.Red, goals[0].Color);
            Assert.AreEqual(3, goals[1].TargetCount);
        }

        [Test]
        public void Validator_RejectsLockOnBoxAndTooManyGoals()
        {
            LevelData level = Level(new[] { "B..", "...", "..." }, new[] { "L..", "...", "..." });
            level.goals.Add(new LevelGoalData { type = "ClearBox", count = 2 });

            var errors = LevelValidator.Validate(level);

            Assert.IsTrue(errors.Any(e => e.Contains("lock")), "Lock on a box must be rejected");
            Assert.IsTrue(errors.Any(e => e.Contains("ClearBox")), "ClearBox 2 with 1 box must be rejected");
        }

        [Test]
        public void Validator_AcceptsAValidLevel()
        {
            LevelData level = Level(new[] { "O..", ".B.", "..." }, new[] { "...", "...", "L.." }, new[] { "...", "...", "..g" });
            level.goals.Add(new LevelGoalData { type = "CollectToy", count = 1 });

            Assert.IsEmpty(LevelValidator.Validate(level));
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using LogicLibrary;

namespace Tests.EditMode
{
    public class BoosterTest
    {
        private static Board Build(params string[] rows)
        {
            Board board = new Board(rows[0].Length, rows.Length);
            for (int y = 0; y < rows.Length; y++)
            for (int x = 0; x < rows[y].Length; x++)
            {
                Piece piece;
                switch (rows[y][x])
                {
                    case 'O': piece = new ToyPiece(board.NextId()); break;
                    case 'H': piece = new HolePiece(board.NextId()); break;
                    case 'X': piece = new BoxPiece(board.NextId(), PieceColor.Empty); break;
                    case 'R': piece = new ColoredPiece(board.NextId(), PieceColor.Red); break;
                    case 'G': piece = new ColoredPiece(board.NextId(), PieceColor.Green); break;
                    case 'B': piece = new ColoredPiece(board.NextId(), PieceColor.Blue); break;
                    default:  piece = new ColoredPiece(board.NextId(), PieceColor.Yellow); break;
                }
                board.SetCell(x, y, piece);
            }
            return board;
        }

        private static bool WasCleared(List<TurnStepData> steps, int id) =>
            steps.OfType<ClearPieceData>().Any(c => c.Hits.Any(h => h.PieceId == id));

        [Test]
        public void Hammer_ClearsSingleCell_WithoutMatch()
        {
            Board board = Build(
                "RGB",
                "GBR",
                "BRG");
            Piece target = board.GetCell(1, 1);

            var result = TurnResolver.Resolve(board, new BoosterCommand(BoosterType.Hammer, 1, 1));

            Assert.IsTrue(WasCleared(result, target.Id));
            Assert.IsTrue(result.OfType<BoosterActivationData>().Any(b => b.Type == BoosterType.Hammer && b.X == 1 && b.Y == 1));
            Assert.IsNotNull(board.GetCell(1, 1), "The hammered cell must be refilled");
            Assert.AreEqual(1, result.OfType<GoalProgressData>().Sum(g => g.ClearedColors.Count(c => c == PieceColor.Blue)));
        }

        [Test]
        public void Hammer_DamagesBox()
        {
            Board board = Build(
                "RGB",
                "GXR",
                "BRG");
            Piece box = board.GetCell(1, 1);

            TurnResolver.Resolve(board, new BoosterCommand(BoosterType.Hammer, 1, 1));

            Assert.AreSame(box, board.GetCell(1, 1));
            Assert.AreEqual(2, box.Health);
        }

        [Test]
        public void Hammer_OnCage_HitsCageNotPieceInside()
        {
            Board board = Build(
                "RGBY",
                "GBRG",
                "BRGB");
            Piece inside = board.GetCell(1, 1);
            var cage = new CagePiece(board.NextId(), 1, 1);
            board.AddOverlayPiece(cage);
            foreach (var (cx, cy) in cage.CoveredCells)
            {
                BoardLogic.AddFlag(board.GetCell(cx, cy), PieceFlags.Locked);
            }

            var result = TurnResolver.Resolve(board, new BoosterCommand(BoosterType.Hammer, 1, 1));

            Assert.IsFalse(WasCleared(result, inside.Id));
            Assert.AreEqual(2, cage.Health, "Cage starts at 3 health");
        }

        [Test]
        public void RowBooster_ClearsWholeRow_AndChainsSpecials()
        {
            Board board = Build(
                "RGBYR",
                "GBRGB",
                "BRGBY");
            var rocket = new RocketPiece(board.NextId(), RocketDirection.Vertical);
            board.SetCell(4, 1, rocket);
            var rowIds = Enumerable.Range(0, 4).Select(x => board.GetCell(x, 1).Id).ToList();
            int topOfRocketColumn = board.GetCell(4, 0).Id;

            var result = TurnResolver.Resolve(board, new BoosterCommand(BoosterType.Row, 1, 1));

            foreach (int id in rowIds) Assert.IsTrue(WasCleared(result, id));
            Assert.IsTrue(result.OfType<SpecialActivationData>().Any(s => s.PieceId == rocket.Id), "Rocket in the row must chain");
            Assert.IsTrue(WasCleared(result, topOfRocketColumn), "Chained vertical rocket clears its column");
        }

        [Test]
        public void ColumnBooster_DoesNotDestroyToy_AndToyFallsToExit()
        {
            Board board = Build(
                "ROB",
                "GBR",
                "BRG");
            Piece toy = board.GetCell(1, 0);

            var result = TurnResolver.Resolve(board, new BoosterCommand(BoosterType.Column, 1, 2));

            Assert.IsFalse(WasCleared(result, toy.Id));
            Assert.IsTrue(result.OfType<CollectStepData>().Any(c => c.Hits.Any(h => h.PieceId == toy.Id)),
                "Clearing the column under the toy drops it to the exit");
        }

        [Test]
        public void Booster_OnHoleOrOutside_IsRejected()
        {
            Board board = Build(
                "RGB",
                "GHR",
                "BRG");

            Assert.AreEqual(0, TurnResolver.Resolve(board, new BoosterCommand(BoosterType.Hammer, 1, 1)).Count);
            Assert.AreEqual(0, TurnResolver.Resolve(board, new BoosterCommand(BoosterType.Row, 5, 0)).Count);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using LogicLibrary;

namespace Tests.EditMode
{
    public class ToyPieceTest
    {
        private static bool WasCollected(List<TurnStepData> steps, int id) =>
            steps.OfType<CollectStepData>().Any(c => c.Hits.Any(h => h.PieceId == id));

        private static bool WasCleared(List<TurnStepData> steps, int id) =>
            steps.OfType<ClearPieceData>().Any(c => c.Hits.Any(h => h.PieceId == id));

        private static int ToyGoal(List<TurnStepData> steps) =>
            steps.OfType<GoalProgressData>().Sum(g => g.GetCount(GoalType.CollectToy));

        // R G B Y = colored, O = Toy, H = hole. rows[0] is the top row.
        private static Board Build(string[] rows, out ToyPiece firstToy)
        {
            Board board = new Board(rows[0].Length, rows.Length);
            firstToy = null;
            for (int y = 0; y < rows.Length; y++)
            for (int x = 0; x < rows[y].Length; x++)
            {
                Piece piece;
                switch (rows[y][x])
                {
                    case 'O': piece = new ToyPiece(board.NextId()); firstToy ??= (ToyPiece)piece; break;
                    case 'H': piece = new HolePiece(board.NextId()); break;
                    case 'R': piece = new ColoredPiece(board.NextId(), PieceColor.Red); break;
                    case 'G': piece = new ColoredPiece(board.NextId(), PieceColor.Green); break;
                    case 'B': piece = new ColoredPiece(board.NextId(), PieceColor.Blue); break;
                    default:  piece = new ColoredPiece(board.NextId(), PieceColor.Yellow); break;
                }
                board.SetCell(x, y, piece);
            }
            return board;
        }

        [Test]
        public void Toy_ReachesBottomRow_IsCollected()
        {
            Board board = Build(new[]
            {
                "BGY",
                "OYR",
                "RRB",
            }, out ToyPiece Toy);

            var result = TurnResolver.Resolve(board, new SwapCommand(board, 2, 1, 2, 2));

            Assert.IsTrue(WasCollected(result, Toy.Id));
            Assert.AreEqual(1, ToyGoal(result));
            Assert.AreNotSame(Toy, board.GetCell(0, 2), "Collected Toy must leave the board");
            Assert.IsNotNull(board.GetCell(0, 2), "The emptied exit cell must be refilled");
        }

        [Test]
        public void Toy_HitByRocket_IsNotDestroyed()
        {
            Board board = Build(new[]
            {
                "BGYBG",
                "GYYRO",
                "GYBGY",
            }, out ToyPiece Toy);
            board.SetCell(2, 1, new RocketPiece(board.NextId(), RocketDirection.Horizontal));

            var result = TurnResolver.Resolve(board, new SwapCommand(board, 1, 1, 2, 1));

            Assert.IsFalse(WasCleared(result, Toy.Id));
            Assert.IsFalse(WasCollected(result, Toy.Id));
            Assert.AreSame(Toy, board.GetCell(4, 1));
        }

        [Test]
        public void Toy_InsideCageOnBottomRow_IsNotCollected()
        {
            Board board = Build(new[]
            {
                "RRGY",
                "GBRB",
                "OYBG",
            }, out ToyPiece Toy);
            var cage = new CagePiece(board.NextId(), 0, 1);
            board.AddOverlayPiece(cage);
            foreach (var (cx, cy) in cage.CoveredCells)
            {
                BoardLogic.AddFlag(board.GetCell(cx, cy), PieceFlags.Locked);
            }

            var result = TurnResolver.Resolve(board, new SwapCommand(board, 2, 0, 2, 1));

            Assert.IsTrue(result.OfType<ClearPieceData>().Any(), "Swap should produce a match");
            Assert.IsFalse(WasCollected(result, Toy.Id));
            Assert.AreSame(Toy, board.GetCell(0, 2));
        }

        [Test]
        public void Toy_AboveHoleAtColumnBottom_IsCollected()
        {
            Board board = Build(new[]
            {
                "RRG",
                "BOR",
                "GHB",
            }, out ToyPiece Toy);

            Assert.AreEqual(1, BoardLogic.GetExitY(board, 1), "A hole at the bottom moves the exit up");

            var result = TurnResolver.Resolve(board, new SwapCommand(board, 2, 0, 2, 1));

            Assert.IsTrue(WasCollected(result, Toy.Id));
            Assert.AreEqual(1, ToyGoal(result));
        }
    }
}
using NUnit.Framework;
using LogicLibrary;

namespace Tests.EditMode
{
    public class BoardColorsTest
    {
        private sealed class SeqRandom : IRandom
        {
            private int _i;
            public int Next(int maxExclusive) => (_i++ * 7 + 3) % maxExclusive;
        }

        [Test]
        public void FillAllEmptyCells_UsesOnlyBoardColors()
        {
            Board board = new Board(6, 6);
            board.SetColors(new[] { PieceColor.Red, PieceColor.Blue, PieceColor.Green });

            OrganizationLogic.FillAllEmptyCells(board, new SeqRandom());

            for (int x = 0; x < 6; x++)
            for (int y = 0; y < 6; y++)
            {
                Piece piece = board.GetCell(x, y);
                Assert.IsNotNull(piece);
                Assert.AreNotEqual(PieceColor.Yellow, piece.Color);
            }
        }

        [Test]
        public void FillAllEmptyCells_FillsBelowBlockers()
        {
            Board board = new Board(3, 3);
            board.SetCell(1, 0, new BoxPiece(board.NextId(), PieceColor.Empty));

            OrganizationLogic.FillAllEmptyCells(board, new SeqRandom());

            Assert.IsNotNull(board.GetCell(1, 1), "Cell under a box must be filled at level start");
            Assert.IsNotNull(board.GetCell(1, 2));
        }
    }
}
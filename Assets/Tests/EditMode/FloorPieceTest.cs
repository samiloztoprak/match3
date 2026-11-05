using NUnit.Framework;

namespace Tests.EditMode
{
    public class FloorPieceTest
    {
        [Test]
        public void IsCellSwapBlocked_GrassFloor_NoLongerBlocksSwap()
        {
            Board board = new Board(2, 1);
            board.SetCell(0, 0, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetFloor(0, 0, new GrassPiece(board.NextId()));

            Assert.IsFalse(LogicLibrary.BoardLogic.IsCellSwapBlocked(board, 0, 0));
        }

        [Test]
        public void Grass_BreaksWhenCoveredPieceClearsViaNormalMatch_AndIncrementsGoalCount()
        {
            Board board = new Board(3, 3);
            board.SetCell(0, 0, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(2, 0, new ColoredPiece(board.NextId(), PieceColor.Yellow));

            board.SetCell(0, 1, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(1, 1, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(2, 1, new ColoredPiece(board.NextId(), PieceColor.Red));

            board.SetCell(0, 2, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(1, 2, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(2, 2, new ColoredPiece(board.NextId(), PieceColor.Yellow));

            board.SetFloor(1, 1, new GrassPiece(board.NextId()));

            var result = TurnResolver.Resolve(board, null);

            Assert.IsNull(board.GetFloor(1, 1), "grass should break once the piece covering it clears");

            GoalProgressData goalProgress = null;
            foreach (var step in result)
            {
                if (step is GoalProgressData data)
                {
                    goalProgress = data;
                }
            }

            Assert.IsNotNull(goalProgress);
            Assert.AreEqual(1, goalProgress.GetCount(GoalType.ClearGrass));
        }

        [Test]
        public void Grass_DoesNotBreakFromAdjacentMatchAlone()
        {
            Board board = new Board(3, 3);
            board.SetCell(0, 0, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(2, 0, new ColoredPiece(board.NextId(), PieceColor.Yellow));

            board.SetCell(0, 1, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(1, 1, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(2, 1, new ColoredPiece(board.NextId(), PieceColor.Red));

            board.SetCell(0, 2, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(1, 2, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(2, 2, new ColoredPiece(board.NextId(), PieceColor.Yellow));

            GrassPiece grass = new GrassPiece(board.NextId());
            board.SetFloor(0, 0, grass);

            var result = TurnResolver.Resolve(board, null);

            bool grassBrokeThisTurn = false;
            foreach (var step in result)
            {
                if (step is FallStepData)
                {
                    break;
                }
                if (step is ClearOverlayData clearOverlay)
                {
                    foreach (var hit in clearOverlay.Hits)
                    {
                        if (hit.PieceId == grass.Id)
                        {
                            grassBrokeThisTurn = true;
                        }
                    }
                }
            }

            Assert.IsFalse(grassBrokeThisTurn, "grass adjacent to a match, but not itself covered by a cleared cell, should not break");
        }

        [Test]
        public void Floor_ActivationBlastThroughAlreadyEmptyCell_StillBreaksGrass()
        {
            Board board = new Board(5, 1);
            board.SetCell(0, 0, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 0, new RocketPiece(board.NextId(), RocketDirection.Horizontal));
            board.SetCell(3, 0, new ColoredPiece(board.NextId(), PieceColor.Green));

            board.SetFloor(4, 0, new GrassPiece(board.NextId()));

            TurnResolver.Resolve(board, new TapCommand(2, 0));

            Assert.IsNull(board.GetFloor(4, 0),
                "an activation blast passing through an already-empty cell should still break the floor piece there");
        }
    }
}
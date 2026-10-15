using NUnit.Framework;
using System.Collections.Generic;


public class NewTestScript
{
    [Test]
    public void TestSwappOnBoad()
    {
        Board board = new Board(3,3);
        board.SetCell(0, 0, new ColoredPiece(board.NextId(), PieceType.Blue));
        board.SetCell(0, 1, new ColoredPiece(board.NextId(), PieceType.Red));
        board.SetCell(0, 2, new ColoredPiece(board.NextId(), PieceType.Yellow));
        board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceType.Green));
        board.SetCell(1, 1, new ColoredPiece(board.NextId(), PieceType.Yellow));
        board.SetCell(1, 2, new ColoredPiece(board.NextId(), PieceType.Blue));
        board.SetCell(2, 0, new ColoredPiece(board.NextId(), PieceType.Red));
        board.SetCell(2, 1, new ColoredPiece(board.NextId(), PieceType.Blue));
        board.SetCell(2, 2, new ColoredPiece(board.NextId(), PieceType.Green));

        SwapCommand swapCommand = new SwapCommand(board, 0, 0, 1, 0);
        swapCommand.Execute();
        Assert.AreEqual(PieceType.Green, board.GetCell(0,0).Color);
    }

    [Test]
    public void TestFindMatchOnBoard()
    {
        Board board = new Board(3,4);
        board.SetCell(0, 0, new ColoredPiece(board.NextId(), PieceType.Blue));
        board.SetCell(0, 1, new ColoredPiece(board.NextId(), PieceType.Blue));
        board.SetCell(0, 2, new ColoredPiece(board.NextId(), PieceType.Blue));
        board.SetCell(0, 3, new ColoredPiece(board.NextId(), PieceType.Green));

        board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceType.Green));
        board.SetCell(1, 1, new ColoredPiece(board.NextId(), PieceType.Yellow));
        board.SetCell(1, 2, new ColoredPiece(board.NextId(), PieceType.Yellow));
        board.SetCell(1, 3, new ColoredPiece(board.NextId(), PieceType.Blue));

        board.SetCell(2, 0, new ColoredPiece(board.NextId(), PieceType.Red));
        board.SetCell(2, 1, new ColoredPiece(board.NextId(), PieceType.Green));
        board.SetCell(2, 2, new ColoredPiece(board.NextId(), PieceType.Green));
        board.SetCell(2, 3, new ColoredPiece(board.NextId(), PieceType.Green));

        HashSet<(int, int)> matchSet = new HashSet<(int, int)>();
        matchSet = LogicLibrary.MatchLogic.FindMatches(board);
        
        Assert.AreEqual(matchSet.Contains((0,0)), true);
        Assert.AreEqual(matchSet.Contains((0,1)), true);
        Assert.AreEqual(matchSet.Contains((0,2)), true);
        Assert.AreEqual(matchSet.Contains((0,3)), false);
        Assert.AreEqual(matchSet.Contains((1,0)), false);
        Assert.AreEqual(matchSet.Contains((1,1)), false);
        Assert.AreEqual(matchSet.Contains((1,2)), false);
        Assert.AreEqual(matchSet.Contains((1,3)), false);
        Assert.AreEqual(matchSet.Contains((2,0)), false);
        Assert.AreEqual(matchSet.Contains((2,1)), true);
        Assert.AreEqual(matchSet.Contains((2,2)), true);
        Assert.AreEqual(matchSet.Contains((2,3)), true);
    }

    [Test]
    public void TestResolve()
    {
        Board board = new Board(3,3);
        board.SetCell(0, 0, new ColoredPiece(board.NextId(), PieceType.Blue));
        board.SetCell(0, 1, new ColoredPiece(board.NextId(), PieceType.Green));
        board.SetCell(0, 2, new ColoredPiece(board.NextId(), PieceType.Green));
        
        board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceType.Green));
        board.SetCell(1, 1, new ColoredPiece(board.NextId(), PieceType.Yellow));
        board.SetCell(1, 2, new ColoredPiece(board.NextId(), PieceType.Blue));
        
        board.SetCell(2, 0, new ColoredPiece(board.NextId(), PieceType.Red));
        board.SetCell(2, 1, new ColoredPiece(board.NextId(), PieceType.Blue));
        board.SetCell(2, 2, new ColoredPiece(board.NextId(), PieceType.Green));
        
        var command = new SwapCommand(board, 0, 0, 1, 0);
        var result = TurnResolver.Resolve(board, command);

        bool bHasTurnStepData = false;
        bool bHasRefilStepData = false;
        foreach (var turnStepData in result)
        {
            if (turnStepData is ClearStepData)
            {
                bHasTurnStepData = true;
            }
            if (turnStepData is RefillStepData)
            {
                bHasRefilStepData = true;
            }
        }
        Assert.AreEqual(bHasTurnStepData, true);
        Assert.AreEqual(bHasRefilStepData, true);
    }
}

using NUnit.Framework;
using System.Collections.Generic;


public class CoreCommandTests
{
    [Test]
    public void TestSwappOnBoad()
    {
        Board board = new Board(3,3);
        board.SetCell(0, 0, new ColoredPiece(board.NextId(), PieceColor.Blue));
        board.SetCell(0, 1, new ColoredPiece(board.NextId(), PieceColor.Red));
        board.SetCell(0, 2, new ColoredPiece(board.NextId(), PieceColor.Yellow));
        board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceColor.Green));
        board.SetCell(1, 1, new ColoredPiece(board.NextId(), PieceColor.Yellow));
        board.SetCell(1, 2, new ColoredPiece(board.NextId(), PieceColor.Blue));
        board.SetCell(2, 0, new ColoredPiece(board.NextId(), PieceColor.Red));
        board.SetCell(2, 1, new ColoredPiece(board.NextId(), PieceColor.Blue));
        board.SetCell(2, 2, new ColoredPiece(board.NextId(), PieceColor.Green));

        SwapCommand swapCommand = new SwapCommand(board, 0, 0, 1, 0);
        swapCommand.Execute();
        Assert.AreEqual(PieceColor.Green, board.GetCell(0,0).Color);
    }

    [Test]
    public void TestFindMatchOnBoard()
    {
        Board board = new Board(3,4);
        board.SetCell(0, 0, new ColoredPiece(board.NextId(), PieceColor.Blue));
        board.SetCell(0, 1, new ColoredPiece(board.NextId(), PieceColor.Blue));
        board.SetCell(0, 2, new ColoredPiece(board.NextId(), PieceColor.Blue));
        board.SetCell(0, 3, new ColoredPiece(board.NextId(), PieceColor.Green));

        board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceColor.Green));
        board.SetCell(1, 1, new ColoredPiece(board.NextId(), PieceColor.Yellow));
        board.SetCell(1, 2, new ColoredPiece(board.NextId(), PieceColor.Yellow));
        board.SetCell(1, 3, new ColoredPiece(board.NextId(), PieceColor.Green));

        board.SetCell(2, 0, new ColoredPiece(board.NextId(), PieceColor.Red));
        board.SetCell(2, 1, new ColoredPiece(board.NextId(), PieceColor.Green));
        board.SetCell(2, 2, new ColoredPiece(board.NextId(), PieceColor.Green));
        board.SetCell(2, 3, new ColoredPiece(board.NextId(), PieceColor.Green));

        List<(ShapeTemplate template, HashSet<(int, int)> cells)> matches = LogicLibrary.MatchLogic.FindMatches(board);
        HashSet<(int, int)> matchSet = new HashSet<(int, int)>();
        foreach (var (template, cells) in matches)
        {
            foreach (var cell in cells)
            {
                matchSet.Add(cell);
            }
        }
        
        Assert.AreEqual(matchSet.Contains((0,0)), true);
        Assert.AreEqual(matchSet.Contains((0,1)), true);
        Assert.AreEqual(matchSet.Contains((0,2)), true);
        Assert.AreEqual(matchSet.Contains((0,3)), true);
        Assert.AreEqual(matchSet.Contains((1,0)), false);
        Assert.AreEqual(matchSet.Contains((1,1)), false);
        Assert.AreEqual(matchSet.Contains((1,2)), false);
        Assert.AreEqual(matchSet.Contains((1,3)), true);
        Assert.AreEqual(matchSet.Contains((2,0)), false);
        Assert.AreEqual(matchSet.Contains((2,1)), true);
        Assert.AreEqual(matchSet.Contains((2,2)), true);
        Assert.AreEqual(matchSet.Contains((2,3)), true);
    }

    [Test]
    public void TestResolve()
    {
        Board board = new Board(3,3);
        board.SetCell(0, 0, new ColoredPiece(board.NextId(), PieceColor.Blue));
        board.SetCell(0, 1, new ColoredPiece(board.NextId(), PieceColor.Green));
        board.SetCell(0, 2, new ColoredPiece(board.NextId(), PieceColor.Green));
        
        board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceColor.Green));
        board.SetCell(1, 1, new ColoredPiece(board.NextId(), PieceColor.Yellow));
        board.SetCell(1, 2, new ColoredPiece(board.NextId(), PieceColor.Blue));
        
        board.SetCell(2, 0, new ColoredPiece(board.NextId(), PieceColor.Red));
        board.SetCell(2, 1, new ColoredPiece(board.NextId(), PieceColor.Blue));
        board.SetCell(2, 2, new ColoredPiece(board.NextId(), PieceColor.Green));
        
        var command = new SwapCommand(board, 0, 0, 1, 0);
        var result = TurnResolver.Resolve(board, command);

        bool bHasTurnStepData = false;
        bool bHasRefilStepData = false;
        foreach (var turnStepData in result)
        {
            if (turnStepData is ClearPieceData)
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

    [Test]
    public void TestFindClustersConnectedCells()
    {
        Board board = new Board(3,3);
        board.SetCell(0, 0, new ColoredPiece(board.NextId(), PieceColor.Green));
        board.SetCell(0, 1, new ColoredPiece(board.NextId(), PieceColor.Green));
        board.SetCell(0, 2, new ColoredPiece(board.NextId(), PieceColor.Green));
        
        board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceColor.Blue));
        board.SetCell(1, 1, new ColoredPiece(board.NextId(), PieceColor.Green));
        board.SetCell(1, 2, new ColoredPiece(board.NextId(), PieceColor.Blue));
        
        board.SetCell(2, 0, new ColoredPiece(board.NextId(), PieceColor.Blue));
        board.SetCell(2, 1, new ColoredPiece(board.NextId(), PieceColor.Green));
        board.SetCell(2, 2, new ColoredPiece(board.NextId(), PieceColor.Blue));
        
        HashSet<(int,int)> matchSet = new HashSet<(int,int)>() {(0,0),(1,0),(2,0),(1,1),(1,2)};
        List<HashSet<(int, int)>> result = LogicLibrary.MatchLogic.FindClusters(board, matchSet);
        
        Assert.AreEqual(1, result.Count);
        if (result.Count == 1)
        {
            Assert.AreEqual(5, result[0].Count);
        }
    }
    
    [Test]
    public void TestFindClustersDisconnectedGroups()
    {
        Board board = new Board(3,3);
        board.SetCell(0, 0, new ColoredPiece(board.NextId(), PieceColor.Green));
        board.SetCell(0, 1, new ColoredPiece(board.NextId(), PieceColor.Green));
        board.SetCell(0, 2, new ColoredPiece(board.NextId(), PieceColor.Green));
        
        board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceColor.Blue));
        board.SetCell(1, 1, new ColoredPiece(board.NextId(), PieceColor.Green));
        board.SetCell(1, 2, new ColoredPiece(board.NextId(), PieceColor.Blue));
        
        board.SetCell(2, 0, new ColoredPiece(board.NextId(), PieceColor.Blue));
        board.SetCell(2, 1, new ColoredPiece(board.NextId(), PieceColor.Blue));
        board.SetCell(2, 2, new ColoredPiece(board.NextId(), PieceColor.Blue));
        
        List<(ShapeTemplate template, HashSet<(int, int)> cells)> matches = LogicLibrary.MatchLogic.FindMatches(board);
        HashSet<(int, int)> matchSet = new HashSet<(int, int)>();
        
        foreach (var (template, cells) in matches)
        {
            foreach (var cell in cells)
            {
                matchSet.Add(cell);
            }
        }
        
        List<HashSet<(int, int)>> result = LogicLibrary.MatchLogic.FindClusters(board, matchSet);
        
        Assert.AreEqual(2, result.Count);
        if (result.Count == 2)
        {
            Assert.AreEqual(3, result[0].Count);
            Assert.AreEqual(3, result[1].Count);
        }
    }
    
    [Test]
    [Timeout(2000)]
    public void FindBestShapeMatchWithPosition_EmptyCells_ReturnsNullImmediately()
    {
        var result = LogicLibrary.MatchLogic.FindBestShapeMatchWithPosition(new HashSet<(int, int)>());
        Assert.IsNull(result.template);
    }

    [Test]
    public void TestFindMatches_TShape_MatchesExactly()
    {
        Board board = new Board(3,3);
        
        board.SetCell(0, 0, new ColoredPiece(board.NextId(), PieceColor.Green));
        board.SetCell(0, 1, new ColoredPiece(board.NextId(), PieceColor.Red));
        board.SetCell(0, 2, new ColoredPiece(board.NextId(), PieceColor.Blue));
        
        board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceColor.Green));
        board.SetCell(1, 1, new ColoredPiece(board.NextId(), PieceColor.Green));
        board.SetCell(1, 2, new ColoredPiece(board.NextId(), PieceColor.Green));

        board.SetCell(2, 0, new ColoredPiece(board.NextId(), PieceColor.Green));
        board.SetCell(2, 1, new ColoredPiece(board.NextId(), PieceColor.Yellow));
        board.SetCell(2, 2, new ColoredPiece(board.NextId(), PieceColor.Blue));
        
        List<(ShapeTemplate template, HashSet<(int, int)> cells)> matches = LogicLibrary.MatchLogic.FindMatches(board);

        Assert.AreEqual(1, matches.Count);
        Assert.AreEqual("t-shape", matches[0].template.Name);
        Assert.AreEqual(5, matches[0].cells.Count);
        Assert.IsTrue(matches[0].cells.Contains((0, 0)));
        Assert.IsTrue(matches[0].cells.Contains((1, 0)));
        Assert.IsTrue(matches[0].cells.Contains((1, 1)));
        Assert.IsTrue(matches[0].cells.Contains((1, 2)));
        Assert.IsTrue(matches[0].cells.Contains((2, 0)));
    }

    [Test]
    public void TestFindMatches_Line5_MatchesAsOneShape()
    {
        Board board = new Board(5,1);
        for (int x = 0; x < 5; x++)
        {
            board.SetCell(x, 0, new ColoredPiece(board.NextId(), PieceColor.Green));
        }
        
        List<(ShapeTemplate template, HashSet<(int, int)> cells)> matches = LogicLibrary.MatchLogic.FindMatches(board);

        Assert.AreEqual(1, matches.Count);
        Assert.AreEqual("line5-shape", matches[0].template.Name);
        Assert.AreEqual(5, matches[0].cells.Count);
    }
    
    [Test]
    public void TestFindMatches_UnregisteredStaircase_DoesNotMatch()
    {
        Board board = new Board(3,3);
        
        board.SetCell(0, 0, new ColoredPiece(board.NextId(), PieceColor.Green));
        board.SetCell(0, 1, new ColoredPiece(board.NextId(), PieceColor.Red));
        board.SetCell(0, 2, new ColoredPiece(board.NextId(), PieceColor.Blue));
        
        board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceColor.Green));
        board.SetCell(1, 1, new ColoredPiece(board.NextId(), PieceColor.Green));
        board.SetCell(1, 2, new ColoredPiece(board.NextId(), PieceColor.Yellow));

        board.SetCell(2, 0, new ColoredPiece(board.NextId(), PieceColor.Red));
        board.SetCell(2, 1, new ColoredPiece(board.NextId(), PieceColor.Green));
        board.SetCell(2, 2, new ColoredPiece(board.NextId(), PieceColor.Green));

        List<(ShapeTemplate template, HashSet<(int, int)> cells)> matches = LogicLibrary.MatchLogic.FindMatches(board);
        Assert.AreEqual(0, matches.Count);
    }
}

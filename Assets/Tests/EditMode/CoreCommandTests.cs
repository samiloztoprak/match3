using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class NewTestScript
{
    [Test]
    public void TestSwappOnBoad()
    {
        Board board = new Board(3,3);
        board.SetCell(0, 0, new Piece(PieceType.Blue));
        board.SetCell(0, 1, new Piece(PieceType.Red));
        board.SetCell(0, 2, new Piece(PieceType.Yellow));
        board.SetCell(1, 0, new Piece(PieceType.Green));
        board.SetCell(1, 1, new Piece(PieceType.Yellow));
        board.SetCell(1, 2, new Piece(PieceType.Blue));
        board.SetCell(2, 0, new Piece(PieceType.Red));
        board.SetCell(2, 1, new Piece(PieceType.Blue));
        board.SetCell(2, 2, new Piece(PieceType.Green));

        SwapCommand swapCommand = new SwapCommand(board, 0, 0, 1, 0);
        swapCommand.Execute();
        Assert.AreEqual(PieceType.Green, board.GetCell(0,0).Type);
    }

    [Test]
    public void TestFindMatchOnBoard()
    {
        Board board = new Board(3,4);
        board.SetCell(0, 0, new Piece(PieceType.Blue));
        board.SetCell(0, 1, new Piece(PieceType.Blue));
        board.SetCell(0, 2, new Piece(PieceType.Blue));
        board.SetCell(0, 3, new Piece(PieceType.Green));

        board.SetCell(1, 0, new Piece(PieceType.Green));
        board.SetCell(1, 1, new Piece(PieceType.Yellow));
        board.SetCell(1, 2, new Piece(PieceType.Yellow));
        board.SetCell(1, 3, new Piece(PieceType.Blue));

        board.SetCell(2, 0, new Piece(PieceType.Red));
        board.SetCell(2, 1, new Piece(PieceType.Green));
        board.SetCell(2, 2, new Piece(PieceType.Green));
        board.SetCell(2, 3, new Piece(PieceType.Green));

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
}

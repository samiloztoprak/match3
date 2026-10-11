using NUnit.Framework;
using System.Collections;
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
}

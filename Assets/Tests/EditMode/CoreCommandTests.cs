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

    [Test]
    public void TestStateMachine()
    {
        GameLoopEnvironment _gameLoopEnvironment = new GameLoopEnvironment();
        _gameLoopEnvironment.board = new Board(4,4);
        _gameLoopEnvironment.board.SetCell(0, 0, new Piece(PieceType.Blue));
        _gameLoopEnvironment.board.SetCell(0, 1, new Piece(PieceType.Blue));
        _gameLoopEnvironment.board.SetCell(0, 2, new Piece(PieceType.Green));
        _gameLoopEnvironment.board.SetCell(0, 3, new Piece(PieceType.Green));

        _gameLoopEnvironment.board.SetCell(1, 0, new Piece(PieceType.Blue));
        _gameLoopEnvironment.board.SetCell(1, 1, new Piece(PieceType.Green));
        _gameLoopEnvironment.board.SetCell(1, 2, new Piece(PieceType.Yellow));
        _gameLoopEnvironment.board.SetCell(1, 3, new Piece(PieceType.Blue));

        _gameLoopEnvironment.board.SetCell(2, 0, new Piece(PieceType.Red));
        _gameLoopEnvironment.board.SetCell(2, 1, new Piece(PieceType.Green));
        _gameLoopEnvironment.board.SetCell(2, 2, new Piece(PieceType.Red));
        _gameLoopEnvironment.board.SetCell(2, 3, new Piece(PieceType.Red));

        _gameLoopEnvironment.board.SetCell(3, 0, new Piece(PieceType.Blue));
        _gameLoopEnvironment.board.SetCell(3, 1, new Piece(PieceType.Red));
        _gameLoopEnvironment.board.SetCell(3, 2, new Piece(PieceType.Yellow));
        _gameLoopEnvironment.board.SetCell(3, 3, new Piece(PieceType.Green));

        _gameLoopEnvironment.pendingCommandSlot = new PendingCommandSlot();
        _gameLoopEnvironment.pendingPresentationSlot = new PendingPresentationSlot();
        IGameState _currentState = new IdleState(_gameLoopEnvironment);
        _currentState.Enter();
        _gameLoopEnvironment.pendingCommandSlot.Submit(new SwapCommand(_gameLoopEnvironment.board, 3, 1, 2, 1));
        int i = 0;
        int s = 0;
        while(i < 999)
        {
            if(_currentState != null)
            {
                IGameState _nextState = _currentState.Execute();
                if(_nextState != null)
                {
                    _currentState.Exit();
                    _currentState = _nextState;
                    _currentState.Enter();
                }
                if (_currentState is WaitingForPresentationState)
                {
                    s++;
                    if(s > 10)
                    {
                        s = 0;
                        _gameLoopEnvironment.pendingPresentationSlot.MarkComplete();
                    }
                }
            }
            i++;
        }
        for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                Assert.AreNotEqual(PieceType.Empty, _gameLoopEnvironment.board.GetCell(x, y).Type);
    }
}

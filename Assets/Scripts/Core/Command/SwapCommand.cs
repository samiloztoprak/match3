public class SwapCommand : ICommand, IUndoableCommand
{
    private readonly Board _board;
    private readonly int _x1, _y1, _x2, _y2;
    public (int x, int y) SourcePosition => (_x1, _y1);
    public (int x, int y) TargetPosition => (_x2, _y2);
    
    public SwapCommand(Board board, int x1, int y1, int x2, int y2)
    {
        _board = board;
        _x1 = x1;
        _y1 = y1;
        _x2 = x2;
        _y2 = y2;
    }

    public bool Validate(Board board)
    {
        if (!LogicLibrary.BoardLogic.IsValidIndex(_x1, _y1, board))
        {
            return false;
        }
        if (!LogicLibrary.BoardLogic.IsValidIndex(_x2, _y2, board))
        {
            return false;
        }
        if (!LogicLibrary.BoardLogic.IsSwappableIndexes(_x1, _y1, _x2, _y2))
        {
            return false;
        }

        Piece piece1 = board.GetCell(_x1, _y1);
        if (piece1 == null)
        {
            return false;
        }
        Piece piece2 = board.GetCell(_x2, _y2);
        if (piece2 == null)
        {
            return false;
        }

        if (LogicLibrary.BoardLogic.IsCellSwapBlocked(board, _x1, _y1))
        {
            return false;
        }
        if (LogicLibrary.BoardLogic.IsCellSwapBlocked(board, _x2, _y2))
        {
            return false;
        }
        if (!LogicLibrary.BoardLogic.IsPieceMoveable(piece1))
        {
            return false;
        }
        if (!LogicLibrary.BoardLogic.IsPieceMoveable(piece2))
        {
            return false;
        }

        return true;
    }
    
    public void Execute()
    {        
        Piece piece1 = _board.GetCell(_x1, _y1);
        Piece piece2 = _board.GetCell(_x2, _y2);
        _board.SetCell(_x1, _y1, piece2);
        _board.SetCell(_x2, _y2, piece1);
    }

    public void Undo()
    {
        Execute();
    }
}

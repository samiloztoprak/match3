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
        if (!LogicLibrary.BoardLogic.IsValidIndex(_x1, _y1, board)
            || !LogicLibrary.BoardLogic.IsValidIndex(_x2, _y2, board)
            || !LogicLibrary.BoardLogic.IsSwappableIndexes(_x1, _y1, _x2, _y2))
        {
            return false;
        }

        Piece first = board.GetCell(_x1, _y1);
        Piece second = board.GetCell(_x2, _y2);

        if (first == null && second == null)
        {
            return false;
        }

        return CanUseCell(board, _x1, _y1, first)
               && CanUseCell(board, _x2, _y2, second);
    }
    
    private static bool CanUseCell(Board board, int x, int y, Piece piece)
    {
        if (piece == null)
        {
            return board.GetOverlayPieceAt(x, y) == null;
        }

        return !LogicLibrary.BoardLogic.IsVoid(piece)
               && !LogicLibrary.BoardLogic.IsCellSwapBlocked(board, x, y)
               && LogicLibrary.BoardLogic.IsPieceMoveable(piece);
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

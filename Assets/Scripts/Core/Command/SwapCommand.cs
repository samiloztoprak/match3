public class SwapCommand : ICommand
{
    private readonly Board _board;
    private readonly int _x1, _y1, _x2, _y2;
    public (int x, int y) TargetPosition => (_x2, _y2);
    
    public SwapCommand(Board board, int x1, int y1, int x2, int y2)
    {
        _board = board;
        _x1 = x1;
        _y1 = y1;
        _x2 = x2;
        _y2 = y2;
    }

    public void Execute()
    {        
        if(!LogicLibrary.BoardLogic.IsValidIndex(_x1, _y1, _board))
            return;
        
        if(!LogicLibrary.BoardLogic.IsValidIndex(_x2, _y2, _board))
            return;

        if(!LogicLibrary.BoardLogic.IsSwappableIndexes(_x1, _y1, _x2, _y2))
            return;
        
        Piece piece1 = _board.GetCell(_x1, _y1);
        if(piece1 == null)
            return;
        
        Piece piece2 = _board.GetCell(_x2, _y2);
        if(piece2 == null)
            return;
        
        if(LogicLibrary.BoardLogic.IsPieceLocked(piece1))
            return;

        if(LogicLibrary.BoardLogic.IsPieceLocked(piece2))
            return;
        
        if(!LogicLibrary.BoardLogic.IsPieceMoveable(piece1))
            return;

        if(!LogicLibrary.BoardLogic.IsPieceMoveable(piece2))
            return;
        
        _board.SetCell(_x1, _y1, piece2);
        _board.SetCell(_x2, _y2, piece1);
    }
}

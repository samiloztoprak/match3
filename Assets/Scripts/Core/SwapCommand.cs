public class SwapCommand : ICommand
{
    private readonly Board _board;
    private readonly int _x1, _y1, _x2, _y2;

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
        if(!BoardLogic.IsValidIndex(_x1, _y1, _board))
            return;
        
        if(!BoardLogic.IsValidIndex(_x2, _y2, _board))
            return;

        if(_board.IsCellLocked(_x1, _y1))
            return;

        if(_board.IsCellLocked(_x2, _y2))
            return;
        
        if(BoardLogic.IsSwappableIndices(_x1, _y1, _x2, _y2))
        {
            Piece tempPiece = _board.GetCell(_x1, _y1);
            _board.SetCell(_x1, _y1, _board.GetCell(_x2, _y2));
            _board.SetCell(_x2, _y2, tempPiece);
        }
    }
}

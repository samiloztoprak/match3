
public class Board
{
    private readonly Piece[,] _cells;

    public int Width => _cells.GetLength(0);
    public int Height => _cells.GetLength(1);

    public Board(int width, int height)
    {
        _cells = new Piece[width, height];
    } 
    
    public Piece GetCell(int x, int y)
    {
        return _cells[x, y];
    }

    public void SetCell(int x, int y, Piece value)
    {
        _cells[x,y] = value;
    }

    public bool IsCellLocked(int x, int y)
    {
        return false;
    }

    public void ClearCell(int x, int y)
    {
        SetCell(x, y, new Piece(PieceType.Empty));
    }
}

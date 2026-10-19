
using System.Collections.Generic;

public class Board
{
    private readonly Piece[,] _cells;
    private readonly List<OverlayPiece> _overlayPieces = new List<OverlayPiece>();
    
    private int _nextId;
    public int NextId() => _nextId++;

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

    public void ClearCell(int x, int y)
    {
        SetCell(x, y, null);
    }

    public void AddOverlayPiece(OverlayPiece overlayPiece)
    {
        _overlayPieces.Add(overlayPiece);
    }

    public void RemoveOverlayPiece(OverlayPiece overlayPiece)
    {
        _overlayPieces.Remove(overlayPiece);
    }

    public OverlayPiece GetOverlayPieceAt(int x, int y)
    {
        foreach (var overlay in _overlayPieces)
        {
            if (overlay.CoveredCells.Contains((x, y)))
            {
                return overlay;
            }
        }
        return null;
    }
}


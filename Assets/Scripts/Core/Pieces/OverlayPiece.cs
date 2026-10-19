
using System.Collections.Generic;

public abstract  class OverlayPiece : Piece
{
    public HashSet<(int x, int y)> CoveredCells { get; }

    public OverlayPiece(int id, PieceColor color, PieceFlags flags, HashSet<(int x, int y)> coveredCells) :
        base(id, color, flags)
    {
        CoveredCells = coveredCells;
    }
}

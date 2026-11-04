using System.Collections.Generic;

public struct ComboEffect
{
    public HashSet<(int x, int y)> Cells;
    public List<(Piece oldPiece, Piece newPiece, int x, int y)> Transformed;
}
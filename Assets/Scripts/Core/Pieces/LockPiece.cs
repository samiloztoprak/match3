
using System.Collections.Generic;

public sealed class LockPiece : OverlayPiece
{
    public LockPiece(int id, HashSet<(int x, int y)> coveredCells)
        : base(id, PieceColor.Empty, PieceFlags.DamageableByNeighbor, coveredCells)
    {
        VisualType = PieceVisualType.Lock;
        Health = 1;
    }
}


using System.Collections.Generic;

public sealed class LockPiece : OverlayPiece
{
    public LockPiece(int id, int x, int y)
        : base(id, PieceColor.Empty, PieceFlags.DamageableByNeighbor, new HashSet<(int x, int y)> { (x, y) })
    {
        VisualType = PieceVisualType.Lock;
        Health = 1;
    }
}

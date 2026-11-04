
using System.Collections.Generic;

public sealed class GrassPiece : OverlayPiece
{
    public GrassPiece(int id, int x, int y)
        : base(id, PieceColor.Empty, PieceFlags.DamageableByNeighbor, new HashSet<(int x, int y)> { (x, y) })
    {
        VisualType = PieceVisualType.Grass;
        GoalKind = GoalType.ClearGrass;
        Health = 1;
    }
}
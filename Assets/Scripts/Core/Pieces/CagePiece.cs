using System.Collections.Generic;

public sealed class CagePiece : OverlayPiece
{
    public CagePiece(int id, int x, int y)
        : base(id, PieceColor.Empty, PieceFlags.DamageableBySpecial, BuildCoveredCells(x, y))
    {
        VisualType = PieceVisualType.Cage;
        GoalKind = GoalType.ClearCage;
        Health = 3;
    }
    
    private static HashSet<(int x, int y)> BuildCoveredCells(int x, int y)
    {
        return new HashSet<(int x, int y)>
        {
            (x, y), (x + 1, y), (x, y + 1), (x + 1, y + 1)
        };
    }
}
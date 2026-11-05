
public sealed class GrassPiece : FloorPiece
{
    public GrassPiece(int id) : base(id)
    {
        VisualType = PieceVisualType.Grass;
        GoalKind = GoalType.ClearGrass;
        Health = 1;
    }
}
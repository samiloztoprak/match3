
public sealed class ToyPiece : Piece
{
    public ToyPiece(int id) : base(id, PieceColor.Empty, PieceFlags.Movable | PieceFlags.Collectable)
    {
        VisualType = PieceVisualType.Toy;
        GoalKind = GoalType.CollectToy;
        Health = 1;
    }
}
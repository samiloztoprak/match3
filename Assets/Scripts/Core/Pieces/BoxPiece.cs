
public sealed class BoxPiece : Piece
{
    public BoxPiece(int id, PieceColor color) : base(id, color, PieceFlags.DamageableByNeighbor)
    {
        VisualType = PieceVisualType.Box;
        Health = 3;
    }
}


public sealed class BoxPiece : Piece
{
    public BoxPiece(int id, PieceColor color) : base(id, color)
    {
        Flags = PieceFlags.DamageableByNeighbor;
        VisualType = PieceVisualType.Box;
        Health = 3;
    }
}

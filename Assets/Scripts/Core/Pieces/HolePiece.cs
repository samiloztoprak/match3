
public sealed class HolePiece : Piece
{
    public HolePiece(int id) : base(id, PieceColor.Empty, PieceFlags.Void)
    {
        VisualType = PieceVisualType.Hole;
    }
}
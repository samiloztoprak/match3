
public sealed class ColoredPiece : Piece
{
    public ColoredPiece(int id, PieceColor color) : base(id, color)
    {
        Flags = PieceFlags.Movable | PieceFlags.CanMatch;
        VisualType = PieceVisualType.Standard;
        Health = 1;
    }
}

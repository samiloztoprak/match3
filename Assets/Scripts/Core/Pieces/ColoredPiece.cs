
public sealed class ColoredPiece : Piece
{
    public ColoredPiece(int id, PieceType color) : base(id, color)
    {
        Flags = PieceFlags.Movable | PieceFlags.CanMatch;
    }
}


public sealed class BoxPiece : Piece
{
    public BoxPiece(int id, PieceType color) : base(id, color)
    {
        Flags = PieceFlags.None;
    }
}

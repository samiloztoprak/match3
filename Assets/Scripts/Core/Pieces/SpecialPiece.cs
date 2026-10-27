
public sealed class SpecialPiece : Piece
{
    public PieceKind Kind { get; }

    public SpecialPiece(int id, PieceKind kind) :
        base(id, PieceColor.Empty, PieceFlags.Movable)
    {
        Kind = kind;
        Health = 1;
    }
}

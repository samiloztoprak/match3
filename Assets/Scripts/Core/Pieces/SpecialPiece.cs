
public class SpecialPiece : Piece
{
    public PieceKind Kind { get; }

    public SpecialPiece(int id, PieceKind kind) :
        base(id, PieceColor.Empty, PieceFlags.Movable | PieceFlags.CrossFill | PieceFlags.DamageableBySpecial | PieceFlags.TargetableByPropeller)
    {
        Kind = kind;
        Health = 1;
        VisualType = PieceVisualType.Special;
    }
}

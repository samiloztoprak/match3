
public sealed class ColoredPiece : Piece
{
    public ColoredPiece(int id, PieceColor color) : base(id, color, PieceFlags.Movable 
                                                                    | PieceFlags.CanMatch | PieceFlags.CrossFill | PieceFlags.TargetableByPropeller)
    {
        VisualType = PieceVisualType.Standard;
        Health = 1;
    }
}


public sealed class RocketPiece : Piece
{
    public RocketDirection Direction { get; }

    public RocketPiece(int id, RocketDirection direction)
        : base(id, PieceColor.Empty, PieceFlags.Movable)
    {
        Direction = direction;
        Health = 1;
    }
}

public sealed class RocketPiece : SpecialPiece
{
    public RocketDirection Direction { get; }

    public RocketPiece(int id, RocketDirection direction)
        : base(id, PieceKind.Rocket)
    {
        Direction = direction;
    }
}
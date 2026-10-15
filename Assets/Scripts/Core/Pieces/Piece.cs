
public abstract class Piece
{
    protected Piece(int id, PieceType color)
    {
        Id = id;
        Color = color;
    }

    public int Id { get; }
    public PieceType Color { get; }
    public PieceFlags Flags { get; internal set;}
}

public abstract class Piece
{ 
    protected Piece(int id, PieceColor color, PieceFlags defaultFlags)
    {
        Id = id;
        Color = color;
        Flags = defaultFlags;
    }

    public int Id { get; }
    public PieceColor Color { get; }
    public PieceFlags Flags { get; internal set;}
    public int Health { get; internal set;}

    public bool ApplyDamage(int amount)
    {
        Health -= amount;
        return Health <= 0;
    }
 
    public PieceVisualType VisualType { get; internal set;}
}
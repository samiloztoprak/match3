
public readonly struct HitData
{
    public readonly int PieceId;
    public readonly int X;
    public readonly int Y;
    public readonly float TimeMs;

    public HitData(int pieceId, int x, int y, float timeMs)
    {
        PieceId = pieceId;
        X = x;
        Y = y;
        TimeMs = timeMs;
    }
}
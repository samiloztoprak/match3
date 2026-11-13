using System.Collections.Generic;

public sealed class LineBooster : IBoosterEffect
{
    private readonly RocketDirection _direction;

    public LineBooster(BoosterType type, RocketDirection direction)
    {
        Type = type;
        _direction = direction;
    }

    public BoosterType Type { get; }
    public float StartMs => ActivationTiming.BoosterLaunchMs;
    public RocketDirection Direction => _direction;

    public List<(int x, int y, float timeMs)> GetCells(Board board, int x, int y)
    {
        var cells = new List<(int x, int y, float timeMs)>();
        if (_direction == RocketDirection.Horizontal)
        {
            for (int cx = 0; cx < board.Width; cx++)
            {
                cells.Add((cx, y, ActivationTiming.RocketMsPerCell * System.Math.Abs(cx - x)));
            }
        }
        else
        {
            for (int cy = 0; cy < board.Height; cy++)
            {
                cells.Add((x, cy, ActivationTiming.RocketMsPerCell * System.Math.Abs(cy - y)));
            }
        }
        return cells;
    }
}
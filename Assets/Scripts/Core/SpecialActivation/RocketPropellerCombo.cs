using System.Collections.Generic;

public sealed class RocketPropellerCombo : IComboActivation
{
    public ComboEffect GetEffect(Board board, PieceKind kindA, PieceKind kindB, int x, int y, RocketDirection? rocketDirection, IRandom random, ITargetPriority targetPriority)
    {
        var (rx, ry) = CellPatterns.PickTarget(board, random, targetPriority);
        RocketDirection dir = rocketDirection ?? RocketDirection.Horizontal;

        var map = new Dictionary<(int x, int y), float>();
        map[(x, y)] = 0f;

        if (dir == RocketDirection.Horizontal)
        {
            for (int cx = 0; cx < board.Width; cx++)
            {
                float t = ActivationTiming.PropellerFlightMs + ActivationTiming.RocketMsPerCell * System.Math.Abs(cx - rx);
                if (!map.TryGetValue((cx, ry), out float old) || t < old) map[(cx, ry)] = t;
            }
        }
        else
        {
            for (int cy = 0; cy < board.Height; cy++)
            {
                float t = ActivationTiming.PropellerFlightMs + ActivationTiming.RocketMsPerCell * System.Math.Abs(cy - ry);
                if (!map.TryGetValue((rx, cy), out float old) || t < old) map[(rx, cy)] = t;
            }
        }

        var timed = new List<(int x, int y, float timeMs)>();
        foreach (var kvp in map) timed.Add((kvp.Key.x, kvp.Key.y, kvp.Value));

        return new ComboEffect { TimedCells = timed, FlightTargets = new List<(int x, int y)> { (rx, ry) } };

    }
}
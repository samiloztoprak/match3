using System.Collections.Generic;

public sealed class RocketPropellerCombo : IComboActivation
{
    public ComboEffect GetEffect(Board board, PieceKind kindA, PieceKind kindB, int x, int y, RocketDirection? rocketDirection, IRandom random, ITargetPriority targetPriority)
    {
        var takeoff = CellPatterns.PropellerTakeoff(board, x, y);
        var (rx, ry) = CellPatterns.PickTarget(board, random, targetPriority, CellPatterns.ToCellSet(takeoff));
        RocketDirection dir = rocketDirection ?? RocketDirection.Horizontal;

        var rocket = new List<(int x, int y, float timeMs)>();
        if (dir == RocketDirection.Horizontal)
        {
            for (int cx = 0; cx < board.Width; cx++)
            {
                rocket.Add((cx, ry, ActivationTiming.PropellerFlightMs + ActivationTiming.RocketMsPerCell * System.Math.Abs(cx - rx)));
            }
        }
        else
        {
            for (int cy = 0; cy < board.Height; cy++)
            {
                rocket.Add((rx, cy, ActivationTiming.PropellerFlightMs + ActivationTiming.RocketMsPerCell * System.Math.Abs(cy - ry)));
            }
        }

        return new ComboEffect
        {
            TimedCells = takeoff,
            FlightTargets = new List<(int x, int y)> { (rx, ry) },
            LandingStrikes = new List<List<(int x, int y, float timeMs)>> { rocket },
        };
    }
}
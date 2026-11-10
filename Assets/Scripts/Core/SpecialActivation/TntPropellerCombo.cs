using System.Collections.Generic;

public sealed class TntPropellerCombo : IComboActivation
{
    public ComboEffect GetEffect(Board board, PieceKind kindA, PieceKind kindB, int x, int y, RocketDirection? rocketDirection, IRandom random, ITargetPriority targetPriority)
    {
        var takeoff = CellPatterns.PropellerTakeoff(board, x, y);
        var (rx, ry) = CellPatterns.PickTarget(board, random, targetPriority, CellPatterns.ToCellSet(takeoff));

        var area = new HashSet<(int x, int y)>();
        CellPatterns.AddArea(board, rx, ry, ActivationArea.TntPropellerRadius, area);
        var blast = new List<(int x, int y, float timeMs)>();
        foreach (var (cx, cy) in area)
        {
            blast.Add((cx, cy, ActivationTiming.PropellerFlightMs));
        }

        return new ComboEffect
        {
            TimedCells = takeoff,
            FlightTargets = new List<(int x, int y)> { (rx, ry) },
            LandingStrikes = new List<List<(int x, int y, float timeMs)>> { blast },
        };
    }
}
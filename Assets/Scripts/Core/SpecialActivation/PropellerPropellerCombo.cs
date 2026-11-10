using System.Collections.Generic;

public sealed class PropellerPropellerCombo : IComboActivation
{
    public ComboEffect GetEffect(Board board, PieceKind kindA, PieceKind kindB, int x, int y, RocketDirection? rocketDirection, IRandom random, ITargetPriority targetPriority)
    {
        var takeoff = CellPatterns.PropellerTakeoff(board, x, y);
        var picked = CellPatterns.ToCellSet(takeoff);
        var order = new List<(int x, int y)>();
        var strikes = new List<List<(int x, int y, float timeMs)>>();
        for (int i = 0; i < ActivationArea.PropellerComboTargets; i++)
        {
            var target = CellPatterns.PickTarget(board, random, targetPriority, picked);
            picked.Add(target);
            order.Add(target);
            strikes.Add(new List<(int x, int y, float timeMs)> { (target.x, target.y, ActivationTiming.PropellerFlightMs) });
        }

        return new ComboEffect  
        { 
            TimedCells = takeoff,
            FlightTargets = order,
            LandingStrikes = strikes
        };
    }
}
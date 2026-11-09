using System.Collections.Generic;

public sealed class PropellerPropellerCombo : IComboActivation
{
    public ComboEffect GetEffect(Board board, PieceKind kindA, PieceKind kindB, int x, int y, RocketDirection? rocketDirection, IRandom random, ITargetPriority targetPriority)
    {
        var picked = new HashSet<(int x, int y)>();
        var order = new List<(int x, int y)>();
        for (int i = 0; i < ActivationArea.PropellerComboTargets; i++)
        {
            var target = CellPatterns.PickTarget(board, random, targetPriority, picked);
            picked.Add(target);
            order.Add(target);
        }
        return new ComboEffect { Cells = picked, FlightTargets = order };
    }
}
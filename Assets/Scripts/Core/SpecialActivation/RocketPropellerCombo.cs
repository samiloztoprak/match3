using System.Collections.Generic;

public sealed class RocketPropellerCombo : IComboActivation
{
    public ComboEffect GetEffect(Board board, PieceKind kindA, PieceKind kindB, int x, int y, RocketDirection? rocketDirection, IRandom random, ITargetPriority targetPriority)
    {
        var cells = new HashSet<(int x, int y)>();
        var (rx, ry) = CellPatterns.PickTarget(board, random, targetPriority);
        CellPatterns.AddDirectional(board, rx, ry, rocketDirection ?? RocketDirection.Horizontal, cells);
        return new ComboEffect { Cells = cells };
    }
}
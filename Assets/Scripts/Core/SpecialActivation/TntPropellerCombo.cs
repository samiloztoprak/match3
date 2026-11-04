using System.Collections.Generic;

public sealed class TntPropellerCombo : IComboActivation
{
    public ComboEffect GetEffect(Board board, PieceKind kindA, PieceKind kindB, int x, int y, RocketDirection? rocketDirection, IRandom random, ITargetPriority targetPriority)
    {
        var cells = new HashSet<(int x, int y)>();
        var (rx, ry) = CellPatterns.PickTarget(board, random, targetPriority);
        CellPatterns.AddArea(board, rx, ry, 1, cells);
        return new ComboEffect { Cells = cells };
    }
}
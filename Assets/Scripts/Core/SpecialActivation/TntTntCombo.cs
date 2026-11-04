using System.Collections.Generic;

public sealed class TntTntCombo : IComboActivation
{
    public ComboEffect GetEffect(Board board, PieceKind kindA, PieceKind kindB, int x, int y, RocketDirection? rocketDirection, IRandom random, ITargetPriority targetPriority)
    {
        var cells = new HashSet<(int x, int y)>();
        CellPatterns.AddArea(board, x, y, 4, cells);
        return new ComboEffect { Cells = cells };
    }
}
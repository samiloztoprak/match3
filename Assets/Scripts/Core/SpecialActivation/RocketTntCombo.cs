using System.Collections.Generic;

public sealed class RocketTntCombo : IComboActivation
{
    public ComboEffect GetEffect(Board board, PieceKind kindA, PieceKind kindB, int x, int y, RocketDirection? rocketDirection, IRandom random, ITargetPriority targetPriority)
    {
        var cells = new HashSet<(int x, int y)>();
        CellPatterns.AddThickCross(board, x, y, 1, cells);
        return new ComboEffect { Cells = cells };
    }
}
using System.Collections.Generic;

public sealed class RocketRocketCombo : IComboActivation
{
    public ComboEffect GetEffect(Board board, PieceKind kindA, PieceKind kindB, int x, int y, RocketDirection? rocketDirection, IRandom random, ITargetPriority targetPriority)
    {
        return new ComboEffect { TimedCells = CellPatterns.AddRowAndColumnTimed(board, x, y) };
    }
}
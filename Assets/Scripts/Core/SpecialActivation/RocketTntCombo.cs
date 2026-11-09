using System.Collections.Generic;

public sealed class RocketTntCombo : IComboActivation
{
    public ComboEffect GetEffect(Board board, PieceKind kindA, PieceKind kindB, int x, int y, RocketDirection? rocketDirection, IRandom random, ITargetPriority targetPriority)
    {
        return new ComboEffect { TimedCells = CellPatterns.AddThickCrossTimed(board, x, y, ActivationArea.RocketTntThickness) };
    }
}
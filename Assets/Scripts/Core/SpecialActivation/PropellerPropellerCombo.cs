using System.Collections.Generic;

public sealed class PropellerPropellerCombo : IComboActivation
{
    public ComboEffect GetEffect(Board board, PieceKind kindA, PieceKind kindB, int x, int y, RocketDirection? rocketDirection, IRandom random, ITargetPriority targetPriority)
    {
        var cells = new HashSet<(int x, int y)>();
        for (int i = 0; i < 3; i++)
        {
            cells.Add(CellPatterns.PickTarget(board, random, targetPriority));
        }
        return new ComboEffect { Cells = cells };
    }
}
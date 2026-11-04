using System.Collections.Generic;

public sealed class LightBallLightBallCombo : IComboActivation
{
    public ComboEffect GetEffect(Board board, PieceKind kindA, PieceKind kindB, int x, int y, RocketDirection? rocketDirection, IRandom random, ITargetPriority targetPriority)
    {
        var cells = new HashSet<(int x, int y)>();
        for (int cx = 0; cx < board.Width; cx++)
        {
            for (int cy = 0; cy < board.Height; cy++)
            {
                cells.Add((cx, cy));
            }
        }
        return new ComboEffect { Cells = cells };
    }
}
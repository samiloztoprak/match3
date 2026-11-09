using System.Collections.Generic;

public sealed class TntTntCombo : IComboActivation
{
    public ComboEffect GetEffect(Board board, PieceKind kindA, PieceKind kindB, int x, int y, RocketDirection? rocketDirection, IRandom random, ITargetPriority targetPriority)
    {
        var timed = new List<(int x, int y, float timeMs)>();
        int r = ActivationArea.TntComboRadius;
        for (int cx = x - r; cx <= x + r; cx++)
        {
            for (int cy = y - r; cy <= y + r; cy++)
            {
                if (cx < 0 || cx >= board.Width || cy < 0 || cy >= board.Height) continue;
                int ring = ActivationTiming.ChebyshevDistance(x, y, cx, cy);
                float t = ActivationTiming.TntDelayMs + ActivationTiming.TntRingMs * ring;
                timed.Add((cx, cy, t));
            }
        }
        return new ComboEffect { TimedCells = timed };
    }
}
using System.Collections.Generic;

public sealed class LightBallLightBallCombo : IComboActivation
{
    public ComboEffect GetEffect(Board board, PieceKind kindA, PieceKind kindB, int x, int y, RocketDirection? rocketDirection, IRandom random, ITargetPriority targetPriority)
    {
        var timed = new List<(int x, int y, float timeMs)>();
        for (int cx = 0; cx < board.Width; cx++)
        {
            for (int cy = 0; cy < board.Height; cy++)
            {
                int ring = ActivationTiming.ChebyshevDistance(x, y, cx, cy);
                timed.Add((cx, cy, ActivationTiming.LightBallWaveMsPerRing * ring));
            }
        }
        return new ComboEffect { TimedCells = timed };
    }
}
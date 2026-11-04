using System.Collections.Generic;

public sealed class TntActivation : ISpecialActivation
{
    public PieceKind Kind => PieceKind.Tnt;

    public ActivationEffect GetAffectedCells(Board board, Piece piece, int x, int y, Piece partner, IRandom random, ITargetPriority targetPriority)
    {
        var cells = new List<(int x, int y, float timeMs)>();
        for (int cx = x - 1; cx <= x + 1; cx++)
        {
            for (int cy = y - 1; cy <= y + 1; cy++)
            {
                if (cx < 0 || cx >= board.Width || cy < 0 || cy >= board.Height)
                {
                    continue;
                }
                int ring = ActivationTiming.ChebyshevDistance(x, y, cx, cy);
                float t = ActivationTiming.TntDelayMs + ActivationTiming.TntRingMs * ring;
                cells.Add((cx, cy, t));
            }
        }
        return new ActivationEffect { Cells = cells, FlightTarget = null };
    }
}
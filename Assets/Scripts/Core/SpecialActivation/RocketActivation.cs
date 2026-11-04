using System.Collections.Generic;

public sealed class RocketActivation : ISpecialActivation
{
    public PieceKind Kind => PieceKind.Rocket;

    public ActivationEffect GetAffectedCells(Board board, Piece piece, int x, int y, Piece partner, IRandom random, ITargetPriority targetPriority)
    {
        var cells = new List<(int x, int y, float timeMs)>();
        RocketPiece rocket = (RocketPiece)piece;

        if (rocket.Direction == RocketDirection.Horizontal)
        {
            for (int cx = 0; cx < board.Width; cx++)
            {
                float t = ActivationTiming.RocketMsPerCell * System.Math.Abs(cx - x);
                cells.Add((cx, y, t));
            }
        }
        else
        {
            for (int cy = 0; cy < board.Height; cy++)
            {
                float t = ActivationTiming.RocketMsPerCell * System.Math.Abs(cy - y);
                cells.Add((x, cy, t));
            }
        }
        return new ActivationEffect { Cells = cells, FlightTarget = null };
    }
}
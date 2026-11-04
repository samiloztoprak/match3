using System.Collections.Generic;

public sealed class LightBallActivation : ISpecialActivation
{
    public PieceKind Kind => PieceKind.LightBall;

    public ActivationEffect GetAffectedCells(Board board, Piece piece, int x, int y, Piece partner, IRandom random, ITargetPriority targetPriority)
    {
        var cells = new List<(int x, int y, float timeMs)>();
        PieceColor targetColor = (partner != null && partner.VisualType == PieceVisualType.Standard && partner.Color != PieceColor.Empty)
            ? partner.Color
            : CellPatterns.GetMostCommonColor(board);

        if (targetColor != PieceColor.Empty)
        {
            for (int cx = 0; cx < board.Width; cx++)
            {
                for (int cy = 0; cy < board.Height; cy++)
                {
                    Piece p = board.GetCell(cx, cy);
                    if (p != null && p.VisualType == PieceVisualType.Standard && p.Color == targetColor)
                    {
                        cells.Add((cx, cy, 0f));
                    }
                }
            }
        }
        cells.Add((x, y, 0f));
        return new ActivationEffect { Cells = cells, FlightTarget = null };
    }
}
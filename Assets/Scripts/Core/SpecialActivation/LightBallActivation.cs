using System.Collections.Generic;
using LogicLibrary;

public sealed class LightBallActivation : ISpecialActivation
{
    public PieceKind Kind => PieceKind.LightBall;

    public ActivationEffect GetAffectedCells(Board board, Piece piece, int x, int y, Piece partner, IRandom random, ITargetPriority targetPriority)
    {
        var cells = new List<(int x, int y, float timeMs)>();
        PieceColor targetColor = (partner != null && partner.VisualType == PieceVisualType.Standard && partner.Color != PieceColor.Empty)
            ? partner.Color
            : CellPatterns.GetMostCommonColor(board);

        var targets = new List<(int x, int y)>();
        if (targetColor != PieceColor.Empty)
        {
            for (int cx = 0; cx < board.Width; cx++)
            {
                for (int cy = 0; cy < board.Height; cy++)
                {
                    Piece p = board.GetCell(cx, cy);
                    if (BoardLogic.IsColorTargetable(p) && p.Color == targetColor)
                    {
                        targets.Add((cx, cy));
                    }
                }
            }
        }
        
        targets.Sort((a, b) =>
        {
            int da = (a.x - x) * (a.x - x) + (a.y - y) * (a.y - y);
            int db = (b.x - x) * (b.x - x) + (b.y - y) * (b.y - y);
            if (da != db) return da.CompareTo(db);
            if (a.x != b.x) return a.x.CompareTo(b.x);
            return a.y.CompareTo(b.y);
        });

        float revealMs = ActivationTiming.LightBallStartMs + ActivationTiming.LightBallStepMs * targets.Count;
        float clearMs = revealMs + ActivationTiming.LightBallPulseMs;

        foreach (var target in targets)
        {
            cells.Add((target.x, target.y, clearMs));
        }
        cells.Add((x, y, clearMs));

        return new ActivationEffect { Cells = cells, FlightTarget = null };
    }
}

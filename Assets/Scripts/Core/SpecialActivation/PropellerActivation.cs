using System.Collections.Generic;

public sealed class PropellerActivation : ISpecialActivation
{
    public PieceKind Kind => PieceKind.Propeller;

    public ActivationEffect GetAffectedCells(Board board, Piece piece, int x, int y, Piece partner, IRandom random, ITargetPriority targetPriority)
    {
        var cells = new List<(int x, int y, float timeMs)>();
        var (targetX, targetY) = CellPatterns.PickTarget(board, random, targetPriority);
        cells.Add((targetX, targetY, 0f));
        cells.Add((x, y, 0f));

        var neighborCells = CellPatterns.AddNeighbors(board, x, y, cells);
        return new ActivationEffect { Cells = cells, FlightTarget = (targetX, targetY), NeighborCells = neighborCells };
    }
}
using System.Collections.Generic;

public sealed class PropellerActivation : ISpecialActivation
{
    public PieceKind Kind => PieceKind.Propeller;

    public ActivationEffect GetAffectedCells(Board board, Piece piece, int x, int y, Piece partner, IRandom random, ITargetPriority targetPriority)
    {
        var cells = new List<(int x, int y, float timeMs)>();
        var (targetX, targetY) = CellPatterns.PickTarget(board, random, targetPriority);

        var neighborCells = CellPatterns.AddNeighbors(board, x, y, cells);

        cells.Add((targetX, targetY, ActivationTiming.PropellerFlightMs));
        cells.Add((x, y, ActivationTiming.PropellerFlightMs));

        return new ActivationEffect { Cells = cells, FlightTarget = (targetX, targetY), NeighborCells = neighborCells };
    }
}

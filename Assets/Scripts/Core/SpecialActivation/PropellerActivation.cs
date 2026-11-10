using System.Collections.Generic;

public sealed class PropellerActivation : ISpecialActivation
{
    public PieceKind Kind => PieceKind.Propeller;

    public ActivationEffect GetAffectedCells(Board board, Piece piece, int x, int y, Piece partner, IRandom random, ITargetPriority targetPriority)
    {
        var cells = new List<(int x, int y, float timeMs)> { (x, y, 0f) };
        var neighborCells = CellPatterns.AddNeighbors(board, x, y, cells);

        var (targetX, targetY) = CellPatterns.PickTarget(board, random, targetPriority, CellPatterns.ToCellSet(cells));

        var landing = new List<(int x, int y, float timeMs)>
        {
            (targetX, targetY, ActivationTiming.PropellerFlightMs),
        };

        return new ActivationEffect { Cells = cells, LandingCells = landing, FlightTarget = (targetX, targetY), NeighborCells = neighborCells };
    }
}

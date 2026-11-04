using System.Collections.Generic;

public struct ActivationEffect
{
    public List<(int x, int y, float timeMs)> Cells;
    public (int x, int y)? FlightTarget;
    public List<(int x, int y)> NeighborCells;
}
using System.Collections.Generic;

public struct ComboEffect
{
    public HashSet<(int x, int y)> Cells;
    public List<(int x, int y, float timeMs)> TimedCells;
    public List<(Piece oldPiece, Piece newPiece, int x, int y)> Transformed;
    public List<(int x, int y)> FlightTargets;
    public List<List<(int x, int y, float timeMs)>> LandingStrikes;
}
using System.Collections.Generic;

public interface IBoosterEffect
{
    BoosterType Type { get; }
    float StartMs { get; }
    List<(int x, int y, float timeMs)> GetCells(Board board, int x, int y);
}
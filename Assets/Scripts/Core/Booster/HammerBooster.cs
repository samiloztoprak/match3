using System.Collections.Generic;

public sealed class HammerBooster : IBoosterEffect
{
    public BoosterType Type => BoosterType.Hammer;
    public float StartMs => ActivationTiming.HammerStrikeMs;

    public List<(int x, int y, float timeMs)> GetCells(Board board, int x, int y)
    {
        return new List<(int x, int y, float timeMs)> { (x, y, 0f) };
    }
}
using System.Collections.Generic;

public sealed class BoosterActivationData : TurnStepData
{
    public BoosterType Type;
    public int X;
    public int Y;
    public float TimeMs;
    public List<(int x, int y, float timeMs)> Cells;
}
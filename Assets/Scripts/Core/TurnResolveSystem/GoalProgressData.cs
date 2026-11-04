using System.Collections.Generic;

public sealed class GoalProgressData : TurnStepData
{
    public GoalProgressData()
    {
        ClearedColors = new List<PieceColor>();
    }

    public List<PieceColor> ClearedColors;
    public int ClearedBoxCount;
    public int ClearedLockCount;
    public int ClearedGrassCount;
    public int ClearedCageCount;
}
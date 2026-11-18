using System;

[Serializable]
public struct LevelGoal
{
    public GoalType Type;
    public PieceColor Color;   // just Type == CollectColor
    public int TargetCount;
}
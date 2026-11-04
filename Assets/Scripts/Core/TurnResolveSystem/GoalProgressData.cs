using System.Collections.Generic;

public sealed class GoalProgressData : TurnStepData
{
    public GoalProgressData()
    {
        ClearedColors = new List<PieceColor>();
        ClearedCounts = new Dictionary<GoalType, int>();
    }

    public List<PieceColor> ClearedColors;
    public Dictionary<GoalType, int> ClearedCounts;

    public void Increment(GoalType? kind)
    {
        if (kind == null) return;
        ClearedCounts.TryGetValue(kind.Value, out int count);
        ClearedCounts[kind.Value] = count + 1;
    }

    public int GetCount(GoalType type)
    {
        ClearedCounts.TryGetValue(type, out int count);
        return count;
    }
}
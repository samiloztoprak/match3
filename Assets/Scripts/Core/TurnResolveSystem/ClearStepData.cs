using System.Collections.Generic;

public sealed class ClearStepData : TurnStepData
{
    public ClearStepData()
    {
        Cells = new List<(int, int)>();
    }
    
    public List<(int,int)> Cells;
}

using System.Collections.Generic;

public sealed class FallStepData : TurnStepData
{
    public FallStepData()
    {
        Moves = new List<(int, int, int, int)>();
    }
    
    public List<(int,int,int,int)> Moves;
}

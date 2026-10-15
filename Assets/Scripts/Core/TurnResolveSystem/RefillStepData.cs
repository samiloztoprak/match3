using System.Collections.Generic;

public sealed class RefillStepData :  TurnStepData
{
    public RefillStepData()
    {
        Moves = new List<(int, int, int, int)>();
    }
    
    public List<(int,int,int,int)> Moves;
}

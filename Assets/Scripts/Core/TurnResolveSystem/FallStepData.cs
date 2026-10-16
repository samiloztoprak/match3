using System.Collections.Generic;

public sealed class FallStepData : TurnStepData
{
    public FallStepData()
    {
        Moves = new List<(int, int, int)>();
    }
    
    public List<(int pieceId, int toX, int toY)> Moves;
}

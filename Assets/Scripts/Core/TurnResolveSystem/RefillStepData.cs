using System.Collections.Generic;

public sealed class RefillStepData :  TurnStepData
{
    public RefillStepData()
    {
        Moves = new List<(int, int, int)>();
    }
    
    public List<(int pieceId, int toX, int toY)> Moves;
}

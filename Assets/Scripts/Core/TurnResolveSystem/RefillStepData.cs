using System.Collections.Generic;

public sealed class RefillStepData :  TurnStepData
{
    public RefillStepData()
    {
        Moves = new List<(int, PieceType, int, int)>();
    }
    
    public List<(int pieceId, PieceType color, int toX, int toY)> Moves;
}

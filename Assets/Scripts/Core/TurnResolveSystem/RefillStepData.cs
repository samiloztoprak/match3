using System.Collections.Generic;

public sealed class RefillStepData :  TurnStepData
{
    public RefillStepData()
    {
        Moves = new List<(int, PieceColor, int, int)>();
    }
    
    public List<(int pieceId, PieceColor color, int toX, int toY)> Moves;
}

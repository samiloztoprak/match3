using System.Collections.Generic;

public sealed class ShuffleStepData : TurnStepData
{
    public ShuffleStepData()
    {
        Moves = new List<(int, int, int)>();
    }

    public List<(int pieceId, int toX, int toY)> Moves;
}
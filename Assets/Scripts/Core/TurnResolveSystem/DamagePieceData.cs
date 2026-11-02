
using System.Collections.Generic;

public class DamagePieceData : TurnStepData
{
    public DamagePieceData()
    {
        Hits = new List<(int pieceId, int healthAfter)>();
    }

    public List<(int pieceId, int healthAfter)> Hits;
}

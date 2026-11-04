
using System.Collections.Generic;

public class DamagePieceData : TurnStepData
{
    public DamagePieceData()
    {
        Hits = new List<(int pieceId, int healthAfter, float timeMs)>();
    }
    
    public List<(int pieceId, int healthAfter, float timeMs)> Hits;
}

using System.Collections.Generic;

public sealed class ClearPieceData : TurnStepData
{
    public ClearPieceData()
    {
        Hits = new List<HitData>();
    }
    
    public List<HitData> Hits;
}

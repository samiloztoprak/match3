
using System.Collections.Generic;

public class ClearOverlayData : TurnStepData
{
    public ClearOverlayData()
    {
        Hits = new List<HitData>();
    }
    
    public List<HitData> Hits;
}

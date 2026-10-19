
using System.Collections.Generic;

public class ClearOverlayData : TurnStepData
{
    public ClearOverlayData()
    {
        PieceIds = new List<int>();
    }
    
    public List<int> PieceIds;
}

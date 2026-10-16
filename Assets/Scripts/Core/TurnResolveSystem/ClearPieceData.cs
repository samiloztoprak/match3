using System.Collections.Generic;

public sealed class ClearPieceData : TurnStepData
{
    public ClearPieceData()
    {
        PieceIds = new List<int>();
    }
    
    public List<int> PieceIds;
}

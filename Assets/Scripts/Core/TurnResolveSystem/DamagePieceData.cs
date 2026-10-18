
using System.Collections.Generic;

public class DamagePieceData : TurnStepData
{
    public DamagePieceData()
    {
        PieceIds = new List<int>();
    }
    
    public List<int> PieceIds;
}

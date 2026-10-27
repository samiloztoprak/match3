
using System.Collections.Generic;

public sealed class SpecialCreatedData : TurnStepData
{
    public SpecialCreatedData()
    {
        MergedIds = new List<int>();
    }
    
    public int PieceId;
    public PieceKind Kind;
    public int X;
    public int Y;
    public List<int> MergedIds;
}
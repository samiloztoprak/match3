using System.Collections.Generic;

public class PendingPresentationSlot
{
    private List<(int, int, int, int)> _movementList;
    private bool bPresentationDone = false;

    public void Submit(List<(int,int,int,int)> movementList) 
    {
        _movementList = movementList;
    }

    public List<(int,int,int,int)> GetMoves() 
    { 
        return _movementList;
    }

    public void MarkComplete() 
    { 
        bPresentationDone = true;
    }

    public bool IsComplete() 
    { 
        return bPresentationDone;
    }

    public void Reset()
    {
        _movementList.Clear();
        bPresentationDone = false;
    }
}

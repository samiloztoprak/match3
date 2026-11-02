using System.Collections.Generic;
using UnityEngine;

public class MergeToAnchorViewCommand : IViewCommand
{
    private readonly List<int> _mergedIds;
    private readonly Vector3 _anchorPos;
    private readonly TurnPlayer _player;
    private readonly float _duration;
    
    public MergeToAnchorViewCommand(List<int> mergedIds, Vector3 anchorPos, TurnPlayer player, float duration)
    {
        _mergedIds = mergedIds;
        _anchorPos = anchorPos;
        _player = player;
        _duration = duration;
    }
    
    public float Duration => _duration;
    
    public void Execute()
    {
        foreach (int mergedId in _mergedIds)
        {
            PieceView view = _player.GetView(mergedId);
            view?.MoveTo(_anchorPos, _duration);
        }
    }
}

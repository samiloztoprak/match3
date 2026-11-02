using System.Collections.Generic;
using UnityEngine;

public class ClearPiecesViewCommand : IViewCommand
{
    private readonly List<int> _pieceIds;
    private readonly TurnPlayer _player;
    
    private readonly float _duration;
    public float Duration => _duration;
    
    public ClearPiecesViewCommand(List<int> pieceIds, TurnPlayer player, float duration)
    {
        _pieceIds = pieceIds;
        _player = player;
        _duration = duration;
    }
    
    public void Execute()
    {
        foreach (int pieceId in _pieceIds)
        {
            PieceView view = _player.GetView(pieceId);
            if (view == null) continue;

            _player.SpawnBreakEffect(view.transform.position);
            view.ShrinkAndRelease(_duration, v => _player.ReleasePooled(v));
            _player.UnregisterView(pieceId);
        }
    }
}

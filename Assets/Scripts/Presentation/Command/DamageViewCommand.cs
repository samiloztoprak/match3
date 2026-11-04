using System.Collections.Generic;
using UnityEngine;

public class DamageViewCommand : IViewCommand
{
    private readonly List<(int pieceId, int healthAfter, float timeMs)> _hits;
    private readonly TurnPlayer _player;
    private readonly float _duration;
    
    public DamageViewCommand(List<(int pieceId, int healthAfter, float timeMs)> hits, TurnPlayer player, float duration)
    {
        _hits = hits;
        _player = player;
        _duration = duration;
    }
    
    public float Duration => _duration;
    
    public void Execute()
    {
        foreach (var (pieceId, healthAfter, _) in _hits)
        {
            PieceView view = _player.GetView(pieceId);
            if (view == null) continue;

            _player.SpawnBreakEffect(view.transform.position);
            view.RefreshSprite(_player.VisualConfig, healthAfter);
        }
    }
}

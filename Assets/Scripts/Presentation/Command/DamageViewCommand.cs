using System.Collections.Generic;
using DG.Tweening;
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

        float maxTimeMs = 0f;
        foreach (var (_, _, timeMs) in hits)
        {
            if (timeMs > maxTimeMs)
            {
                maxTimeMs = timeMs;
            }
        }
        _duration = maxTimeMs / 1000f + duration;
    }

    public float Duration => _duration;

    public void Execute()
    {
        foreach (var (pieceId, healthAfter, timeMs) in _hits)
        {
            float delay = timeMs / 1000f;
            if (delay <= 0f)
            {
                ApplyDamage(pieceId, healthAfter);
            }
            else
            {
                DOVirtual.DelayedCall(delay, () => ApplyDamage(pieceId, healthAfter));
            }
        }
    }

    private void ApplyDamage(int pieceId, int healthAfter)
    {
        PieceView view = _player.GetView(pieceId);
        if (view == null) return;

        _player.SpawnBreakEffect(view.transform.position);
        view.RefreshSprite(_player.VisualConfig, healthAfter);
    }
}

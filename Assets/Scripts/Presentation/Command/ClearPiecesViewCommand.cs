using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ClearPiecesViewCommand : IViewCommand
{
    private readonly List<HitData> _hits;
    private readonly TurnPlayer _player;
    private readonly float _shrinkDuration;

    private readonly float _duration;
    public float Duration => _duration;

    public ClearPiecesViewCommand(List<HitData> hits, TurnPlayer player, float shrinkDuration)
    {
        _hits = hits;
        _player = player;
        _shrinkDuration = shrinkDuration;

        float maxTimeMs = 0f;
        foreach (var hit in hits)
        {
            if (hit.TimeMs > maxTimeMs)
            {
                maxTimeMs = hit.TimeMs;
            }
        }
        _duration = maxTimeMs / 1000f + shrinkDuration;
    }

    public void Execute()
    {
        foreach (HitData hit in _hits)
        {
            PieceView view = _player.GetView(hit.PieceId);
            if (view == null) continue;

            float delay = hit.TimeMs / 1000f;
            if (delay <= 0f)
            {
                ClearOne(hit.PieceId, view);
            }
            else
            {
                DOVirtual.DelayedCall(delay, () => ClearOne(hit.PieceId, view));
            }
        }
    }

    private void ClearOne(int pieceId, PieceView view)
    {
        _player.SpawnBreakEffect(view.transform.position);
        view.ShrinkAndRelease(_shrinkDuration, v => _player.ReleasePooled(v));
        _player.UnregisterView(pieceId);
    }
}

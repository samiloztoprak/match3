using System.Collections.Generic;
using UnityEngine;

public class ClearPiecesViewCommand : IViewCommand
{
    private readonly List<HitData> _hits;
    private readonly TurnPlayer _player;
    private readonly float _shrinkDuration;

    public float Duration => _shrinkDuration;

    public ClearPiecesViewCommand(List<HitData> hits, TurnPlayer player, float shrinkDuration)
    {
        _hits = hits;
        _player = player;
        _shrinkDuration = shrinkDuration;
    }

    public void Execute()
    {
        foreach (HitData hit in _hits)
        {
            PieceView view = _player.GetView(hit.PieceId);
            if (view == null) continue;

            _player.SpawnBreakEffect(view.transform.position);
            _player.UnregisterView(hit.PieceId);
            view.ShrinkAndRelease(_shrinkDuration, v => _player.ReleasePooled(v));
        }
    }
}

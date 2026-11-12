using System.Collections.Generic;
using UnityEngine;

public class ClearPiecesViewCommand : IViewCommand
{
    private const float GoalFlyStagger = 0.04f;

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
        GoalFlyer flyer = _player.GoalFlyer;
        int flyIndex = 0;

        foreach (HitData hit in _hits)
        {
            PieceView view = _player.GetView(hit.PieceId);
            if (view == null)
            {
                continue;
            }

            Vector3 position = view.transform.position;
            Piece piece = view.Piece;
            _player.UnregisterView(hit.PieceId);
            view.ShrinkAndRelease(_shrinkDuration, v => _player.ReleasePooled(v));
            _player.SpawnBreakEffect(piece, position);

            if (flyer != null && flyer.TryFly(piece, position, flyIndex * GoalFlyStagger))
            {
                flyIndex++;
            }
        }
    }
}
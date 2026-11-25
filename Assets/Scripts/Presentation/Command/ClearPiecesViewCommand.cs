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
        AudioManager.Play(SoundId.Match);
        
        GoalFlyer flyer = _player.GoalFlyer;
        int flyIndex = 0;

        foreach (HitData hit in _hits)
        {
            PieceView view = _player.GetView(hit.PieceId);
            if (view == null)
            {
                Debug.LogWarning($"[Clear] View not found: pieceId={hit.PieceId} ({hit.X},{hit.Y})");
                continue;
            }

            Vector3 position = view.transform.position;
            Piece piece = view.Piece;
            CageVisualConfig cage = _player.VisualConfig.Cage;
            if (piece is CagePiece && cage != null && cage.HasFrames)
            {
                _player.UnregisterView(hit.PieceId);
                view.PlayFramesTo(cage.Frames, cage.StopFrame(0), cage.Fps, cage.CatchUpSpeed,
                    () => _player.ReleasePooled(view));

                if (flyer != null && flyer.TryFly(piece, position, flyIndex * GoalFlyStagger))
                {
                    flyIndex++;
                }
                continue;
            }
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
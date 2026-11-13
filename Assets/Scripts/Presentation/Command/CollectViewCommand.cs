using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CollectViewCommand : IViewCommand
{
    private const float StaggerSeconds = 0.05f;
    private const float JumpHeightInCells = 0.8f;

    private readonly List<HitData> _hits;
    private readonly TurnPlayer _player;
    private readonly float _jumpDuration;
    private readonly float _flyDuration;

    public float Duration => _jumpDuration;

    public CollectViewCommand(List<HitData> hits, TurnPlayer player, float jumpDuration, float flyDuration)
    {
        _hits = hits;
        _player = player;
        _jumpDuration = jumpDuration;
        _flyDuration = flyDuration;
    }

    public void Execute()
    {
        GoalFlyer flyer = _player.GoalFlyer;
        float jumpHeight = _player.CellSize * JumpHeightInCells;

        for (int i = 0; i < _hits.Count; i++)
        {
            HitData hit = _hits[i];
            PieceView view = _player.GetView(hit.PieceId);
            if (view == null)
            {
                continue;
            }

            GoalSlotView slot = null;
            Vector3 target = _player.CellToWorld(hit.X, -3);
            if (flyer != null && flyer.TryReserve(view.Piece, out GoalSlotView reserved, out Vector3 slotTarget))
            {
                slot = reserved;
                target = slotTarget;
            }

            _player.UnregisterView(hit.PieceId);
            view.KillTweens();
            view.SetSortingOrder(SortingOrders.Flight);

            Transform t = view.transform;
            Vector3 baseScale = t.localScale;
            Vector3 start = _player.CellToWorld(hit.X, hit.Y);
            t.position = start;

            Sequence seq = DOTween.Sequence().SetTarget(t);
            seq.AppendInterval(i * StaggerSeconds);
            seq.Append(t.DOJump(start, jumpHeight, 1, _jumpDuration));
            seq.Join(t.DOScale(baseScale * 1.2f, _jumpDuration * 0.5f).SetLoops(2, LoopType.Yoyo));
            seq.Append(t.DOMove(target, _flyDuration).SetEase(Ease.InQuad));
            seq.Join(t.DOScale(baseScale * 0.6f, _flyDuration).SetEase(Ease.InQuad));
            seq.Join(t.DORotate(new Vector3(0f, 0f, 360f), _flyDuration, RotateMode.FastBeyond360));
            seq.OnComplete(() =>
            {
                if (slot != null)
                {
                    flyer.CompleteFlight(slot);
                }
                _player.ReleasePooled(view);
            });
        }
    }
}
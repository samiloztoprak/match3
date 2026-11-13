using DG.Tweening;
using UnityEngine;

public class HammerStrikeViewCommand : IViewCommand
{
    private readonly int _x;
    private readonly int _y;
    private readonly TurnPlayer _player;

    public float Duration => ActivationTiming.HammerStrikeMs / 1000f;

    public HammerStrikeViewCommand(int x, int y, TurnPlayer player)
    {
        _x = x;
        _y = y;
        _player = player;
    }

    public void Execute()
    {
        Vector3 cell = _player.CellToWorld(_x, _y);
        float s = _player.CellSize;

        SpriteRenderer prefab = _player.HammerPrefab;
        SpriteRenderer hammer = _player.Effects.Spawn(prefab, cell + new Vector3(0.9f * s, 1.0f * s, 0f));
        hammer.sortingOrder = SortingOrders.Flight;

        Transform t = hammer.transform;
        Vector3 baseScale = prefab.transform.localScale;
        t.localScale = Vector3.zero;
        t.rotation = Quaternion.Euler(0f, 0f, -40f);

        float windUp = Duration * 0.45f;
        float swing = Duration - windUp;

        Sequence seq = DOTween.Sequence().SetTarget(t);
        seq.Append(t.DOScale(baseScale, windUp).SetEase(Ease.OutBack));
        seq.Join(t.DORotate(new Vector3(0f, 0f, -60f), windUp).SetEase(Ease.OutQuad));        // raise
        seq.Append(t.DORotate(new Vector3(0f, 0f, 20f), swing).SetEase(Ease.InCubic));         // swing down
        seq.Join(t.DOMove(cell + new Vector3(0.45f * s, 0.4f * s, 0f), swing).SetEase(Ease.InCubic));
        seq.AppendCallback(() =>
        {
            SpawnImpact(cell);
            if (_player.CameraShake != null)
            {
                _player.CameraShake.Add(0.15f);
            }
        });
        seq.Append(t.DORotate(new Vector3(0f, 0f, -10f), 0.12f).SetEase(Ease.OutQuad));       // bounce back
        seq.Append(t.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack));
        seq.OnComplete(() => _player.Effects.Release(hammer));
    }

    private void SpawnImpact(Vector3 position)
    {
        SpriteRenderer prefab = _player.HammerImpactPrefab;
        SpriteRenderer impact = _player.Effects.Spawn(prefab, position);
        impact.sortingOrder = SortingOrders.Flight - 1;
        impact.color = Color.white;

        Transform t = impact.transform;
        Vector3 baseScale = prefab.transform.localScale;
        t.localScale = baseScale * 0.3f;

        Sequence seq = DOTween.Sequence().SetTarget(t);
        seq.Append(t.DOScale(baseScale * 1.1f, 0.12f).SetEase(Ease.OutBack));
        seq.Append(impact.DOFade(0f, 0.18f));
        seq.OnComplete(() => _player.Effects.Release(impact));
    }
}
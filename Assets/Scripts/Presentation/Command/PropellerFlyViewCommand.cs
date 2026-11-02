using DG.Tweening;
using UnityEngine;

public class PropellerFlyViewCommand : IViewCommand
{
    private readonly int _pieceId;
    private readonly Vector3 _targetPos;
    private readonly TurnPlayer _player;
    private readonly PropellerAnimationConfig _config;
    
    public PropellerFlyViewCommand(int pieceId, Vector3 targetPos, TurnPlayer player, PropellerAnimationConfig config)
    {
        _pieceId = pieceId;
        _targetPos = targetPos;
        _player = player;
        _config = config;
    }
    
    public float Duration => _config.FlightDuration;
    
    public void Execute()
    {
        PieceView view = _player.GetView(_pieceId);
        if (view == null) return;

        view.SetSortingOrder(_config.FlightSortingOrder);

        Transform t = view.transform;
        Vector3 baseScale = t.localScale;
        Vector3 start = t.position;
        Vector3 end = _targetPos;

        Vector3 toTarget = end - start;
        Vector3 perpendicular = new Vector3(-toTarget.y, toTarget.x, 0f).normalized;
        float sideways = Mathf.Max(toTarget.magnitude * _config.WeaveFactor, _config.MinWeaveAmount)
                         * (Random.value > 0.5f ? 1f : -1f);
        float arcHeight = Mathf.Max(toTarget.magnitude * _config.ArcHeightFactor, _config.MinArcHeight);

        float progress = 0f;
        DOTween.To(() => progress, p => progress = p, 1f, _config.FlightDuration)
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                float arcFactor = Mathf.Sin(progress * Mathf.PI);
                Vector3 flatPos = Vector3.Lerp(start, end, progress) + perpendicular * (sideways * arcFactor);
                t.position = flatPos + Vector3.up * (arcHeight * arcFactor);
                t.localScale = baseScale * (1f + arcFactor * (_config.PeakScale - 1f));
            })
            .OnComplete(() =>
            {
                t.position = end;
                t.localScale = baseScale;
            });
    }
}

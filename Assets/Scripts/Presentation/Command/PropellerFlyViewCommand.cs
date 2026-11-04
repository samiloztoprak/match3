using DG.Tweening;
using UnityEngine;

public class PropellerFlyViewCommand : IViewCommand
{
    private readonly int _pieceId;
    private readonly Vector3 _targetPos;
    private readonly TurnPlayer _player;
    private readonly PropellerAnimationConfig _config;
    private readonly float _flightDuration;
    
    public PropellerFlyViewCommand(int pieceId, Vector3 targetPos, TurnPlayer player, PropellerAnimationConfig config, float flightDuration)
    {
        _pieceId = pieceId;
        _targetPos = targetPos;
        _player = player;
        _config = config;
        _flightDuration = flightDuration;
    }
    
    public float Duration => _flightDuration;
    
    public void Execute()
    {
        PieceView view = _player.GetView(_pieceId);
        if (view == null) return;

        view.transform.DOKill();
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
        DOTween.To(() => progress, p => progress = p, 1f, _flightDuration)
            .SetEase(Ease.Linear)
            .SetTarget(t)
            .OnUpdate(() =>
            {
                float arcFactor = Mathf.Sin(progress * Mathf.PI);
                Vector3 flatPos = Vector3.Lerp(start, end, progress) + perpendicular * (sideways * arcFactor);
                t.position = flatPos + Vector3.up * (arcHeight * arcFactor);
                t.localScale = baseScale * (1f + arcFactor * (_config.PeakScale - 1f));
                t.rotation = Quaternion.Euler(0f, 0f, t.rotation.eulerAngles.z + 100f);
            })
            .OnComplete(() =>
            {
                t.position = end;
                t.localScale = baseScale;
            });
    }
}

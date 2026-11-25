using DG.Tweening;
using UnityEngine;

public class PropellerActivationView  : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _animSprite;
    [SerializeField] private string _stateName;

    public void Fly(Vector3 target, float duration, PropellerAnimationConfig config, System.Action onComplete)
    {
        Transform t = transform;
        Vector3 baseScale = t.localScale;
        Vector3 start = t.position;
        Vector3 end = target;

        Vector3 toTarget = end - start;
        Vector3 perpendicular = new Vector3(-toTarget.y, toTarget.x, 0f).normalized;
        float sideways = Mathf.Max(toTarget.magnitude * config.WeaveFactor, config.MinWeaveAmount)
                         * (Random.value > 0.5f ? 1f : -1f);
        float arcHeight = Mathf.Max(toTarget.magnitude * config.ArcHeightFactor, config.MinArcHeight);

        _animator.enabled = true;
        _animSprite.enabled = true;
        _animSprite.sortingOrder = config.FlightSortingOrder;
        _animator.Play(_stateName, 0, 0f);

        float progress = 0f;
        DOTween.To(() => progress, p => progress = p, 1f, duration)
            .SetEase(Ease.Linear)
            .SetTarget(t)
            .OnUpdate(() =>
            {
                float arcFactor = Mathf.Sin(progress * Mathf.PI);
                Vector3 flatPos = Vector3.Lerp(start, end, progress) + perpendicular * (sideways * arcFactor);
                t.position = flatPos + Vector3.up * (arcHeight * arcFactor);
                t.localScale = baseScale * (1f + arcFactor * (config.PeakScale - 1f));
            })
            .OnComplete(() =>
            {
                t.position = end;
                t.localScale = baseScale;
                onComplete?.Invoke();
            });
    }
}

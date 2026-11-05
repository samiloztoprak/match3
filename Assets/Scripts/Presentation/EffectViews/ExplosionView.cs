using DG.Tweening;
using UnityEngine;

public class ExplosionView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _ring;
    [SerializeField] private ParticleSystem _burst;
    [SerializeField] private float _lifetime = 0.6f;

    public void Play(float expandDuration, float diameter, System.Action onComplete)
    {
        _ring.sortingOrder = SortingOrders.Flight;
        _ring.transform.localScale = Vector3.zero;
        Color c = _ring.color; c.a = 1f; _ring.color = c;

        _ring.transform.DOScale(diameter, expandDuration).SetEase(Ease.OutQuad);
        _ring.DOFade(0f, 0.25f).SetDelay(expandDuration);

        _burst.Clear();
        _burst.Play();

        DOVirtual.DelayedCall(_lifetime, () => onComplete?.Invoke());
    }
}
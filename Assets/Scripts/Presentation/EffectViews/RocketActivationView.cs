using DG.Tweening;
using UnityEngine;

public class RocketActivationView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private ParticleSystem _trail;
    [SerializeField] private float _overshootDistance = 5f;
        
    private Vector3 _baseScale;
    void Awake()
    {
        _baseScale = transform.localScale;
    }
    
    public void SetSprite(Sprite sprite)
    {
        _spriteRenderer.sprite = sprite;
    }

    public void SetDirection(Vector3 direction)
    {
        transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
    }

    public void Play(Vector3 target, float duration, System.Action onComplete)
    {
        Vector3 start = transform.position;
        Vector3 toTarget = target - start;
        float distance = toTarget.magnitude;
        float speed = distance / duration;                       
        Vector3 end = start + toTarget / distance * (distance + _overshootDistance);
        float totalDuration = (distance + _overshootDistance) / speed;
        
        if (_trail != null)
        {
            _trail.Clear();
            _trail.Play();
        }
        _spriteRenderer.sortingOrder = SortingOrders.Flight;
        
        transform.localScale = _baseScale;
        transform.DOScale(_baseScale * 1.2f, .1f).SetEase(Ease.InOutSine);
        transform.DOMove(end, totalDuration).SetEase(Ease.Linear).OnComplete(() =>
        {
            if (_trail != null)
            {
                _trail.Stop();
            }
            onComplete?.Invoke();
        });
    }
}
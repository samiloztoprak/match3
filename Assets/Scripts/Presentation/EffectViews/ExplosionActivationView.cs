using DG.Tweening;
using UnityEngine;

public class ExplosionActivationView : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _animSprite;
    [SerializeField] private string _stateName;
    
    [SerializeField] private SpriteRenderer _ring;
    [SerializeField] private ParticleSystem _burst;
    [SerializeField] private float _lifetime = 0.6f;

    private float _expandDuration;
    private float _diameter;
    private System.Action  _onComplete;
    
    private void Awake()
    {
        _ring.gameObject.SetActive(false);
        _burst.gameObject.SetActive(false);
    }
    
    public void Play(float expandDuration, float diameter, System.Action onComplete)
    {
        _ring.gameObject.SetActive(false);
        _burst.gameObject.SetActive(false);
        _animator.enabled = true;
        _animSprite.enabled = true;
        
        _animator.Play(_stateName);
        _animSprite.sortingOrder = SortingOrders.Explosion;
        _expandDuration = expandDuration;
        _diameter = diameter;
        _onComplete = onComplete;
    }

    public void TriggerParticuleEffect()
    {
        _ring.gameObject.SetActive(true);
        _burst.gameObject.SetActive(true);
        _animator.enabled = false;
        _animSprite.enabled = false;
       
        _ring.sortingOrder = SortingOrders.Explosion;
        _ring.transform.localScale = Vector3.zero;
        Color c = _ring.color; c.a = 1f; _ring.color = c;

        _ring.transform.DOScale(_diameter, _expandDuration).SetEase(Ease.OutQuad);
        _ring.DOFade(0f, 0.25f).SetDelay(_expandDuration);

        _burst.Clear();
        _burst.Play();

        DOVirtual.DelayedCall(_lifetime, () => _onComplete?.Invoke());
    }
}
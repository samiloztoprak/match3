using DG.Tweening;
using UnityEngine;

public class LightBallActivationView : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _animSprite;
    [SerializeField] private string _stateName;
    
    public void Play(float lifetime, System.Action onComplete)
    {
        _animator.enabled = true;
        _animSprite.enabled = true;
        
        _animator.Play(_stateName);
        _animSprite.sortingOrder = SortingOrders.Explosion;
        DOVirtual.DelayedCall(lifetime, () => onComplete?.Invoke());
    }
}

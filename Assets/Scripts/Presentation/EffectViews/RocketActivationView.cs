using DG.Tweening;
using UnityEngine;

public class RocketActivationView : MonoBehaviour
{
    [System.Serializable]
    private class Head
    {
        public Transform Root;
        public SpriteRenderer Sprite;
        public Animator Anim;
        public string StateName;
    }
    
    [Header("Charge")]
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _animSprite;
    [SerializeField] private string _stateName;

    [Header("Heads")]
    [SerializeField] private Head _headA;
    [SerializeField] private Head _headB;
    [SerializeField] private float _overshootDistance = 5f;
        
    private Vector3 _baseScale;
    void Awake()
    {
        ResetHead(_headA);
        ResetHead(_headB);
    }

    public void Play(Vector3 direction, float chargeDuration, Vector3 targetA, float durationA, Vector3 targetB, float durationB,
        System.Action onComplete)
    {
        transform.rotation = FaceDirection(direction, -90);
        ResetHead(_headA);
        ResetHead(_headB);

        _animSprite.enabled = true;
        _animSprite.sortingOrder = SortingOrders.Flight;
        _animator.enabled = true;
        _animator.Play(_stateName, 0, 0f);

        float totalA = durationA > 0f ? FlightTime(targetA, durationA) : 0f;
        float totalB = durationB > 0f ? FlightTime(targetB, durationB) : 0f;

        Sequence seq = DOTween.Sequence().SetTarget(transform);
        seq.InsertCallback(chargeDuration, () =>
        {
            _animator.enabled = false;
            _animSprite.enabled = false;
            if (durationA > 0f) Launch(_headA, targetA, durationA);
            if (durationB > 0f) Launch(_headB, targetB, durationB);
        });
        seq.InsertCallback(chargeDuration + Mathf.Max(totalA, totalB), () => onComplete?.Invoke());
    }
    
    private float FlightTime(Vector3 target, float duration)
    {
        float distance = (target - transform.position).magnitude;
        return duration * (distance + _overshootDistance) / distance;
    }
    
    private void Launch(Head head, Vector3 target, float duration)
    {
        Vector3 start = transform.position;
        Vector3 toTarget = target - start;
        float distance = toTarget.magnitude;
        Vector3 dir = toTarget / distance;
        Vector3 end = start + dir * (distance + _overshootDistance);
        float total = duration * (distance + _overshootDistance) / distance;

        head.Root.gameObject.SetActive(true);
        head.Root.position = start;
        head.Root.rotation = FaceDirection(dir, 0);
        head.Sprite.sortingOrder = SortingOrders.Flight;

        if (head.Anim != null)
        {
            head.Anim.enabled = true;
            head.Anim.Play(head.StateName, 0, 0f);
        }

        head.Root.DOMove(end, total).SetEase(Ease.Linear).SetTarget(transform).OnComplete(() =>
        {
            if (head.Anim != null)
            {
                head.Anim.enabled = false;
            }
            head.Root.gameObject.SetActive(false);
        });
    }
    
    private static void ResetHead(Head head)
    {
        if (head?.Root == null) return;
        head.Root.gameObject.SetActive(false);
        head.Root.localPosition = Vector3.zero;
        if (head.Anim != null)
        {
            head.Anim.enabled = false;
        }
    }
    
    private static Quaternion FaceDirection(Vector3 dir, float RotOffset)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + RotOffset;
        return Quaternion.Euler(0f, 0f, angle);
    }
}
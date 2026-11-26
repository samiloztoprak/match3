using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class RewardFlyer : MonoBehaviour
{
    [SerializeField] private RectTransform _layer;
    [SerializeField] private Image _template;
    [SerializeField] private int _prewarm = 5;
    [SerializeField] private float _popScale = 1.3f;
    [SerializeField] private float _popDuration = 0.25f;
    [SerializeField] private float _holdDuration = 0.25f;
    [SerializeField] private float _flyDuration = 0.6f;
    [SerializeField] private float _stagger = 0.08f;
    [SerializeField] private float _spread = 60f;
    [SerializeField] private float _arcHeight = 180f;
    
    private readonly List<Image> _active = new List<Image>();
    private ObjectPool<Image> _pool;

    public Vector3 Center => _layer.TransformPoint(_layer.rect.center);
    
    private void OnValidate()
    {
        if (_layer == null) _layer = (RectTransform)transform;
        if (_template == null) _template = AutoRef.Child<Image>(this, "ui_fly_icon");
    }
    
    private void Awake()
    {
        _template.gameObject.SetActive(false);
        _pool = new ObjectPool<Image>(
            createFunc: () => Instantiate(_template, _layer),
            actionOnGet: icon =>
            {
                icon.gameObject.SetActive(true);
                icon.transform.SetAsLastSibling();
            },
            actionOnRelease: icon =>
            {
                icon.rectTransform.DOKill();
                icon.gameObject.SetActive(false);
            },
            actionOnDestroy: icon => Destroy(icon.gameObject),
            collectionCheck: true,
            defaultCapacity: _prewarm,
            maxSize: 16);

        var warm = new List<Image>(_prewarm);
        for (int i = 0; i < _prewarm; i++) warm.Add(_pool.Get());
        foreach (Image icon in warm) _pool.Release(icon);
    }

    private void OnDisable()
    {
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            _pool.Release(_active[i]);
        }
        _active.Clear();
    }
    
    public void Fly(Sprite icon, Vector3 from, RectTransform target, int count, Action onEachArrive, Action onAllArrived)
    {
        Fly(icon, from, 0f, target, count, onEachArrive, onAllArrived);
    }
    
    public void Fly(Sprite icon, Vector3 from, float startScale, RectTransform target, int count, Action onEachArrive, Action onAllArrived)
    {
        count = Mathf.Max(1, count);
        float unit = _layer.lossyScale.y;
        int arrived = 0;

        for (int i = 0; i < count; i++)
        {
            Image img = Spawn();
            img.sprite = icon;
            img.color = _template.color;

            RectTransform rt = img.rectTransform;
            rt.position = from;
            rt.localRotation = Quaternion.identity;
            rt.localScale = Vector3.one * startScale;

            float angle = count > 1 ? i * Mathf.PI * 2f / count : 0f;
            float spread = count > 1 ? _spread * unit : 0f;
            Vector3 burst = from + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * spread;

            Sequence seq = DOTween.Sequence().SetTarget(rt);
            seq.Append(rt.DOScale(_popScale, _popDuration).SetEase(Ease.OutBack));
            seq.Join(rt.DOMove(burst, _popDuration).SetEase(Ease.OutCubic));
            seq.AppendInterval(_holdDuration + i * _stagger);
            seq.Append(rt.DOJump(target.position, _arcHeight * unit, 1, _flyDuration).SetEase(Ease.InQuad));
            seq.Join(rt.DOScale(0.5f, _flyDuration).SetEase(Ease.InQuad));
            seq.OnComplete(() =>
            {
                Return(img);
                onEachArrive?.Invoke();
                arrived++;
                if (arrived == count) onAllArrived?.Invoke();
            });
        }
    }
    
    private Image Spawn()
    {
        Image img = _pool.Get();
        _active.Add(img);
        return img;
    }

    private void Return(Image img)
    {
        _active.Remove(img);
        _pool.Release(img);
    }
}
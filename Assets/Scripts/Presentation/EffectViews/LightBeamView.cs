using DG.Tweening;
using UnityEngine;

public class LightBeamView : MonoBehaviour
{
    [SerializeField] private LineRenderer _line;
    [SerializeField] private int _segments = 12;
    [SerializeField] private float _amplitude = 0.08f;
    [SerializeField] private float _frequency = 18f;
    [SerializeField] private float _speed = 6f;
    [SerializeField] private float _fadeInDuration = 0.15f;
    
    private Vector3 _from;
    private Vector3 _to;
    private bool _active;
    private Tweener _fadeTween;
    
    public void Awake()
    {
        _line.useWorldSpace = true;
    }

    public void Play(Vector3 from, Vector3 to)
    {
        _from = from;
        _to = to;
        _active = true;
        _line.positionCount = _segments;
        
        _fadeTween?.Kill();
        SetAlpha(0f);
        _fadeTween = DOTween.To(SetAlpha, 0f, 1f, _fadeInDuration);
    }

    public void Stop()
    {
        _active = false;
        _fadeTween?.Kill();
    }
    
    private void SetAlpha(float a)
    {
        Color start = _line.startColor;
        start.a = a;
        _line.startColor = start;

        Color end = _line.endColor;
        end.a = a;
        _line.endColor = end;
    }
    
    void Update()
    {
        if (!_active) return;

        Vector3 dir = _to - _from;
        Vector3 perpendicular = new Vector3(-dir.y, dir.x, 0f).normalized;

        for (int i = 0; i < _segments; i++)
        {
            float p = i / (float)(_segments - 1);
            Vector3 point = Vector3.Lerp(_from, _to, p);
            float taper = Mathf.Sin(p * Mathf.PI);
            float wave = Mathf.Sin(p * _frequency + Time.time * _speed) * _amplitude * taper;
            point += perpendicular * wave;
            _line.SetPosition(i, point);
        }
    }
}
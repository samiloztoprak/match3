using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WheelView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform _disc;
    [SerializeField] private RectTransform _slotTemplate;
    [SerializeField] private RectTransform _pointer;

    [Header("Layout")]
    [SerializeField] private float _iconRadius = 220f;

    [Header("Idle")]
    [SerializeField] private float _idleSpeed = 18f;
    [SerializeField] private float _idleResumeDuration = 0.8f;

    [Header("Spin")]
    [SerializeField] private float _accelDuration = 0.5f;
    [SerializeField] private float _decelDuration = 3f;
    [SerializeField] private int _fullTurns = 5;
    [SerializeField, Range(0f, 0.45f)] private float _landingJitter = 0.35f;

    [Header("Pointer")]
    [SerializeField] private float _pointerKick = 14f;
    [SerializeField] private float _pointerReturn = 0.18f;

    private readonly List<RectTransform> _slots = new List<RectTransform>();
    private int _count;
    private float _step;
    private float _angle;
    private float _idleFactor = 1f;
    private int _lastSegment = -1;

    public bool IsSpinning { get; private set; }
    public Vector3 SlotPosition(int index) => _slots[index].position;
    
    private void OnValidate()
    {
        if (_disc == null) _disc = AutoRef.Child<RectTransform>(this, "ui_wheel_disc");
        if (_slotTemplate == null) _slotTemplate = AutoRef.Child<RectTransform>(this, "ui_wheel_slot");
        if (_pointer == null) _pointer = AutoRef.Child<RectTransform>(this, "ui_image_wheel_pointer");
    }

    private void OnEnable()
    {
        _angle = _disc.localEulerAngles.z;
        _lastSegment = SegmentAt(_angle);
    }

    private void OnDisable()
    {
        DOTween.Kill(this);
        _pointer.DOKill();
        _pointer.localRotation = Quaternion.identity;
        IsSpinning = false;
        _idleFactor = 1f;
    }

    private void Update()
    {
        if (IsSpinning || _count == 0) return;

        _angle = Mathf.Repeat(_angle - _idleSpeed * _idleFactor * Time.deltaTime, 360f);
        ApplyAngle();
    }

    private void LateUpdate()
    {
        int segment = SegmentAt(_angle);
        if (segment != _lastSegment)
        {
            if (_lastSegment >= 0) Tick();
            _lastSegment = segment;
        }
    }

    public void Build(WheelConfig.Segment[] segments)
    {
        _slotTemplate.gameObject.SetActive(false);
        _count = segments.Length;
        _step = 360f / _count;

        for (int i = 0; i < _count; i++)
        {
            RectTransform slot = Instantiate(_slotTemplate, _disc);
            _slots.Add(slot);
            slot.gameObject.SetActive(true);

            float angle = i * _step + _step * 0.5f;
            float rad = angle * Mathf.Deg2Rad;
            slot.anchoredPosition = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * _iconRadius;
            slot.localRotation = Quaternion.Euler(0f, 0f, -angle);

            slot.GetComponent<Image>().sprite = segments[i].Icon;
            slot.GetComponentInChildren<TMP_Text>().text = segments[i].AmountLabel;
        }

        _lastSegment = SegmentAt(_angle);
    }

    public void SpinTo(int index, Action onComplete)
    {
        DOTween.Kill(this);
        IsSpinning = true;

        float jitter = UnityEngine.Random.Range(-_landingJitter, _landingJitter) * _step;
        float target = index * _step + _step * 0.5f + jitter;
        float distance = Mathf.Repeat(_angle - target, 360f) + 360f * _fullTurns;

        float t1 = _accelDuration;
        float t2 = _decelDuration;
        float decelDistance = distance / (1f + 2f * t1 / t2);
        float accelShare = (distance - decelDistance) / distance;

        DOTween.To(() => _angle, SetAngle, _angle - distance, t1 + t2)
            .SetTarget(this)
            .SetEase((time, duration, amplitude, period) =>
            {
                if (time < t1)
                {
                    float u = time / t1;
                    return accelShare * u * u;
                }
                float v = 1f - (time - t1) / t2;
                return accelShare + (1f - accelShare) * (1f - v * v * v * v);
            })
            .OnComplete(() =>
            {
                IsSpinning = false;
                _idleFactor = 0f;
                _angle = Mathf.Repeat(_angle, 360f);
                onComplete?.Invoke();
            });
    }
    
    public void ResumeIdle()
    {
        DOTween.To(() => _idleFactor, v => _idleFactor = v, 1f, _idleResumeDuration)
            .SetEase(Ease.InOutSine)
            .SetTarget(this);
    }
    
    private void SetAngle(float value)
    {
        _angle = value;
        ApplyAngle();
    }
    
    private void ApplyAngle()
    {
        _disc.localRotation = Quaternion.Euler(0f, 0f, _angle);
    }

    private int SegmentAt(float angle)
    {
        if (_count == 0) return -1;
        return Mathf.FloorToInt(Mathf.Repeat(angle, 360f) / _step) % _count;
    }

    private void Tick()
    {
        _pointer.DOKill();
        _pointer.localRotation = Quaternion.Euler(0f, 0f, _pointerKick);
        _pointer.DOLocalRotate(Vector3.zero, _pointerReturn).SetEase(Ease.OutBack);
        AudioManager.Play(SoundId.WheelTick);
    }
}
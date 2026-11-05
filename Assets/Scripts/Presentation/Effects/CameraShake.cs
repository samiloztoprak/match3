using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float _maxOffset = 0.25f;
    [SerializeField] private float _decayPerSecond = 2.2f;

    private float _trauma;
    private Vector3 _basePosition;

    void Start()
    {
        _basePosition = transform.localPosition;
    } 
    
    public void Add(float amount)
    { 
        _trauma = Mathf.Max(_trauma, Mathf.Clamp01(amount));
    }

    void LateUpdate()
    {
        if (_trauma <= 0f)
        {
            transform.localPosition = _basePosition;
            return;
        }

        float strength = _trauma * _trauma;
        float n = Time.time * 30f;
        Vector3 offset = new Vector3(Mathf.PerlinNoise(n, 0f) * 2f - 1f, Mathf.PerlinNoise(0f, n) * 2f - 1f, 0f) * (_maxOffset * strength);
        transform.localPosition = _basePosition + offset;
        _trauma = Mathf.Max(0f, _trauma - _decayPerSecond * Time.deltaTime);
    }
}
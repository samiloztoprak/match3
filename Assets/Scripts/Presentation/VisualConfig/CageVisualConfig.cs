using UnityEngine;

[CreateAssetMenu(fileName = "CageVisualConfig", menuName = "Match3/CageVisualConfig")]
public class CageVisualConfig : ScriptableObject
{
    [Header("Animation")]
    [SerializeField] private Sprite[] _frames;
    [Tooltip("Health 3, 2, 1, 0 for frame indexs")]
    [SerializeField] private int[] _stopFrames = { 0, 0, 0, 0 };
    [SerializeField] private float _fps = 60f;
    [SerializeField] private float _catchUpSpeed = 2.5f;
    
    public bool HasFrames => _frames != null && _frames.Length > 0;
    public Sprite[] Frames => _frames;
    public float Fps => _fps;
    public float CatchUpSpeed => _catchUpSpeed;
    
    public int StopFrame(int health)
    {
        int index = Mathf.Clamp(3 - health, 0, _stopFrames.Length - 1);
        return Mathf.Clamp(_stopFrames[index], 0, _frames.Length - 1);
    }
    
    public Sprite GetSprite(int health)
    {
        return _frames[StopFrame(health)];
    }
}
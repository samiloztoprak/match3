using UnityEngine;

public class GoalSpark : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private TrailRenderer _trail;

    public SpriteRenderer Sprite => _sprite;
    public TrailRenderer Trail => _trail;

    private void OnDisable()
    {
        if (_trail != null)
        {
            _trail.Clear();
        }
    }
}
using UnityEngine;

[CreateAssetMenu(fileName = "BoxVisualConfig", menuName = "Match3/BoxVisualConfig")]
public class BoxVisualConfig : ScriptableObject
{
    [SerializeField]
    private Sprite[] _damageSprites;
    
    public Sprite GetSprite(int health)
    {
        int index = Mathf.Clamp(_damageSprites.Length - health, 0, _damageSprites.Length - 1);
        return _damageSprites[index];
    }
}

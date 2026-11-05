using UnityEngine;

[CreateAssetMenu(fileName = "CageVisualConfig", menuName = "Match3/CageVisualConfig")]
public class CageVisualConfig : ScriptableObject
{
    [SerializeField]
    private Sprite[] _damageSprites;

    public Sprite GetSprite(int health)
    {
        int index = Mathf.Clamp(_damageSprites.Length - health, 0, _damageSprites.Length - 1);
        return _damageSprites[index];
    }
}
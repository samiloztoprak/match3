using UnityEngine;

[CreateAssetMenu(fileName = "LevelCatalog", menuName = "Match3/LevelCatalog")]
public class LevelCatalog : ScriptableObject, ILevelSource
{
    [SerializeField] private LevelAsset[] _levelAssets;

    public int Count => _levelAssets != null ? _levelAssets.Length : 0;

    public LevelData Get(int index)
    {
        return _levelAssets[Mathf.Clamp(index, 0, _levelAssets.Length - 1)].Data;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_levelAssets == null || _levelAssets.Length == 0)
        {
            Refresh();
        }
    }

    [ContextMenu("Refresh From Data/LevelAssets")]
    private void Refresh()
    {
        _levelAssets = AutoRef.Assets<LevelAsset>("Assets/Data/LevelAssets");
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
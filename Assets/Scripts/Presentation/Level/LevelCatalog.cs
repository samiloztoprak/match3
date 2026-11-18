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
    public const string AssetFolder = "Assets/Data/LevelAssets";

    private void OnValidate()
    {
        if (_levelAssets == null || _levelAssets.Length == 0) Refresh();
    }

    [ContextMenu("Refresh From Data/LevelAssets")]
    public void Refresh()
    {
        _levelAssets = AutoRef.Assets<LevelAsset>(AssetFolder);
        UnityEditor.EditorUtility.SetDirty(this);
    }

    public int IndexOf(LevelAsset asset)
    {
        return _levelAssets != null ? System.Array.IndexOf(_levelAssets, asset) : -1;
    }
#endif
}
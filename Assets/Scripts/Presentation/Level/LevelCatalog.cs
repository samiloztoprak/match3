using UnityEngine;

[CreateAssetMenu(fileName = "LevelCatalog", menuName = "Match3/LevelCatalog")]
public class LevelCatalog : ScriptableObject
{
    [SerializeField] private LevelDefinition[] _levels;

    public int Count => _levels != null ? _levels.Length : 0;

    public LevelDefinition Get(int index)
    {
        return _levels[Mathf.Clamp(index, 0, _levels.Length - 1)];
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_levels == null || _levels.Length == 0) Refresh();
    }

    [ContextMenu("Refresh From Data/Levels")]
    private void Refresh()
    {
        _levels = AutoRef.Assets<LevelDefinition>("Assets/Data/Levels");
    }
#endif
}
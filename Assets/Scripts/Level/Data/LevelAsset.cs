using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Match3/Level Asset")]
public class LevelAsset : ScriptableObject
{
    public LevelData Data = new LevelData();
}
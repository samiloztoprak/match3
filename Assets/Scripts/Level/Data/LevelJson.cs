using UnityEngine;

public static class LevelJson
{
    public static string ToJson(LevelData data) => JsonUtility.ToJson(data, true);

    public static LevelData FromJson(string json) =>
        string.IsNullOrWhiteSpace(json) ? null : JsonUtility.FromJson<LevelData>(json);
}
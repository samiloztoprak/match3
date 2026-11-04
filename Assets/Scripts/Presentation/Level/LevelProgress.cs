using UnityEngine;

public static class LevelProgress
{
    private const string Key = "CurrentLevelIndex";

    public static int CurrentIndex
    {
        get => PlayerPrefs.GetInt(Key, 0);
        set => PlayerPrefs.SetInt(Key, value);
    }
}
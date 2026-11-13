using UnityEngine;

public static class LevelProgress
{
    private const string Key = "CurrentLevelIndex";

    public static int CurrentIndex
    {
        get => PlayerPrefs.GetInt(Key, 0);
        set => PlayerPrefs.SetInt(Key, value);
    }
    
    public static int GetStars(int levelIndex)
    {
        return PlayerPrefs.GetInt("Stars_" + levelIndex, 0);
    }

    public static void SaveStars(int levelIndex, int stars)
    {
        if (stars > GetStars(levelIndex))
        {
            PlayerPrefs.SetInt("Stars_" + levelIndex, stars);
            PlayerPrefs.Save();
        }
    }
}
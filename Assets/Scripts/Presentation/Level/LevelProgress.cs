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

    public static int SaveStars(int levelIndex, int stars)
    {
        int previous = GetStars(levelIndex);
        if (stars <= previous) 
        {
            return 0;
        }

        PlayerPrefs.SetInt("Stars_" + levelIndex, stars);
        PlayerPrefs.Save();
        return stars - previous;
    }
}
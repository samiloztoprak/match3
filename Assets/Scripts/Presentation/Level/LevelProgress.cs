using UnityEngine;

public static class LevelProgress
{
    private const string Key = "CurrentLevelIndex";
    private const int CollectionUnlockIndex = 10;

    public static bool IsCollectionUnlocked => CurrentIndex >= CollectionUnlockIndex;

    public static int CurrentIndex
    {
        get => PlayerPrefs.GetInt(Key, 0);
        set => PlayerPrefs.SetInt(Key, value);
    }
    
    public static int GetStars(int levelIndex)
    {
        foreach (var best in MetaProgress.State.LevelBests)
        {
            if (best.LevelIndex == levelIndex) return best.Stars;
        }
        return PlayerPrefs.GetInt("Stars_" + levelIndex, 0);
    }

    public static int SaveStars(int levelIndex, int stars, int starsPerSpin)
    {
        return SaveStars(levelIndex, stars, starsPerSpin, true);
    }

    public static int SaveStars(int levelIndex, int stars)
    {
        return SaveStars(levelIndex, stars, 0, false);
    }

    private static int SaveStars(int levelIndex, int stars, int starsPerSpin, bool rewardWheel)
    {
        int previous = GetStars(levelIndex);
        if (stars <= previous) return 0;

        MetaProgressState next = MetaProgress.BeginUpdate();
        var best = new MetaProgressState.LevelBest { LevelIndex = levelIndex, Stars = stars };
        int index = next.LevelBests.FindIndex(entry => entry.LevelIndex == levelIndex);
        if (index < 0) next.LevelBests.Add(best);
        else next.LevelBests[index] = best;

        int gained = stars - previous;
        if (rewardWheel) next.AddStars(gained, starsPerSpin, true);
        MetaProgress.Commit(next);
        return gained;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

internal static class MetaProgress
{
    private const string Key = "Meta_Progress_v1";
    private const string SpinsKey = "Wheel_Spins";
    private const string StarBankKey = "Wheel_StarBank";
    private const string TotalStarsKey = "Wheel_TotalStars";
    private const string PendingFlyKey = "Wheel_PendingFly";
    private static readonly BoosterType[] BoosterTypes = (BoosterType[])Enum.GetValues(typeof(BoosterType));
    private static MetaProgressState _state;

    public static MetaProgressState State => _state ??= Load();

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void SetEditorStartingSpins()
    {
        _state = Load();
        if (_state.Spins == 10) return;

        MetaProgressState next = BeginUpdate();
        next.Spins = 10;
        Commit(next);
    }
#endif

    public static MetaProgressState BeginUpdate() => State.Copy();

    public static void Commit(MetaProgressState next)
    {
        Normalize(next);
        MetaProgressState previous = State;
        Save(next);
        _state = next;
        NotifyChanges(previous, next);
    }

    private static void Save(MetaProgressState next)
    {
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(next));
        PlayerPrefs.SetInt(SpinsKey, next.Spins);
        PlayerPrefs.SetInt(StarBankKey, next.StarBank);
        PlayerPrefs.SetInt(TotalStarsKey, next.TotalStars);
        PlayerPrefs.SetInt(PendingFlyKey, next.PendingFly);
        foreach (BoosterType type in BoosterTypes)
            PlayerPrefs.SetInt("Booster_" + type, next.GetBooster(type));
        foreach (MetaProgressState.LevelBest best in next.LevelBests)
            PlayerPrefs.SetInt("Stars_" + best.LevelIndex, best.Stars);
        PlayerPrefs.Save();
    }

    private static void NotifyChanges(MetaProgressState previous, MetaProgressState next)
    {
        if (previous.Spins != next.Spins || previous.StarBank != next.StarBank
            || previous.TotalStars != next.TotalStars)
        {
            WheelProgress.NotifyChanged();
        }

        foreach (BoosterType type in BoosterTypes)
        {
            int count = next.GetBooster(type);
            if (previous.GetBooster(type) != count) BoosterInventory.NotifyChanged(type, count);
        }
    }

    private static MetaProgressState Load()
    {
        if (PlayerPrefs.HasKey(Key))
        {
            var saved = JsonUtility.FromJson<MetaProgressState>(PlayerPrefs.GetString(Key));
            if (saved == null) saved = new MetaProgressState();
            Normalize(saved);
            return saved;
        }

        var state = new MetaProgressState
        {
            Spins = PlayerPrefs.GetInt(SpinsKey, 1),
            StarBank = PlayerPrefs.GetInt(StarBankKey),
            TotalStars = PlayerPrefs.GetInt(TotalStarsKey),
            PendingFly = PlayerPrefs.GetInt(PendingFlyKey)
        };

        foreach (BoosterType type in BoosterTypes)
            state.SetBooster(type, PlayerPrefs.GetInt("Booster_" + type, 3));

        return state;
    }

    private static void Normalize(MetaProgressState state)
    {
        state.Boosters ??= new List<MetaProgressState.BoosterBalance>();
        state.OwnedCards ??= new List<string>();
        state.ClaimedSets ??= new List<string>();
        state.LevelBests ??= new List<MetaProgressState.LevelBest>();
    }
}

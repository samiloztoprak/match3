using System;
using UnityEngine;

public static class BoosterInventory
{
    private const int StartCount = 3;

    public static event Action<BoosterType, int> Changed;

    private static string Key(BoosterType type) => "Booster_" + type;

    public static int Get(BoosterType type)
    {
        return PlayerPrefs.GetInt(Key(type), StartCount);
    }

    public static void Add(BoosterType type, int amount)
    {
        Set(type, Get(type) + amount);
    }

    public static bool TryUse(BoosterType type)
    {
        int count = Get(type);
        if (count <= 0)
        {
            return false;
        }
        Set(type, count - 1);
        return true;
    }

    private static void Set(BoosterType type, int value)
    {
        PlayerPrefs.SetInt(Key(type), Mathf.Max(0, value));
        PlayerPrefs.Save();
        Changed?.Invoke(type, Get(type));
    }
}
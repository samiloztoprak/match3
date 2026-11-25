using System;
using UnityEngine;

public enum SettingType
{
    Music = 0,
    Sound = 1,
    Vibration = 2,
}

public static class GameSettings
{
    public static event Action<SettingType, bool> Changed;

    public static bool Get(SettingType type)
    {
        return PlayerPrefs.GetInt(Key(type), 1) == 1;
    }

    public static void Set(SettingType type, bool on)
    {
        PlayerPrefs.SetInt(Key(type), on ? 1 : 0);
        PlayerPrefs.Save();
        Changed?.Invoke(type, on);
    }

    public static void Toggle(SettingType type)
    {
        Set(type, !Get(type));
    }

    private static string Key(SettingType type) => "Setting_" + type;
}
using UnityEngine;

public static class Haptics
{
    public static void Vibrate()
    {
        if (!GameSettings.Get(SettingType.Vibration)) return;
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }
}
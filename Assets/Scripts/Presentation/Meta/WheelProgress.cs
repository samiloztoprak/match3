using System;
using UnityEngine;

public static class WheelProgress
{
    private const string BankKey = "Wheel_StarBank";
    private const string SpinsKey = "Wheel_Spins";
    private const string TotalKey = "Wheel_TotalStars";
    private const string PendingFlyKey = "Wheel_PendingFly";
    private const int StartSpins = 1;

    public static event Action Changed;

    public static int StarBank => PlayerPrefs.GetInt(BankKey, 0);
    public static int Spins => PlayerPrefs.GetInt(SpinsKey, StartSpins);
    public static int TotalStars => PlayerPrefs.GetInt(TotalKey, 0);

    static WheelProgress()
    {
        PlayerPrefs.SetInt(SpinsKey, 2);
    }
    
    public static void AddStars(int stars, int starsPerSpin, bool queueMenuFly = true)
    {
        Debug.Log($"Adding stars for {stars} stars {BankKey}");

        if (stars <= 0) return;
        
        if (queueMenuFly)
        {
            PlayerPrefs.SetInt(PendingFlyKey, PlayerPrefs.GetInt(PendingFlyKey, 0) + stars);
        }
        var (bank, gained) = WheelLogic.AddStars(StarBank, stars, starsPerSpin);
        Debug.Log($"Adding stars for {stars} stars {BankKey}");

        PlayerPrefs.SetInt(BankKey, bank);
        PlayerPrefs.SetInt(SpinsKey, Spins + gained);
        PlayerPrefs.SetInt(TotalKey, TotalStars + stars);
        Debug.Log($"Adding stars for {stars} stars {BankKey}");

        PlayerPrefs.Save();
        Changed?.Invoke();
    }

    public static bool TryUseSpin()
    {
        int spins = Spins;
        if (spins <= 0) return false;

        PlayerPrefs.SetInt(SpinsKey, spins - 1);
        PlayerPrefs.Save();
        Changed?.Invoke();
        return true;
    }
    
    public static int ConsumePendingFly()
    {
        int stars = PlayerPrefs.GetInt(PendingFlyKey, 0);
        if (stars > 0)
        {
            PlayerPrefs.DeleteKey(PendingFlyKey);
            PlayerPrefs.Save();
        }
        return stars;
    }
}
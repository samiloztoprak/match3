using System;

public static class ActivationTiming
{
    public const float TapActivationMs = 40f;
    public const float RocketChargeMs = 250f;
    public const float RocketMsPerCell = 55f;
    
    public const float TntDelayMs = 10f;
    public const float TntRingMs = 180f;
    public const float TntChargeMs = 250f;
    
    public const float ChainDelayMs = 50f;
    public const float PostActivationMatchDelayMs = 100f;

    public const float PropellerFlightMs = 1000f;

    public const float LightBallTotalMs = 1100f;
    public const float LightBallStartMs = 250f;
    public const float LightBallStepMs = 60f;
    public const float LightBallPulseMs = 120f;
    public const float LightBallWaveMsPerRing = 200f;
    public const float LightBallComboChargeMs = 1800f; 
    
    public const float ComboMergeMs = 150f;
    
    public const float HammerStrikeMs = 250f;
    public const float BoosterLaunchMs = 120f;
    
    public const float SpecialSpawnMs = 160f;
    public const float LightBallPulseAtMs = LightBallTotalMs - LightBallPulseMs;

    
    public static int ChebyshevDistance(int x1, int y1, int x2, int y2)
    {
        return Math.Max(Math.Abs(x1 - x2), Math.Abs(y1 - y2));
    }
    
    public static float LightBallRevealMs(int index, int count)
    {
        float window = LightBallPulseAtMs - LightBallStartMs - SpecialSpawnMs;
        float step = count > 0 ? Math.Min(LightBallStepMs, window / count) : LightBallStepMs;
        return LightBallStartMs + step * index;
    }
}

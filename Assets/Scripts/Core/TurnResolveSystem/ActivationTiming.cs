using System;

public static class ActivationTiming
{
    public const float TapActivationMs = 40f;
    public const float RocketMsPerCell = 55f;
    public const float TntDelayMs = 180f;
    public const float TntRingMs = 70f;
    public const float ChainDelayMs = 50f;

    public const float PropellerFlightMs = 450f;

    public const float LightBallStartMs = 250f;
    public const float LightBallStepMs = 60f;
    public const float LightBallPulseMs = 120f;
    public const float LightBallWaveMsPerRing = 60f;

    public const float ComboMergeMs = 150f;
    
    public const float HammerStrikeMs = 250f;
    public const float BoosterLaunchMs = 120f;

    public static int ChebyshevDistance(int x1, int y1, int x2, int y2)
    {
        return Math.Max(Math.Abs(x1 - x2), Math.Abs(y1 - y2));
    }
}

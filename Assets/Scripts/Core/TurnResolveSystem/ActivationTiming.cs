using System;

public static class ActivationTiming
{
    public const float SwapActivationMs = 140f;
    public const float TapActivationMs = 60f;
    public const float RocketMsPerCell = 34f;
    public const float TntDelayMs = 50f;
    public const float TntRingMs = 22f;
    public const float ChainDelayMs = 70f;

    public static int ChebyshevDistance(int x1, int y1, int x2, int y2)
    {
        return Math.Max(Math.Abs(x1 - x2), Math.Abs(y1 - y2));
    }
}
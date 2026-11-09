using System.Collections.Generic;
using UnityEngine;

public class LightBallLightBallComboPresenter : IActivationPresenter
{
    public float Present(SpecialActivationData data, float t, TurnPlayer player, List<(float, IViewCommand)> timeline)
    {
        float pulseDuration = ActivationTiming.LightBallPulseMs / 1000f;
        timeline.Add((t, new ShakeViewCommand(player, 0.8f)));
        
        int maxRing = 0;
        foreach (int cx in new[] { 0, player.BoardWidth - 1 })
        {
            foreach (int cy in new[] { 0, player.BoardHeight - 1 })
            {
                int d = ActivationTiming.ChebyshevDistance(data.X, data.Y, cx, cy);
                if (d > maxRing) maxRing = d;
            }
        }
        float waveDuration = ActivationTiming.LightBallWaveMsPerRing * maxRing / 1000f;

        return t + Mathf.Max(pulseDuration, waveDuration);
    }
}
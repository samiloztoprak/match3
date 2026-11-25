using System.Collections.Generic;
using UnityEngine;

public class LightBallComboPresenter : IActivationPresenter
{
        
    private const float RevealWindowMs = 900f;
    private readonly IActivationPresenter _fallback = new FlashActivationPresenter();
    
    public float Present(SpecialActivationData data, float t, TurnPlayer player,
        List<(float, IViewCommand)> timeline)
    {
        if (data.Targets == null || data.Targets.Count == 0)
        {
            return _fallback.Present(data, t, player, timeline);
        }

        var activeBeams = new List<LightBeamView>();
        Vector3 origin = player.CellToWorld(data.X, data.Y);

        int count = data.Targets.Count;
        float available = Mathf.Max(0f, RevealWindowMs - ActivationTiming.LightBallStartMs);
        float stepMs = Mathf.Min(ActivationTiming.LightBallStepMs, available / count);

        for (int i = 0; i < count; i++)
        {
            var (x, y, _) = data.Targets[i];
            Vector3 targetPos = player.CellToWorld(x, y);
            float beamTime = t + ActivationTiming.LightBallRevealMs(i, count) / 1000f;
            timeline.Add((beamTime, new LightBeamStartViewCommand(origin, targetPos, player, activeBeams)));
        }

        float revealEnd = t + ActivationTiming.LightBallPulseAtMs / 1000f;
        float pulseDuration = ActivationTiming.LightBallPulseMs / 1000f;

        timeline.Add((revealEnd, new LightBallPulseViewCommand(data.PieceId, player, pulseDuration)));
        timeline.Add((revealEnd + pulseDuration, new LightBeamStopViewCommand(player, activeBeams)));

        return revealEnd + pulseDuration;
    }
}

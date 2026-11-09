using System.Collections.Generic;
using UnityEngine;

public class LightBallPresenter : IActivationPresenter
{
    private readonly IActivationPresenter _fallback = new FlashActivationPresenter();

    public float Present(SpecialActivationData data, float t, TurnPlayer player, List<(float, IViewCommand)> timeline)
    {
        if (data.Targets == null || data.Targets.Count == 0)
        {
            return _fallback.Present(data, t, player, timeline);
        }

        var activeBeams = new List<LightBeamView>();
        Vector3 origin = player.CellToWorld(data.X, data.Y);

        for (int i = 0; i < data.Targets.Count; i++)
        {
            var (x, y, _) = data.Targets[i];
            Vector3 targetPos = player.CellToWorld(x, y);
            float beamTime = t + (ActivationTiming.LightBallStartMs + ActivationTiming.LightBallStepMs * i) / 1000f;
            timeline.Add((beamTime, new LightBeamStartViewCommand(origin, targetPos, player, activeBeams)));
        }

        float revealMs = ActivationTiming.LightBallStartMs + ActivationTiming.LightBallStepMs * data.Targets.Count;
        float revealEnd = t + revealMs / 1000f;
        float pulseDuration = ActivationTiming.LightBallPulseMs / 1000f;

        timeline.Add((revealEnd, new LightBallPulseViewCommand(data.PieceId, player, pulseDuration)));
        timeline.Add((revealEnd + pulseDuration, new LightBeamStopViewCommand(player, activeBeams)));

        return revealEnd + pulseDuration;
    }
}

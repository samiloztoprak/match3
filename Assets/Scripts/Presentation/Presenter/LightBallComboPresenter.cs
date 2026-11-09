using System.Collections.Generic;
using UnityEngine;

public class LightBallComboPresenter : IActivationPresenter
{
    private static float TransformDuration => ActivationTiming.LightBallPulseMs / 1000f;
        
    public float Present(SpecialActivationData data, float t, TurnPlayer player,
        List<(float, IViewCommand)> timeline)
    {
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

        timeline.Add((revealEnd, new LightBeamStopViewCommand(player, activeBeams)));
        
        float fireStart = revealEnd + TransformDuration;
        IActivationPresenter firePresenter = new FlashActivationPresenter();
        float fireEnd = firePresenter.Present(data, fireStart, player, timeline);
        
        return fireEnd;
    }
}

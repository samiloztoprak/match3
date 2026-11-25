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

        var targets = new List<Vector3>(data.Targets.Count);
        foreach (var (x, y, _) in data.Targets)
        {
            targets.Add(player.CellToWorld(x, y));
        }

        Vector3 origin = player.CellToWorld(data.X, data.Y);
        var reveal = new LightBallRevealViewCommand(data.PieceId, player, origin, targets);

        timeline.Add((t, new LightBallActivationViewCommand(player, origin, reveal.Duration)));
        timeline.Add((t, reveal));

        return t + reveal.Duration;
    }
}

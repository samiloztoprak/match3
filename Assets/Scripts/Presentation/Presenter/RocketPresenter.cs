using System.Collections.Generic;
using UnityEngine;

public class RocketPresenter : IActivationPresenter
{
    private readonly IActivationPresenter _fallback = new FlashActivationPresenter();

    public float Present(SpecialActivationData data, float t, TurnPlayer player, List<(float, IViewCommand)> timeline)
    {
        if (!data.Direction.HasValue)
        {
            return _fallback.Present(data, t, player, timeline);
        }
        var fly = new RocketFlyViewCommand(data.Direction.Value, data.X, data.Y, player);
        timeline.Add((t, fly));
        return t + fly.Duration;
    }
}

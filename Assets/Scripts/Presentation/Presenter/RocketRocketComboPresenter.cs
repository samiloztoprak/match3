using System.Collections.Generic;
using UnityEngine;

public class RocketRocketComboPresenter : IActivationPresenter
{
    public float Present(SpecialActivationData data, float t, TurnPlayer player, List<(float, IViewCommand)> timeline)
    {
        var flyH = new RocketFlyViewCommand(RocketDirection.Horizontal, data.X, data.Y, player);
        var flyV = new RocketFlyViewCommand(RocketDirection.Vertical, data.X, data.Y, player);
        timeline.Add((t, flyH));
        timeline.Add((t, flyV));
        return t + Mathf.Max(flyH.Duration, flyV.Duration);
    }
}
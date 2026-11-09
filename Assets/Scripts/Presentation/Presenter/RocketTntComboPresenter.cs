using System.Collections.Generic;
using UnityEngine;

public class RocketTntComboPresenter : IActivationPresenter
{
    public float Present(SpecialActivationData data, float t, TurnPlayer player, List<(float, IViewCommand)> timeline)
    {
        int thickness = ActivationArea.RocketTntThickness;
        float maxDuration = 0f;

        for (int oy = -thickness; oy <= thickness; oy++)
        {
            int ry = data.Y + oy;
            if (ry < 0 || ry >= player.BoardHeight) continue;
            var fly = new RocketFlyViewCommand(RocketDirection.Horizontal, data.X, ry, player);
            timeline.Add((t, fly));
            maxDuration = Mathf.Max(maxDuration, fly.Duration);
        }
        for (int ox = -thickness; ox <= thickness; ox++)
        {
            int rx = data.X + ox;
            if (rx < 0 || rx >= player.BoardWidth) continue;
            var fly = new RocketFlyViewCommand(RocketDirection.Vertical, rx, data.Y, player);
            timeline.Add((t, fly));
            maxDuration = Mathf.Max(maxDuration, fly.Duration);
        }

        timeline.Add((t, new ShakeViewCommand(player, 0.4f)));
        return t + maxDuration;
    }
}
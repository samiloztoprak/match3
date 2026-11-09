using System.Collections.Generic;
using UnityEngine;

public class PropellerPropellerComboPresenter : IActivationPresenter
{
    public float Present(SpecialActivationData data, float t, TurnPlayer player, List<(float, IViewCommand)> timeline)
    {
        Vector3 origin = player.CellToWorld(data.X, data.Y);
        float flight = ActivationTiming.PropellerFlightMs / 1000f;
        Sprite sprite = data.Piece != null ? player.VisualConfig.GetSprite(data.Piece) : null;
        var config = player.PropellerConfig;

        float maxEnd = t;
        foreach (var (x, y, _) in data.Targets)
        {
            Vector3 targetPos = player.CellToWorld(x, y);
            timeline.Add((t, new PropellerComboFlyViewCommand(sprite, origin, targetPos, player, flight)));
            timeline.Add((t + flight, new AreaDamageViewCommand(
                new List<(int x, int y)> { (x, y) }, player, config.ImpactFlashDuration)));
            maxEnd = Mathf.Max(maxEnd, t + flight);
        }

        return maxEnd;
    }
}
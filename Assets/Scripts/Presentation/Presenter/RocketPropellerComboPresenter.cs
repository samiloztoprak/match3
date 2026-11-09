using System.Collections.Generic;
using UnityEngine;

public class RocketPropellerComboPresenter : IActivationPresenter
{
    public float Present(SpecialActivationData data, float t, TurnPlayer player, List<(float, IViewCommand)> timeline)
    {
        Vector3 origin = player.CellToWorld(data.X, data.Y);
        float flight = ActivationTiming.PropellerFlightMs / 1000f;
        var (tx, ty, _) = data.Targets[0];
        Vector3 targetPos = player.CellToWorld(tx, ty);
        Sprite sprite = player.VisualConfig.GetSprite(new SpecialPiece(0, PieceKind.Propeller));

        timeline.Add((t, new PropellerComboFlyViewCommand(sprite, origin, targetPos, player, flight)));

        RocketDirection direction = data.Direction ?? RocketDirection.Horizontal;
        var fly = new RocketFlyViewCommand(direction, tx, ty, player);
        timeline.Add((t + flight, fly));

        return t + flight + fly.Duration;
    }
}
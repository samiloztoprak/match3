using System.Collections.Generic;
using UnityEngine;

public class TntPropellerComboPresenter : IActivationPresenter
{
    public float Present(SpecialActivationData data, float t, TurnPlayer player, List<(float, IViewCommand)> timeline)
    {
        Vector3 origin = player.CellToWorld(data.X, data.Y);
        float flight = ActivationTiming.PropellerFlightMs / 1000f;
        var (tx, ty, _) = data.Targets[0];
        Vector3 targetPos = player.CellToWorld(tx, ty);
        Sprite sprite = player.VisualConfig.GetSprite(new SpecialPiece(0, PieceKind.Propeller));

        timeline.Add((t, new PropellerComboFlyViewCommand(sprite, origin, targetPos, player, flight)));

        float radius = ActivationArea.TntPropellerRadius;
        float ring = ActivationTiming.TntRingMs * radius / 1000f;
        timeline.Add((t + flight, new TntExplosionViewCommand(tx, ty, player, ring, radius)));
        timeline.Add((t + flight, new ShakeViewCommand(player, 0.5f)));

        return t + flight + ring;
    }
}
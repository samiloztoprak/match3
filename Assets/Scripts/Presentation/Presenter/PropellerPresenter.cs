using System.Collections.Generic;
using UnityEngine;

public class PropellerPresenter : IActivationPresenter
{
    private readonly IActivationPresenter _fallback = new FlashActivationPresenter();

    public float Present(SpecialActivationData data, float t, TurnPlayer player,
        List<(float, IViewCommand)> timeline)
    {
        if (!data.TargetX.HasValue || !data.TargetY.HasValue)
        {
            return _fallback.Present(data, t, player, timeline);
        }

        float flight = ActivationTiming.PropellerFlightMs / 1000f;
        Vector3 targetPos = player.CellToWorld(data.TargetX.Value, data.TargetY.Value);
        var config = player.PropellerConfig;

        if (data.NeighborCells != null && data.NeighborCells.Count > 0)
        {
            timeline.Add((t, new AreaDamageViewCommand(data.NeighborCells, player, 0f)));
        }
        timeline.Add((t, new PropellerFlyViewCommand(data.PieceId, targetPos, player, config, flight)));
        timeline.Add((t + flight, new AreaDamageViewCommand(
            new List<(int x, int y)> { (data.TargetX.Value, data.TargetY.Value) },
            player, config.ImpactFlashDuration)));

        return t + flight;
    }
}

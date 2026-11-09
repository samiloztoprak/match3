using System.Collections.Generic;

public class TntTntComboPresenter : IActivationPresenter
{
    private static float Radius => ActivationArea.TntComboRadius;

    public float Present(SpecialActivationData data, float t, TurnPlayer player, List<(float, IViewCommand)> timeline)
    {
        float delay = ActivationTiming.TntDelayMs / 1000f;
        float ring  = ActivationTiming.TntRingMs * Radius / 1000f;

        timeline.Add((t, new TntChargeViewCommand(data.PieceId, player, delay)));
        timeline.Add((t + delay, new TntExplosionViewCommand(data.X, data.Y, player, ring, Radius)));
        timeline.Add((t + delay, new ShakeViewCommand(player, 1f)));

        return t + delay + ring;
    }
}
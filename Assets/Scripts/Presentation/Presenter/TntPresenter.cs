using System.Collections.Generic;

public sealed class TntPresenter : IActivationPresenter
{
    private readonly IActivationPresenter _fallback = new FlashActivationPresenter();
    
    public float Present(SpecialActivatedData data, float t, TurnPlayer player,
        List<(float, IViewCommand)> timeline)
    {
        if (data.IsCombo)
        {
            return _fallback.Present(data, t, player, timeline);
        }
        
        float delay = ActivationTiming.TntDelayMs / 1000f;
        float ring  = ActivationTiming.TntRingMs / 1000f;

        timeline.Add((t, new TntChargeViewCommand(data.PieceId, player, delay)));
        timeline.Add((t + delay, new TntExplosionViewCommand(data.X, data.Y, player, ring)));
        timeline.Add((t + delay, new ShakeViewCommand(player, 0.6f)));

        return t + delay + ring;
    }
}
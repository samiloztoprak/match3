using System.Collections.Generic;

public sealed class FlashActivationPresenter : IActivationPresenter
{
    public float Present(SpecialActivatedData data, float t, TurnPlayer player,
        List<(float, IViewCommand)> timeline)
    {
        timeline.Add((t, new SpecialActivationFlashViewCommand(data.PieceId, data.X, data.Y, player, 0f)));
        return t;
    }
}
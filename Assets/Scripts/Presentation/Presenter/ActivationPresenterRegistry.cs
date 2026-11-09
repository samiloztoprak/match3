using System.Collections.Generic;

public static class ActivationPresenterRegistry
{
    private static readonly IActivationPresenter Fallback = new FlashActivationPresenter();

    private static readonly Dictionary<PieceKind, IActivationPresenter> _presenters =
        new Dictionary<PieceKind, IActivationPresenter>
        {
            { PieceKind.Rocket, new RocketPresenter() },
            { PieceKind.Propeller, new PropellerPresenter() },
            { PieceKind.Tnt, new TntPresenter() },
            { PieceKind.LightBall, new LightBallPresenter() },
        };

    public static IActivationPresenter Get(PieceKind kind)
    {
        return _presenters.TryGetValue(kind, out var presenter) ? presenter : Fallback;
    }

    public static IActivationPresenter Get(SpecialActivationData data)
    {
        if (data.IsCombo)
            return ComboPresenterRegistry.Get(data.Kind, data.ComboPartnerKind ?? data.Kind);

        return Get(data.Kind);
    }
}

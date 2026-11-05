using System.Collections.Generic;

public static class ActivationPresenterRegistry
{
    private static readonly IActivationPresenter Fallback = new FlashActivationPresenter();

    private static readonly Dictionary<PieceKind, IActivationPresenter> _presenters =
        new Dictionary<PieceKind, IActivationPresenter>
        {
            { PieceKind.Rocket, new RocketPresenter() },
            { PieceKind.Propeller, new PropellerPresenter() },
            { PieceKind.Tnt, new TntPresenter() }
        };

    public static IActivationPresenter Get(PieceKind kind)
    {
        return _presenters.TryGetValue(kind, out var presenter) ? presenter : Fallback;
    }
}

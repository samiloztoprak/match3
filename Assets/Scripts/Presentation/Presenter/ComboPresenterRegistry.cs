using System.Collections.Generic;

public static class ComboPresenterRegistry
{
    private static readonly IActivationPresenter Flash = new FlashActivationPresenter();
    private static readonly IActivationPresenter LightBallCombo = new LightBallComboPresenter();
    
    private static readonly Dictionary<(PieceKind, PieceKind), IActivationPresenter> _presenters =
        new Dictionary<(PieceKind, PieceKind), IActivationPresenter>
        {
            { Key(PieceKind.Rocket, PieceKind.Rocket), new RocketRocketComboPresenter() },
            { Key(PieceKind.Rocket, PieceKind.Tnt), new RocketTntComboPresenter() },
            { Key(PieceKind.Rocket, PieceKind.Propeller), new RocketPropellerComboPresenter() },
            { Key(PieceKind.Tnt, PieceKind.Tnt), new TntTntComboPresenter() },
            { Key(PieceKind.Tnt, PieceKind.Propeller), new TntPropellerComboPresenter() },
            { Key(PieceKind.Propeller, PieceKind.Propeller), new PropellerPropellerComboPresenter() },
            { Key(PieceKind.LightBall, PieceKind.Rocket), LightBallCombo },
            { Key(PieceKind.LightBall, PieceKind.Tnt), LightBallCombo },
            { Key(PieceKind.LightBall, PieceKind.Propeller), LightBallCombo },
            { Key(PieceKind.LightBall, PieceKind.LightBall), new LightBallLightBallComboPresenter() },
        };

    private static (PieceKind, PieceKind) Key(PieceKind a, PieceKind b)
    {
        return a <= b ? (a, b) : (b, a);
    }

    public static IActivationPresenter Get(PieceKind a, PieceKind b)
    {
        _presenters.TryGetValue(Key(a, b), out var presenter);
        return presenter ?? Flash;
    }
}
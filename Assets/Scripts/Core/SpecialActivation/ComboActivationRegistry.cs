using System.Collections.Generic;

public static class ComboActivationRegistry
{
    private static readonly LightBallDefaultCombo _lightBallDefault = new LightBallDefaultCombo();

    private static readonly Dictionary<(PieceKind, PieceKind), IComboActivation> _combos = new Dictionary<(PieceKind, PieceKind), IComboActivation>
    {
        { Key(PieceKind.Rocket, PieceKind.Rocket), new RocketRocketCombo() },
        { Key(PieceKind.Rocket, PieceKind.Tnt), new RocketTntCombo() },
        { Key(PieceKind.Tnt, PieceKind.Tnt), new TntTntCombo() },
        { Key(PieceKind.Rocket, PieceKind.Propeller), new RocketPropellerCombo() },
        { Key(PieceKind.Tnt, PieceKind.Propeller), new TntPropellerCombo() },
        { Key(PieceKind.Propeller, PieceKind.Propeller), new PropellerPropellerCombo() },
        { Key(PieceKind.LightBall, PieceKind.LightBall), new LightBallLightBallCombo() },
        { Key(PieceKind.LightBall, PieceKind.Rocket), _lightBallDefault },
        { Key(PieceKind.LightBall, PieceKind.Tnt), _lightBallDefault },
        { Key(PieceKind.LightBall, PieceKind.Propeller), _lightBallDefault },
    };

    private static (PieceKind, PieceKind) Key(PieceKind a, PieceKind b)
    {
        return a <= b ? (a, b) : (b, a);
    }

    public static IComboActivation Get(PieceKind a, PieceKind b)
    {
        _combos.TryGetValue(Key(a, b), out var combo);
        return combo;
    }
}
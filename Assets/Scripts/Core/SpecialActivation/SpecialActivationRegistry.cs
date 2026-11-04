using System.Collections.Generic;

public static class SpecialActivationRegistry
{
    private static readonly Dictionary<PieceKind, ISpecialActivation> _activations = new Dictionary<PieceKind, ISpecialActivation>
    {
        { PieceKind.Rocket, new RocketActivation() },
        { PieceKind.Tnt, new TntActivation() },
        { PieceKind.Propeller, new PropellerActivation() },
        { PieceKind.LightBall, new LightBallActivation() },
    };

    public static ISpecialActivation Get(PieceKind kind)
    {
        _activations.TryGetValue(kind, out var activation);
        return activation;
    }
}
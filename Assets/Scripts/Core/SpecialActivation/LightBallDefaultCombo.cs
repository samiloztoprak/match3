using LogicLibrary;

public sealed class LightBallDefaultCombo : IComboActivation
{
    public ComboEffect GetEffect(Board board, PieceKind kindA, PieceKind kindB, int x, int y, RocketDirection? rocketDirection, IRandom random, ITargetPriority targetPriority)
    {
        PieceKind otherKind = kindA == PieceKind.LightBall ? kindB : kindA;
        var transformed = SpecialActivationLogic.TransformMostCommonColorToKind(board, otherKind, random);
        return new ComboEffect { Transformed = transformed };
    }
}
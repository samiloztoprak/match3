
public interface ISpecialActivation
{
    PieceKind Kind { get; }
    ActivationEffect GetAffectedCells(Board board, Piece piece, int x, int y, Piece partner, IRandom random, ITargetPriority targetPriority);
}
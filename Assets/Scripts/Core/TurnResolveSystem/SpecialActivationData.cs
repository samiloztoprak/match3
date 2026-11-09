
using System.Collections.Generic;

public sealed class SpecialActivationData : TurnStepData
{
    public Piece Piece;
    public int PieceId;
    public PieceKind Kind;
    public int X;
    public int Y;
    public int? TargetX;
    public int? TargetY;
    public List<(int x, int y)> NeighborCells;
    public List<(int x, int y, float timeMs)> Targets;
    
    public float TimeMs;
    public RocketDirection? Direction;
    public bool IsCombo; 
    
    public PieceKind? ComboPartnerKind;
}

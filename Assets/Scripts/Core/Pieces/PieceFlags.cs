using System;

[Flags]
public enum PieceFlags
{
    None = 0,
    Movable = 1 << 0,
    CanMatch = 1 << 1,
    DamageableByNeighbor = 1 << 2
}

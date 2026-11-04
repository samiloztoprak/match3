using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ObstaclePlacement
{
    public ObstacleType Type;
    public List<Vector2Int> Cells;
}
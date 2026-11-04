using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Match3/Level Definition")]
public class LevelDefinition : ScriptableObject
{
    public int Width = 7;
    public int Height = 7;
    public int MoveLimit = 20;

    public List<LevelGoal> Goals = new List<LevelGoal>();
    public List<ObstaclePlacement> Obstacles = new List<ObstaclePlacement>();
}
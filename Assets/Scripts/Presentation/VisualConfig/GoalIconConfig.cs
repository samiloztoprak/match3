using System;
using System.Collections.Generic;
using UnityEngine;

public enum GoalFlyStyle
{
    Icon,
    Spark
}

[CreateAssetMenu(fileName = "GoalIconConfig", menuName = "Match3/GoalIconConfig")]
public class GoalIconConfig : ScriptableObject
{
    [Serializable]
    private struct Entry
    {
        public GoalType Type;
        public PieceColor Color;
        public Sprite Icon;
        public GoalFlyStyle FlyStyle;
        public Color SparkColor;
    }

    [SerializeField] private List<Entry> _entries = new List<Entry>();

    public bool TryGet(GoalType type, PieceColor color, out Sprite icon, out GoalFlyStyle style, out Color sparkColor)
    {
        foreach (Entry entry in _entries)
        {
            if (entry.Type != type)
            {
                continue;
            }
            if (type == GoalType.CollectColor && entry.Color != color)
            {
                continue;
            }

            icon = entry.Icon;
            style = entry.FlyStyle;
            sparkColor = entry.SparkColor;
            return true;
        }

        icon = null;
        style = GoalFlyStyle.Icon;
        sparkColor = Color.white;
        return false;
    }
}
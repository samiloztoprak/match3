using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PieceVisualConfig", menuName = "Scriptable Objects/PieceViewConfig")]
public class PieceVisualConfig : ScriptableObject
{
    [Serializable]
    private struct Entry
    {
        public PieceType Type;
        public Sprite Sprite;
    }
    
    [SerializeField]
    private List<Entry> _entries;

    public Sprite GetSprite(PieceType type)
    {
        foreach (var entry in _entries)
        {
            if (entry.Type == type)
            {
                return entry.Sprite;
            }
        }
        return null;
    }
}

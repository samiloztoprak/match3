using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BreakEffectConfig", menuName = "Match3/BreakEffectConfig")]
public class BreakEffectConfig : ScriptableObject
{
    [Serializable]
    private struct TypeEntry
    {
        public PieceVisualType Type;
        public ParticleSystem Hit;
        public ParticleSystem Break;
    }

    [Serializable]
    private struct ColorEntry
    {
        public PieceColor Color;
        public ParticleSystem Break;
    }

    [SerializeField] private ParticleSystem _default;
    [SerializeField] private List<TypeEntry> _types;
    [SerializeField] private List<ColorEntry> _colors;

    public ParticleSystem Default => _default;

    public ParticleSystem GetBreak(Piece piece)
    {
        if (piece == null) return _default;

        if (piece.VisualType == PieceVisualType.Standard)
        {
            foreach (var entry in _colors)
            {
                if (entry.Color == piece.Color && entry.Break != null) return entry.Break;
            }
        }

        foreach (var entry in _types)
        {
            if (entry.Type == piece.VisualType && entry.Break != null) return entry.Break;
        }
        return _default;
    }

    public ParticleSystem GetHit(Piece piece)
    {
        if (piece == null) return _default;

        foreach (var entry in _types)
        {
            if (entry.Type != piece.VisualType) continue;
            if (entry.Hit != null) return entry.Hit;
            if (entry.Break != null) return entry.Break;
        }
        return _default;
    }
}
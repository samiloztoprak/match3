using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MascotConfig", menuName = "Match3/MascotConfig")]
public class MascotConfig : ScriptableObject
{
    [Serializable]
    private struct Entry
    {
        public MascotMood Mood;
        public Sprite Sprite;
    }

    [SerializeField] private List<Entry> _sprites = new List<Entry>();

    [Header("Timing")]
    public float ReactionHold = 1.6f;
    public float IdleVariationMin = 8f;
    public float IdleVariationMax = 15f;
    public float IdleVariationHold = 2.5f;

    [Header("Thresholds")]
    public int WorriedMovesThreshold = 3;
    public int SadMovesThreshold = 1;
    public int GoodTurnClearCount = 10;
    public int HugeTurnClearCount = 25;

    public Sprite Get(MascotMood mood)
    {
        foreach (Entry entry in _sprites)
        {
            if (entry.Mood == mood)
            {
                return entry.Sprite;
            }
        }
        return null;
    }
}
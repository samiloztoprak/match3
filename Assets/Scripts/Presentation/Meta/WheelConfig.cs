using System;
using UnityEngine;

public enum WheelRewardType
{
    Booster,
    CardPack,
    Stars,
}

[CreateAssetMenu(fileName = "WheelConfig", menuName = "Match3/WheelConfig")]
public class WheelConfig : ScriptableObject
{
    [Serializable]
    public struct Segment
    {
        public WheelRewardType Type;
        public BoosterType Booster;
        public int Amount;
        public int Weight;
        public Sprite Icon;

        public string AmountLabel => Type == WheelRewardType.Stars ? "+" + Amount : "x" + Amount;
    }

    [SerializeField] private int _starsPerSpin = 10;
    [SerializeField] private Segment[] _segments = new Segment[8];
    [SerializeField] private CollectionCatalog _collection;
    [SerializeField] private Sprite _fallbackStarIcon;

    public int StarsPerSpin => _starsPerSpin;
    public Segment[] Segments => _segments;
    public CollectionCatalog Collection => CollectionCatalogSource.Get(_collection);
    public Sprite FallbackStarIcon => _fallbackStarIcon;

    public int[] Weights()
    {
        var weights = new int[_segments.Length];
        for (int i = 0; i < _segments.Length; i++)
        {
            weights[i] = _segments[i].Weight;
        }
        return weights;
    }
}

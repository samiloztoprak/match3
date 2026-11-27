using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Match3/Collection Catalog")]
public sealed class CollectionCatalog : ScriptableObject
{
    public enum Rarity { Regular, Epic, Legendary }

    [Serializable]
    public sealed class Card
    {
        public string Id;
        public string Title;
        public Rarity Tier;
        public Sprite Art;
    }

    [Serializable]
    public sealed class Set
    {
        public string Id;
        public string Title;
        public Card[] Cards;

        public string[] CardIds()
        {
            return Array.ConvertAll(Cards, card => card.Id);
        }
    }

    [SerializeField] private Set[] _sets;
    [SerializeField, Min(1)] private int _regularWeight = 6;
    [SerializeField, Min(1)] private int _epicWeight = 4;
    [SerializeField, Min(1)] private int _legendaryWeight = 2;

    public IReadOnlyList<Set> Sets => _sets;

    public bool HasIncompleteSet(MetaProgressState state)
    {
        foreach (Set set in _sets)
        {
            if (set.Cards.Length == 0) return true;
            foreach (Card card in set.Cards)
            {
                if (!state.OwnedCards.Contains(card.Id)) return true;
            }
        }
        return false;
    }

    public Card[] Draw(IRandom random)
    {
        var cards = new List<Card>();
        var weights = new List<int>();
        foreach (Set set in _sets)
        {
            foreach (Card card in set.Cards)
            {
                cards.Add(card);
                weights.Add(card.Tier == Rarity.Legendary ? _legendaryWeight
                    : card.Tier == Rarity.Epic ? _epicWeight : _regularWeight);
            }
        }

        return Array.ConvertAll(CollectionLogic.DrawPack(weights, random), index => cards[index]);
    }
}

using System;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    public class CollectionLogicTest
    {
        private sealed class FirstRandom : IRandom
        {
            public int Next(int maxExclusive) => 0;
        }

        [Test]
        public void Pack_draws_three_distinct_cards_and_skips_disabled_cards()
        {
            int[] weights = { 0, 6, 4, 2, 1 };
            int[] pack = CollectionLogic.DrawPack(weights, new FirstRandom());

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, pack);
            CollectionAssert.AreEqual(new[] { 0, 6, 4, 2, 1 }, weights);
        }

        [Test]
        public void Pack_requires_three_eligible_cards()
        {
            Assert.Throws<ArgumentException>(() => CollectionLogic.DrawPack(
                new[] { 3, 0, 2 }, new FirstRandom()));
        }

        [Test]
        public void Duplicate_card_adds_one_star_and_can_earn_a_spin()
        {
            var state = new MetaProgressState { Spins = 0, StarBank = 9 };
            Assert.IsTrue(state.AddCard("rocket", 10));
            Assert.IsFalse(state.AddCard("rocket", 10));

            CollectionAssert.AreEqual(new[] { "rocket" }, state.OwnedCards);
            Assert.AreEqual(1, state.TotalStars);
            Assert.AreEqual(0, state.StarBank);
            Assert.AreEqual(1, state.Spins);
            Assert.AreEqual(0, state.PendingFly);
        }

        [Test]
        public void Completed_set_awards_three_of_each_booster_once_even_after_reload()
        {
            var state = new MetaProgressState();
            state.SetBooster(BoosterType.Hammer, 8);
            string[] cards = { "rocket", "tnt" };

            state.AddCard(cards[0], 10);
            Assert.IsFalse(CollectionLogic.TryClaimSet(state, "circus", cards));

            state.AddCard(cards[1], 10);
            Assert.IsTrue(CollectionLogic.TryClaimSet(state, "circus", cards));
            var reloaded = JsonUtility.FromJson<MetaProgressState>(JsonUtility.ToJson(state));

            Assert.IsFalse(CollectionLogic.TryClaimSet(reloaded, "circus", cards));
            CollectionAssert.AreEquivalent(cards, reloaded.OwnedCards);
            foreach (BoosterType type in Enum.GetValues(typeof(BoosterType)))
                Assert.AreEqual(type == BoosterType.Hammer ? 11 : 6, reloaded.GetBooster(type));
        }
    }
}

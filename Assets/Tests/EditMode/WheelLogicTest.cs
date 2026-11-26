using NUnit.Framework;

namespace Tests.EditMode
{
    public class WheelLogicTest
    {
        private sealed class FixedRandom : IRandom
        {
            private readonly int _value;
            public FixedRandom(int value) => _value = value;
            public int Next(int maxExclusive) => _value;
        }

        [Test]
        public void Pick_respects_weights_and_skips_zero()
        {
            int[] weights = { 10, 0, 30 };
            Assert.AreEqual(0, WheelLogic.PickSegment(weights, new FixedRandom(0)));
            Assert.AreEqual(0, WheelLogic.PickSegment(weights, new FixedRandom(9)));
            Assert.AreEqual(2, WheelLogic.PickSegment(weights, new FixedRandom(10)));
            Assert.AreEqual(2, WheelLogic.PickSegment(weights, new FixedRandom(39)));
        }

        [Test]
        public void All_zero_weights_return_first_segment()
        {
            int[] weights = { 0, 0, 0 };
            Assert.AreEqual(0, WheelLogic.PickSegment(weights, new FixedRandom(0)));
        }

        [Test]
        public void Stars_fill_bar_and_carry_over()
        {
            var (bank, spins) = WheelLogic.AddStars(8, 5, 10);
            Assert.AreEqual(3, bank);
            Assert.AreEqual(1, spins);
        }

        [Test]
        public void Many_stars_give_multiple_spins()
        {
            var (bank, spins) = WheelLogic.AddStars(0, 25, 10);
            Assert.AreEqual(5, bank);
            Assert.AreEqual(2, spins);
        }
    }
}
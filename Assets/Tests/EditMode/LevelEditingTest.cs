using NUnit.Framework;

namespace Tests.EditMode
{
    public class LevelEditingTest
    {
        private static LevelData EmptyLevel(int width, int height)
        {
            var level = new LevelData { width = width, height = height };
            LevelEditing.EnsureRows(level);
            return level;
        }

        [Test]
        public void EnsureRows_FillsMissingRowsAndLayers()
        {
            var level = new LevelData { width = 3, height = 2, cells = new[] { "B.." } };

            LevelEditing.EnsureRows(level);

            CollectionAssert.AreEqual(new[] { "B..", "..." }, level.cells);
            CollectionAssert.AreEqual(new[] { "...", "..." }, level.overlays);
            CollectionAssert.AreEqual(new[] { "...", "..." }, level.floors);
        }

        [Test]
        public void Paint_LockOnBox_IsRejectedWithAReason()
        {
            LevelData level = EmptyLevel(3, 3);
            Assert.IsTrue(LevelEditing.Paint(level, LevelElements.Get(LevelLayer.Cell, 'B'), 0, 0, out _));

            bool placed = LevelEditing.Paint(level, LevelElements.Get(LevelLayer.Overlay, 'L'), 0, 0, out string reason);

            Assert.IsFalse(placed);
            Assert.IsNotNull(reason);
            Assert.AreEqual(LevelData.Empty, level.At(LevelLayer.Overlay, 0, 0));
        }

        [Test]
        public void Erase_OnACoveredCageCell_RemovesTheWholeCage()
        {
            LevelData level = EmptyLevel(3, 3);
            LevelEditing.Paint(level, LevelElements.Get(LevelLayer.Overlay, 'C'), 0, 0, out _);

            Assert.IsTrue(LevelEditing.Erase(level, LevelLayer.Overlay, 1, 1));
            Assert.AreEqual(LevelData.Empty, level.At(LevelLayer.Overlay, 0, 0));
        }

        [Test]
        public void Resize_KeepsContent_AndRemovesCagesThatNoLongerFit()
        {
            LevelData level = EmptyLevel(4, 4);
            LevelEditing.Paint(level, LevelElements.Get(LevelLayer.Cell, 'B'), 0, 0, out _);
            LevelEditing.Paint(level, LevelElements.Get(LevelLayer.Overlay, 'C'), 2, 2, out _);

            LevelEditing.Resize(level, 3, 3);

            Assert.AreEqual(3, level.cells.Length);
            Assert.AreEqual('B', level.At(LevelLayer.Cell, 0, 0));
            Assert.AreEqual(LevelData.Empty, level.At(LevelLayer.Overlay, 2, 2));
        }
        
        [Test]
        public void AutoGoal_FollowsTheBoard_AndIsRemovedWhenEmpty()
        {
            LevelData level = EmptyLevel(3, 3);
            ILevelElement box = LevelElements.Get(LevelLayer.Cell, 'B');

            LevelEditing.Paint(level, box, 0, 0, out _);
            LevelEditing.Paint(level, box, 1, 0, out _);
            Assert.AreEqual(2, level.goals.Find(g => g.type == "ClearBox").count);

            LevelEditing.Erase(level, LevelLayer.Cell, 0, 0);
            Assert.AreEqual(1, level.goals.Find(g => g.type == "ClearBox").count);

            LevelEditing.Erase(level, LevelLayer.Cell, 1, 0);
            Assert.IsNull(level.goals.Find(g => g.type == "ClearBox"));
        }

        [Test]
        public void AutoGoal_KeepsAHandLoweredCount_ButNotAboveTheTotal()
        {
            LevelData level = EmptyLevel(4, 4);
            ILevelElement box = LevelElements.Get(LevelLayer.Cell, 'B');
            for (int x = 0; x < 3; x++)
            {
                LevelEditing.Paint(level, box, x, 0, out _);
            }
            level.goals.Find(g => g.type == "ClearBox").count = 2;

            LevelEditing.Paint(level, box, 3, 0, out _);
            Assert.AreEqual(2, level.goals.Find(g => g.type == "ClearBox").count, "A hand-set count must not follow the board");

            for (int x = 0; x < 3; x++) LevelEditing.Erase(level, LevelLayer.Cell, x, 0);
            Assert.AreEqual(1, level.goals.Find(g => g.type == "ClearBox").count, "The count must not stay above the total");
        }
        
        [Test]
        public void SetColors_RemovingAColor_RemovesItsFixedPiecesAndGoal()
        {
            LevelData level = EmptyLevel(3, 3);
            LevelEditing.Paint(level, LevelElements.Get(LevelLayer.Cell, 'r'), 0, 0, out _);
            LevelEditing.AddColorGoal(level, PieceColor.Red, 10);

            LevelEditing.SetColors(level, new[] { PieceColor.Yellow, PieceColor.Blue, PieceColor.Green });

            CollectionAssert.AreEqual(new[] { "Yellow", "Blue", "Green" }, level.colors);
            Assert.AreEqual(LevelData.Empty, level.At(LevelLayer.Cell, 0, 0));
            Assert.IsEmpty(level.goals);
        }

        [Test]
        public void AddColorGoal_SameColorTwice_AddsOnlyOnce()
        {
            LevelData level = EmptyLevel(3, 3);

            Assert.IsTrue(LevelEditing.AddColorGoal(level, PieceColor.Red, 10));
            Assert.IsFalse(LevelEditing.AddColorGoal(level, PieceColor.Red, 5));
            Assert.AreEqual(1, level.goals.Count);
        }
    }
}
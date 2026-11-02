using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.EditMode
{
    public class SpecialActivationTest
    {
        private static HashSet<int> CollectClearedIds(List<TurnStepData> steps)
        {
            var ids = new HashSet<int>();
            foreach (var step in steps)
            {
                if (step is FallStepData)
                {
                    break;
                }
            
                if (step is ClearPieceData clear)
                {
                    foreach (int id in clear.PieceIds)
                    {
                        ids.Add(id);
                    }
                }
            }
            return ids;
        }

        [Test]
        public void Rocket_HorizontalSwapActivation_ClearsFullRow()
        {
            Board board = new Board(5, 3);

            var p00 = new ColoredPiece(board.NextId(), PieceColor.Red);
            board.SetCell(0, 0, p00);
            board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 0, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(3, 0, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(4, 0, new ColoredPiece(board.NextId(), PieceColor.Red));

            var p01 = new ColoredPiece(board.NextId(), PieceColor.Green);
            board.SetCell(0, 1, p01);
            board.SetCell(1, 1, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(2, 1, new RocketPiece(board.NextId(), RocketDirection.Horizontal));
            board.SetCell(3, 1, new ColoredPiece(board.NextId(), PieceColor.Blue));
            var p41 = new ColoredPiece(board.NextId(), PieceColor.Green);
            board.SetCell(4, 1, p41);

            var p02 = new ColoredPiece(board.NextId(), PieceColor.Red);
            board.SetCell(0, 2, p02);
            board.SetCell(1, 2, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 2, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(3, 2, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(4, 2, new ColoredPiece(board.NextId(), PieceColor.Red));

            var command = new SwapCommand(board, 1, 1, 2, 1);
            var result = TurnResolver.Resolve(board, command);
            var clearedIds = CollectClearedIds(result);

            Assert.IsTrue(clearedIds.Contains(p01.Id));
            Assert.IsTrue(clearedIds.Contains(p41.Id));

            Assert.IsFalse(clearedIds.Contains(p00.Id));
            Assert.IsFalse(clearedIds.Contains(p02.Id));
        }
    
        [Test]
        public void Tnt_SwapActivation_Clears3x3Area()
        {
            Board board = new Board(5, 5);

            var p00 = new ColoredPiece(board.NextId(), PieceColor.Red);
            board.SetCell(0, 0, p00);
            board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 0, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(3, 0, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(4, 0, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(0, 1, new ColoredPiece(board.NextId(), PieceColor.Green));
        
            var p11 = new ColoredPiece(board.NextId(), PieceColor.Yellow);
            board.SetCell(1, 1, p11);
            board.SetCell(2, 1, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(3, 1, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(4, 1, new ColoredPiece(board.NextId(), PieceColor.Green));

            board.SetCell(0, 2, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(1, 2, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 2, new SpecialPiece(board.NextId(), PieceKind.Tnt));
            board.SetCell(3, 2, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(4, 2, new ColoredPiece(board.NextId(), PieceColor.Red));

            board.SetCell(0, 3, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(1, 3, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            var p23 = new ColoredPiece(board.NextId(), PieceColor.Red);
            board.SetCell(2, 3, p23);
            board.SetCell(3, 3, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(4, 3, new ColoredPiece(board.NextId(), PieceColor.Green));

            board.SetCell(0, 4, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(1, 4, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 4, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(3, 4, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            var p44 = new ColoredPiece(board.NextId(), PieceColor.Red);
            board.SetCell(4, 4, p44);

            var command = new SwapCommand(board, 1, 2, 2, 2);
            var result = TurnResolver.Resolve(board, command);
            var clearedIds = CollectClearedIds(result);

            Assert.IsTrue(clearedIds.Contains(p11.Id));
            Assert.IsTrue(clearedIds.Contains(p23.Id));

            Assert.IsFalse(clearedIds.Contains(p00.Id));
            Assert.IsFalse(clearedIds.Contains(p44.Id));
        }

        [Test]
        public void Combo_RocketRocket_ClearsFullCrossAtSwapTarget()
        {
            Board board = new Board(4, 4);

            var p00 = new ColoredPiece(board.NextId(), PieceColor.Red);
            board.SetCell(0, 0, p00);
            board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 0, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(3, 0, new ColoredPiece(board.NextId(), PieceColor.Yellow));

            var p01 = new ColoredPiece(board.NextId(), PieceColor.Green);
            board.SetCell(0, 1, p01);
            board.SetCell(1, 1, new RocketPiece(board.NextId(), RocketDirection.Horizontal));
            board.SetCell(2, 1, new RocketPiece(board.NextId(), RocketDirection.Vertical));
            board.SetCell(3, 1, new ColoredPiece(board.NextId(), PieceColor.Blue));

            board.SetCell(0, 2, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(1, 2, new ColoredPiece(board.NextId(), PieceColor.Blue));
            var p22 = new ColoredPiece(board.NextId(), PieceColor.Green);
            board.SetCell(2, 2, p22);
            board.SetCell(3, 2, new ColoredPiece(board.NextId(), PieceColor.Yellow));

            board.SetCell(0, 3, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(1, 3, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(2, 3, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(3, 3, new ColoredPiece(board.NextId(), PieceColor.Blue));

            var command = new SwapCommand(board, 1, 1, 2, 1);
            var result = TurnResolver.Resolve(board, command);
            var clearedIds = CollectClearedIds(result);

            Assert.IsTrue(clearedIds.Contains(p01.Id));
            Assert.IsTrue(clearedIds.Contains(p22.Id));

            Assert.IsFalse(clearedIds.Contains(p00.Id));
        }
        [Test]
        public void Combo_TntTnt_ClearsRadius4Area()
        {
            Board board = new Board(3, 11);

            board.SetCell(0, 0, new ColoredPiece(board.NextId(), PieceColor.Red));
            var p1y0 = new ColoredPiece(board.NextId(), PieceColor.Blue);
            board.SetCell(1, 0, p1y0);
            board.SetCell(2, 0, new ColoredPiece(board.NextId(), PieceColor.Green));

            board.SetCell(0, 1, new ColoredPiece(board.NextId(), PieceColor.Green));
            var p1y1 = new ColoredPiece(board.NextId(), PieceColor.Yellow);
            board.SetCell(1, 1, p1y1);
            board.SetCell(2, 1, new ColoredPiece(board.NextId(), PieceColor.Red));

            board.SetCell(0, 2, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(1, 2, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 2, new ColoredPiece(board.NextId(), PieceColor.Green));

            board.SetCell(0, 3, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(1, 3, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(2, 3, new ColoredPiece(board.NextId(), PieceColor.Red));

            board.SetCell(0, 4, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(1, 4, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 4, new ColoredPiece(board.NextId(), PieceColor.Green));

            board.SetCell(0, 5, new SpecialPiece(board.NextId(), PieceKind.Tnt));
            board.SetCell(1, 5, new SpecialPiece(board.NextId(), PieceKind.Tnt));
            board.SetCell(2, 5, new ColoredPiece(board.NextId(), PieceColor.Red));

            board.SetCell(0, 6, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(1, 6, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 6, new ColoredPiece(board.NextId(), PieceColor.Green));

            board.SetCell(0, 7, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(1, 7, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(2, 7, new ColoredPiece(board.NextId(), PieceColor.Red));

            board.SetCell(0, 8, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(1, 8, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 8, new ColoredPiece(board.NextId(), PieceColor.Green));

            board.SetCell(0, 9, new ColoredPiece(board.NextId(), PieceColor.Green));
            var p19 = new ColoredPiece(board.NextId(), PieceColor.Yellow);
            board.SetCell(1, 9, p19);
            board.SetCell(2, 9, new ColoredPiece(board.NextId(), PieceColor.Red));

            board.SetCell(0, 10, new ColoredPiece(board.NextId(), PieceColor.Red));
            var p110 = new ColoredPiece(board.NextId(), PieceColor.Blue);
            board.SetCell(1, 10, p110);
            board.SetCell(2, 10, new ColoredPiece(board.NextId(), PieceColor.Green));

            var command = new SwapCommand(board, 0, 5, 1, 5);
            var result = TurnResolver.Resolve(board, command);
            var clearedIds = CollectClearedIds(result);

            Assert.IsTrue(clearedIds.Contains(p1y1.Id), "exactly 4 away should still be cleared");
            Assert.IsTrue(clearedIds.Contains(p19.Id));

            Assert.IsFalse(clearedIds.Contains(p1y0.Id), "5 away should be outside the blast");
            Assert.IsFalse(clearedIds.Contains(p110.Id));
        }

        [Test]
        public void Combo_RocketTnt_ClearsThickCross()
        {
            Board board = new Board(6, 6);

            var p00 = new ColoredPiece(board.NextId(), PieceColor.Red);
            board.SetCell(0, 0, p00);
            board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceColor.Blue));
        
            var p20 = new ColoredPiece(board.NextId(), PieceColor.Green);
            board.SetCell(2, 0, p20);
            board.SetCell(3, 0, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(4, 0, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(5, 0, new ColoredPiece(board.NextId(), PieceColor.Blue));

            board.SetCell(0, 1, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(1, 1, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(2, 1, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(3, 1, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(4, 1, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(5, 1, new ColoredPiece(board.NextId(), PieceColor.Yellow));

            var p02 = new ColoredPiece(board.NextId(), PieceColor.Red);
            board.SetCell(0, 2, p02);
            board.SetCell(1, 2, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 2, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(3, 2, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(4, 2, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(5, 2, new ColoredPiece(board.NextId(), PieceColor.Blue));

            board.SetCell(0, 3, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(1, 3, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(2, 3, new RocketPiece(board.NextId(), RocketDirection.Horizontal));
            board.SetCell(3, 3, new SpecialPiece(board.NextId(), PieceKind.Tnt));
            board.SetCell(4, 3, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(5, 3, new ColoredPiece(board.NextId(), PieceColor.Yellow));

            board.SetCell(0, 4, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(1, 4, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 4, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(3, 4, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(4, 4, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(5, 4, new ColoredPiece(board.NextId(), PieceColor.Blue));

            board.SetCell(0, 5, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(1, 5, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(2, 5, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(3, 5, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(4, 5, new ColoredPiece(board.NextId(), PieceColor.Green));
            var p55 = new ColoredPiece(board.NextId(), PieceColor.Yellow);
            board.SetCell(5, 5, p55);

            var command = new SwapCommand(board, 2, 3, 3, 3);
            var result = TurnResolver.Resolve(board, command);
            var clearedIds = CollectClearedIds(result);

            Assert.IsTrue(clearedIds.Contains(p02.Id));
            Assert.IsTrue(clearedIds.Contains(p20.Id));

            Assert.IsFalse(clearedIds.Contains(p00.Id));
            Assert.IsFalse(clearedIds.Contains(p55.Id));
        }

        [Test]
        public void Combo_LightBallLightBall_ClearsEntireBoard()
        {
            Board board = new Board(4, 4);

            board.SetCell(0, 0, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 0, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(3, 0, new ColoredPiece(board.NextId(), PieceColor.Yellow));

            board.SetCell(0, 1, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(1, 1, new SpecialPiece(board.NextId(), PieceKind.LightBall));
            board.SetCell(2, 1, new SpecialPiece(board.NextId(), PieceKind.LightBall));
            board.SetCell(3, 1, new ColoredPiece(board.NextId(), PieceColor.Blue));

            board.SetCell(0, 2, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(1, 2, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 2, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(3, 2, new ColoredPiece(board.NextId(), PieceColor.Yellow));

            board.SetCell(0, 3, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(1, 3, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(2, 3, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(3, 3, new ColoredPiece(board.NextId(), PieceColor.Blue));
        
            var originalIds = new List<int>();
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    originalIds.Add(board.GetCell(x, y).Id);
                }
            }

            var command = new SwapCommand(board, 1, 1, 2, 1);
            var result = TurnResolver.Resolve(board, command);
            var clearedIds = CollectClearedIds(result);

            foreach (int id in originalIds)
            {
                Assert.IsTrue(clearedIds.Contains(id), $"piece {id} should be cleared by LightBall+LightBall");
            }
        }
    
        [Test]
        public void ChainActivation_RocketBlastHitsTnt_TntAlsoActivates()
        {
            Board board = new Board(8, 5);

            board.SetCell(0, 0, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 0, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(3, 0, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(4, 0, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(5, 0, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(6, 0, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(7, 0, new ColoredPiece(board.NextId(), PieceColor.Yellow));

            board.SetCell(0, 1, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(1, 1, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(2, 1, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(3, 1, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(4, 1, new ColoredPiece(board.NextId(), PieceColor.Green));
            var p51 = new ColoredPiece(board.NextId(), PieceColor.Yellow);
            board.SetCell(5, 1, p51);
            board.SetCell(6, 1, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(7, 1, new ColoredPiece(board.NextId(), PieceColor.Blue));

            board.SetCell(0, 2, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(1, 2, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 2, new RocketPiece(board.NextId(), RocketDirection.Horizontal));
            board.SetCell(3, 2, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(4, 2, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(5, 2, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(6, 2, new SpecialPiece(board.NextId(), PieceKind.Tnt));
            board.SetCell(7, 2, new ColoredPiece(board.NextId(), PieceColor.Yellow));

            board.SetCell(0, 3, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(1, 3, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(2, 3, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(3, 3, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(4, 3, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(5, 3, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(6, 3, new ColoredPiece(board.NextId(), PieceColor.Red));
            var p73 = new ColoredPiece(board.NextId(), PieceColor.Blue);
            board.SetCell(7, 3, p73);

            var p04 = new ColoredPiece(board.NextId(), PieceColor.Red);
            board.SetCell(0, 4, p04);
            board.SetCell(1, 4, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 4, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(3, 4, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(4, 4, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(5, 4, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(6, 4, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(7, 4, new ColoredPiece(board.NextId(), PieceColor.Yellow));

            var command = new SwapCommand(board, 1, 2, 2, 2);
            var result = TurnResolver.Resolve(board, command);
            var clearedIds = CollectClearedIds(result);

            Assert.IsTrue(clearedIds.Contains(p51.Id));
            Assert.IsTrue(clearedIds.Contains(p73.Id));

            Assert.IsFalse(clearedIds.Contains(p04.Id));

            int activationCount = 0;
            foreach (var step in result)
            {
                if (step is SpecialActivatedData)
                {
                    activationCount++;
                }
            }
            Assert.AreEqual(2, activationCount);
        }
    
        [Test]
        public void TapCommand_OnSpecialPiece_ActivatesWithoutSwap()
        {
            Board board = new Board(5, 5);

            var p00 = new ColoredPiece(board.NextId(), PieceColor.Red);
            board.SetCell(0, 0, p00);
            board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 0, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(3, 0, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(4, 0, new ColoredPiece(board.NextId(), PieceColor.Red));

            board.SetCell(0, 1, new ColoredPiece(board.NextId(), PieceColor.Green));
            var p11 = new ColoredPiece(board.NextId(), PieceColor.Yellow);
            board.SetCell(1, 1, p11);
            board.SetCell(2, 1, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(3, 1, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(4, 1, new ColoredPiece(board.NextId(), PieceColor.Green));

            board.SetCell(0, 2, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(1, 2, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 2, new SpecialPiece(board.NextId(), PieceKind.Tnt));
            board.SetCell(3, 2, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(4, 2, new ColoredPiece(board.NextId(), PieceColor.Red));

            board.SetCell(0, 3, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(1, 3, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(2, 3, new ColoredPiece(board.NextId(), PieceColor.Red));
            var p33 = new ColoredPiece(board.NextId(), PieceColor.Blue);
            board.SetCell(3, 3, p33);
            board.SetCell(4, 3, new ColoredPiece(board.NextId(), PieceColor.Green));

            board.SetCell(0, 4, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(1, 4, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 4, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(3, 4, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            var p44 = new ColoredPiece(board.NextId(), PieceColor.Red);
            board.SetCell(4, 4, p44);

            var command = new TapCommand(2, 2);
            var result = TurnResolver.Resolve(board, command);
            var clearedIds = CollectClearedIds(result);

            Assert.IsTrue(clearedIds.Contains(p11.Id));
            Assert.IsTrue(clearedIds.Contains(p33.Id));

            Assert.IsFalse(clearedIds.Contains(p00.Id));
            Assert.IsFalse(clearedIds.Contains(p44.Id));
        }
    }
}

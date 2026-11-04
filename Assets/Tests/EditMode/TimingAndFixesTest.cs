using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.EditMode
{
    public class TimingAndFixesTest
    {
        private static List<HitData> CollectClearHits(List<TurnStepData> steps)
        {
            var hits = new List<HitData>();
            foreach (var step in steps)
            {
                if (step is FallStepData)
                {
                    break;
                }
                if (step is ClearPieceData clear)
                {
                    hits.AddRange(clear.Hits);
                }
            }
            return hits;
        }

        [Test]
        public void Rocket_TapActivation_TimeMsIncreasesLinearlyWithDistance()
        {
            Board board = new Board(5, 1);
            board.SetCell(0, 0, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 0, new RocketPiece(board.NextId(), RocketDirection.Horizontal));
            board.SetCell(3, 0, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(4, 0, new ColoredPiece(board.NextId(), PieceColor.Yellow));

            var result = TurnResolver.Resolve(board, new TapCommand(2, 0));
            var hits = CollectClearHits(result);

            var byX = new Dictionary<int, float>();
            foreach (var hit in hits)
            {
                byX[hit.X] = hit.TimeMs;
            }

            Assert.AreEqual(5, byX.Count);
            Assert.AreEqual(ActivationTiming.TapActivationMs, byX[2], 0.01f, "rocket's own cell");
            Assert.AreEqual(ActivationTiming.TapActivationMs + ActivationTiming.RocketMsPerCell, byX[1], 0.01f);
            Assert.AreEqual(ActivationTiming.TapActivationMs + ActivationTiming.RocketMsPerCell, byX[3], 0.01f);
            Assert.AreEqual(ActivationTiming.TapActivationMs + ActivationTiming.RocketMsPerCell * 2, byX[0], 0.01f);
            Assert.AreEqual(ActivationTiming.TapActivationMs + ActivationTiming.RocketMsPerCell * 2, byX[4], 0.01f);
        }

        [Test]
        public void Tnt_TapActivation_TimeMsIncreasesByChebyshevRing()
        {
            Board board = new Board(3, 3);
            board.SetCell(0, 0, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 0, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(0, 1, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(1, 1, new SpecialPiece(board.NextId(), PieceKind.Tnt));
            board.SetCell(2, 1, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(0, 2, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(1, 2, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(2, 2, new ColoredPiece(board.NextId(), PieceColor.Yellow));

            var result = TurnResolver.Resolve(board, new TapCommand(1, 1));
            var hits = CollectClearHits(result);

            float center = -1f, corner = -1f, edge = -1f;
            foreach (var hit in hits)
            {
                if (hit.X == 1 && hit.Y == 1) center = hit.TimeMs;
                else if (hit.X == 0 && hit.Y == 0) corner = hit.TimeMs;
                else if (hit.X == 1 && hit.Y == 0) edge = hit.TimeMs;
            }

            float baseMs = ActivationTiming.TapActivationMs + ActivationTiming.TntDelayMs;
            Assert.AreEqual(baseMs, center, 0.01f, "center is ring 0");
            Assert.AreEqual(baseMs + ActivationTiming.TntRingMs, edge, 0.01f, "edge is ring 1");
            Assert.AreEqual(baseMs + ActivationTiming.TntRingMs, corner, 0.01f, "Chebyshev distance: corner is also ring 1, not ring 2");
        }

        [Test]
        public void ChainActivation_RocketTriggersTnt_SecondActivationStartsAfterChainDelay()
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
            board.SetCell(5, 1, new ColoredPiece(board.NextId(), PieceColor.Yellow));
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
            board.SetCell(7, 3, new ColoredPiece(board.NextId(), PieceColor.Blue));

            board.SetCell(0, 4, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(1, 4, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 4, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(3, 4, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(4, 4, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(5, 4, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(6, 4, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(7, 4, new ColoredPiece(board.NextId(), PieceColor.Yellow));

            var result = TurnResolver.Resolve(board, new TapCommand(2, 2));

            SpecialActivatedData rocketActivation = null;
            SpecialActivatedData tntActivation = null;
            foreach (var step in result)
            {
                if (step is SpecialActivatedData data)
                {
                    if (data.Kind == PieceKind.Rocket) rocketActivation = data;
                    else if (data.Kind == PieceKind.Tnt) tntActivation = data;
                }
            }

            Assert.IsNotNull(rocketActivation);
            Assert.IsNotNull(tntActivation);

            float rocketHitAtTnt = ActivationTiming.TapActivationMs + ActivationTiming.RocketMsPerCell * 4;
            float expectedTntStart = rocketHitAtTnt + ActivationTiming.ChainDelayMs;
            Assert.AreEqual(expectedTntStart, tntActivation.TimeMs, 0.01f);
        }

        [Test]
        public void ClearCellsAndEnqueueChains_OverlayBreaksUnderLockedPiece_PieceOnlyUnlockedNotDamagedThisHit()
        {
            Board board = new Board(3, 1);
            board.SetCell(0, 0, new ColoredPiece(board.NextId(), PieceColor.Red));

            var lockedPiece = new BoxPiece(board.NextId(), PieceColor.Empty);
            board.SetCell(1, 0, lockedPiece);
            LogicLibrary.BoardLogic.AddFlag(lockedPiece, PieceFlags.Locked);

            var lockOverlay = new LockPiece(board.NextId(), 1, 0);
            board.AddOverlayPiece(lockOverlay);

            board.SetCell(2, 0, new RocketPiece(board.NextId(), RocketDirection.Horizontal));

            int healthBefore = lockedPiece.Health;

            TurnResolver.Resolve(board, new TapCommand(2, 0));

            Assert.IsFalse(LogicLibrary.BoardLogic.IsPieceLocked(lockedPiece), "lock should break and free the piece");
            Assert.AreEqual(healthBefore, lockedPiece.Health, "freed piece must not also take damage in the same hit");
        }

        [Test]
        public void Resolve_ValidSwapProducesNoMatchOrActivation_ReturnsEmptyResult()
        {
            Board board = new Board(3, 3);
            board.SetCell(0, 0, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(1, 0, new ColoredPiece(board.NextId(), PieceColor.Blue));
            board.SetCell(2, 0, new ColoredPiece(board.NextId(), PieceColor.Green));

            board.SetCell(0, 1, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(1, 1, new ColoredPiece(board.NextId(), PieceColor.Red));
            board.SetCell(2, 1, new ColoredPiece(board.NextId(), PieceColor.Blue));

            board.SetCell(0, 2, new ColoredPiece(board.NextId(), PieceColor.Green));
            board.SetCell(1, 2, new ColoredPiece(board.NextId(), PieceColor.Yellow));
            board.SetCell(2, 2, new ColoredPiece(board.NextId(), PieceColor.Red));

            var command = new SwapCommand(board, 1, 1, 2, 1);
            var result = TurnResolver.Resolve(board, command);

            Assert.AreEqual(0, result.Count, "a valid swap with no resulting match/activation should produce no steps");
        }
    }
}
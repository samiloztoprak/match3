using System.Collections.Generic;
using UnityEngine;


public static class ViewCommandFactory
{
    public static List<(float startTime, IViewCommand command)> Build(
        List<TurnStepData> steps, TurnPlayer player)
    {
        var result = new List<(float, IViewCommand)>();

        float popDuration = player.PopDuration;
        float fallDuration = player.FallDuration;
        float fallDelay = player.FallDelay;
        float mergeDuration = player.MergeDuration;

        float waveStart = 0f;
        float lastHit = 0f;         
        float fallStart = -1f;
        bool fallHasMoves = false;

        float At(float timeMs) => waveStart + Mathf.Max(0f, timeMs) / 1000f;

        void MarkHit(float t)
        {
            if (t > lastHit) lastHit = t;
        }

        void AddClearHits(List<HitData> hits)
        {
            if (hits == null || hits.Count == 0) return;
            var buckets = new SortedDictionary<float, List<HitData>>();
            foreach (HitData hit in hits)
            {
                if (!buckets.TryGetValue(hit.TimeMs, out var list))
                {
                    list = new List<HitData>();
                    buckets[hit.TimeMs] = list;
                }
                list.Add(hit);
            }
            foreach (var pair in buckets)
            {
                float t = At(pair.Key);
                result.Add((t, new ClearPiecesViewCommand(pair.Value, player, popDuration)));
                MarkHit(t);
            }
        }

        void AddDamageHits(List<(int pieceId, int healthAfter, float timeMs)> hits)
        {
            if (hits == null || hits.Count == 0) return;
            var buckets = new SortedDictionary<float, List<(int, int, float)>>();
            foreach (var hit in hits)
            {
                if (!buckets.TryGetValue(hit.timeMs, out var list))
                {
                    list = new List<(int, int, float)>();
                    buckets[hit.timeMs] = list;
                }
                list.Add(hit);
            }
            foreach (var pair in buckets)
            {
                float t = At(pair.Key);
                result.Add((t, new DamageViewCommand(pair.Value, player, popDuration)));
                MarkHit(t);
            }
        }

        void BeginFallPhase()
        {
            if (fallStart < 0f)
            {
                fallStart = lastHit + fallDelay;
            }
        }

        void EndWave()
        {
            BeginFallPhase();
            float waveEnd = fallHasMoves ? fallStart + fallDuration : lastHit;
            waveStart = Mathf.Max(waveEnd, waveStart);
            lastHit = waveStart;
            fallStart = -1f;
            fallHasMoves = false;
        }

        foreach (var step in steps)
        {
            switch (step)
            {
                case ClearPieceData clear:
                    AddClearHits(clear.Hits);
                    break;

                case ClearOverlayData overlay:
                    AddClearHits(overlay.Hits);
                    break;

                case DamagePieceData damage:
                    AddDamageHits(damage.Hits);
                    break;

                case SpecialActivatedData activated:
                {
                    float t = At(activated.TimeMs);
                    bool isPropellerFlight = activated.Kind == PieceKind.Propeller
                        && activated.TargetX.HasValue && activated.TargetY.HasValue;

                    if (isPropellerFlight)
                    {
                        float flight = ActivationTiming.PropellerFlightMs / 1000f;
                        Vector3 targetPos = player.CellToWorld(activated.TargetX.Value, activated.TargetY.Value);
                        var config = player.PropellerConfig;

                        if (activated.NeighborCells != null && activated.NeighborCells.Count > 0)
                        {
                            result.Add((t, new AreaDamageViewCommand(activated.NeighborCells, player, 0f)));
                        }
                        result.Add((t, new PropellerFlyViewCommand(activated.PieceId, targetPos, player, config, flight)));
                        result.Add((t + flight, new AreaDamageViewCommand(
                            new List<(int x, int y)> { (activated.TargetX.Value, activated.TargetY.Value) },
                            player, config.ImpactFlashDuration)));
                        MarkHit(t + flight);
                    }
                    else
                    {
                        result.Add((t, new SpecialActivationFlashViewCommand(activated.PieceId, activated.X, activated.Y, player, 0f)));
                        MarkHit(t);
                    }
                    break;
                }

                case SpecialCreatedData created:
                {
                    Vector3 anchorPos = player.CellToWorld(created.X, created.Y);
                    result.Add((waveStart, new MergeToAnchorViewCommand(created.MergedIds, anchorPos, player, mergeDuration)));
                    result.Add((waveStart + mergeDuration, new SpawnSpecialViewCommand(created.Piece, anchorPos, player, 0f)));
                    MarkHit(waveStart + mergeDuration);
                    break;
                }

                case FallStepData fall:
                    BeginFallPhase();
                    if (fall.Moves.Count > 0)
                    {
                        result.Add((fallStart, new FallViewCommand(fall.Moves, player, fallDuration)));
                        fallHasMoves = true;
                    }
                    break;

                case RefillStepData refill:
                    BeginFallPhase();
                    if (refill.Moves.Count > 0)
                    {
                        result.Add((fallStart, new RefillViewCommand(refill.Moves, player, fallDuration)));
                        fallHasMoves = true;
                    }
                    EndWave();
                    break;

                case ShuffleStepData shuffle:
                {
                    const float shuffleDuration = 0.5f;
                    result.Add((waveStart, new ShuffleViewCommand(shuffle.Moves, player, shuffleDuration)));
                    waveStart += shuffleDuration;
                    lastHit = waveStart;
                    break;
                }
            }
        }

        return result;
    }
}

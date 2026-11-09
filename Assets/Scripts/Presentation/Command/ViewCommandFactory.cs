using System.Collections.Generic;
using UnityEngine;


public static class ViewCommandFactory
{
    public static List<(float startTime, IViewCommand command)> Build(
        List<TurnStepData> steps, TurnPlayer player)
    {
        var result = new List<(float, IViewCommand)>();

        float popDuration = player.PopDuration;
        float fallSecondsPerCell = player.FallSecondsPerCell;
        float fallDelay = player.FallDelay;
        float mergeDuration = player.MergeDuration;

        float waveStart = 0f;
        float lastHit = 0f;
        float fallStart = -1f;
        bool fallHasMoves = false;
        float fallMaxDuration = 0f; 

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
            float waveEnd = fallHasMoves ? fallStart + fallMaxDuration : lastHit;
            waveStart = Mathf.Max(waveEnd, waveStart);
            lastHit = waveStart;
            fallStart = -1f;
            fallHasMoves = false;
            fallMaxDuration = 0f;
        }

        foreach (var step in steps)
        {
            switch (step)
            {
                case ClearPieceData clear:
                {
                    AddClearHits(clear.Hits);
                    break;
                }
                
                case ClearOverlayData overlay:
                {
                    AddClearHits(overlay.Hits);
                    break;
                }
                
                case DamagePieceData damage:
                {
                    AddDamageHits(damage.Hits);
                    break;
                }
                
                case SpecialActivationData activated:
                {
                    float t = At(activated.TimeMs);
                    IActivationPresenter presenter = ActivationPresenterRegistry.Get(activated);
                    MarkHit(presenter.Present(activated, t, player, result));
                    break;
                }
                
                case SpecialCreatedData created:
                {
                    float ct = At(created.TimeMs);
                    Vector3 anchorPos = player.CellToWorld(created.X, created.Y);
                    result.Add((ct, new MergeToAnchorViewCommand(created.MergedIds, anchorPos, player, mergeDuration)));
                    result.Add((ct + mergeDuration, new SpawnSpecialViewCommand(created.Piece, anchorPos, player, 0f)));
                    MarkHit(ct + mergeDuration);
                    break;
                }
                
                case FallStepData fall:
                {
                    BeginFallPhase();
                    if (fall.Moves.Count > 0)
                    {
                        var fallCmd = new FallViewCommand(fall.Moves, player, fallSecondsPerCell);
                        result.Add((fallStart, fallCmd));
                        fallHasMoves = true;
                        fallMaxDuration = Mathf.Max(fallMaxDuration, fallCmd.Duration);
                    }
                    break;
                }
                
                case RefillStepData refill:
                {
                    BeginFallPhase();
                    if (refill.Moves.Count > 0)
                    {
                        var refillCmd = new RefillViewCommand(refill.Moves, player, fallSecondsPerCell);
                        result.Add((fallStart, refillCmd));
                        fallHasMoves = true;
                        fallMaxDuration = Mathf.Max(fallMaxDuration, refillCmd.Duration);
                    }
                    EndWave();
                    break;
                }
                
                case ShuffleStepData shuffle:
                {
                    const float shuffleDuration = 0.5f;
                    result.Add((waveStart, new ShuffleViewCommand(shuffle.Moves, player, shuffleDuration)));
                    waveStart += shuffleDuration;
                    lastHit = waveStart;
                    break;
                }
                
                case ComboMergeData merge:
                {
                    float duration = ActivationTiming.ComboMergeMs / 1000f;
                    Vector3 to = player.CellToWorld(merge.ToX, merge.ToY);
                    result.Add((waveStart, new MergeToAnchorViewCommand(new List<int> { merge.PieceId }, to, player, duration)));
                    MarkHit(waveStart + duration);
                    break;
                }
            }
        }

        return result;
    }
}

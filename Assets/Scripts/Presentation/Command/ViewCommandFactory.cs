using System.Collections.Generic;
using UnityEngine;

public class ViewCommandFactory
{
    public static List<(float startTime, IViewCommand command)> Build(List<TurnStepData> steps, TurnPlayer player, float stepDuration)
    {
        var result = new List<(float, IViewCommand)>();
        float time = 0f;
        float clearPhaseStartTime = 0f;

        foreach (var step in steps)
        {
            if (step is ClearPieceData clearStep)
            {
                var cmd = new ClearPiecesViewCommand(clearStep.Hits, player, stepDuration);
                clearPhaseStartTime = time;
                result.Add((time, cmd));
                time += cmd.Duration;
            }
            else if (step is DamagePieceData damageStep)
            {
                var cmd = new DamageViewCommand(damageStep.Hits, player, stepDuration);
                result.Add((time, cmd));
                time += cmd.Duration;
            }
            else if (step is ClearOverlayData clearOverlayStep)
            {
                var cmd = new ClearPiecesViewCommand(clearOverlayStep.Hits, player, stepDuration);
                result.Add((time, cmd));
                time += cmd.Duration;
            }
            else if (step is FallStepData fallStep)
            {
                var cmd = new FallViewCommand(fallStep.Moves, player, stepDuration);
                result.Add((clearPhaseStartTime, cmd));
                time = Mathf.Max(time, clearPhaseStartTime + cmd.Duration);
            }
            else if (step is RefillStepData refillStep)
            {
                var cmd = new RefillViewCommand(refillStep.Moves, player, stepDuration);
                result.Add((clearPhaseStartTime, cmd));
                time = Mathf.Max(time, clearPhaseStartTime + cmd.Duration);
            }
            else if (step is SpecialCreatedData specialCreatedStep)
            {
                Vector3 anchorPos = player.CellToWorld(specialCreatedStep.X, specialCreatedStep.Y);
                var mergeCmd = new MergeToAnchorViewCommand(specialCreatedStep.MergedIds, anchorPos, player, stepDuration);
                result.Add((time, mergeCmd));
                time += mergeCmd.Duration;

                var spawnCmd = new SpawnSpecialViewCommand(specialCreatedStep.Piece, anchorPos, player, stepDuration);
                result.Add((time, spawnCmd));
                time += spawnCmd.Duration;
            }
            else if (step is SpecialActivatedData activatedStep)
            {
                bool isPropellerFlight = activatedStep.Kind == PieceKind.Propeller
                    && activatedStep.TargetX.HasValue && activatedStep.TargetY.HasValue;

                if (isPropellerFlight)
                {
                    Vector3 targetPos = player.CellToWorld(activatedStep.TargetX.Value, activatedStep.TargetY.Value);
                    var propellerConfig = player.PropellerConfig;

                    var neighborCmd = new AreaDamageViewCommand(activatedStep.NeighborCells, player, 0f);
                    result.Add((time, neighborCmd));

                    var flyCmd = new PropellerFlyViewCommand(activatedStep.PieceId, targetPos, player, propellerConfig);
                    result.Add((time, flyCmd));
                    time += flyCmd.Duration;

                    var impactCmd = new AreaDamageViewCommand(
                        new List<(int x, int y)> { (activatedStep.TargetX.Value, activatedStep.TargetY.Value) },
                        player, propellerConfig.ImpactFlashDuration);
                    result.Add((time, impactCmd));
                    time += impactCmd.Duration;
                }
                else
                {
                    var cmd = new SpecialActivationFlashViewCommand(activatedStep.PieceId, activatedStep.X, activatedStep.Y, player, stepDuration);
                    result.Add((time, cmd));
                    time += cmd.Duration;
                }
            }
            else if (step is ShuffleStepData shuffleStep)
            {
                var cmd = new ShuffleViewCommand(shuffleStep.Moves, player, 0.5f);
                result.Add((time, cmd));
                time += cmd.Duration;
            }
        }

        return result;
    }
}

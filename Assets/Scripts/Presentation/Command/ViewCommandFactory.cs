using System.Collections.Generic;
using UnityEngine;

public class ViewCommandFactory
{
    public static List<IViewCommand> Build(List<TurnStepData> steps, TurnPlayer player, float stepDuration)
    {
        var commands = new List<IViewCommand>();

        foreach (var step in steps)
        {
            if (step is ClearPieceData clearStep)
            {
                commands.Add(new ClearPiecesViewCommand(clearStep.PieceIds, player, stepDuration));
            }
            else if (step is DamagePieceData damageStep)
            {
                commands.Add(new DamageViewCommand(damageStep.Hits, player, stepDuration));
            }
            else if (step is ClearOverlayData clearOverlayStep)
            {
                commands.Add(new ClearPiecesViewCommand(clearOverlayStep.PieceIds, player, stepDuration));
            }
            else if (step is FallStepData fallStep)
            {
                commands.Add(new FallViewCommand(fallStep.Moves, player, stepDuration));
            }
            else if (step is RefillStepData refillStep)
            {
                commands.Add(new RefillViewCommand(refillStep.Moves, player, stepDuration));
            }
            else if (step is SpecialCreatedData specialCreatedStep)
            {
                Vector3 anchorPos = player.CellToWorld(specialCreatedStep.X, specialCreatedStep.Y);
                commands.Add(new MergeToAnchorViewCommand(specialCreatedStep.MergedIds, anchorPos, player, stepDuration));
                commands.Add(new SpawnSpecialViewCommand(specialCreatedStep.Piece, anchorPos, player, stepDuration));
            }
            else if (step is SpecialActivatedData activatedStep)
            {
                bool isPropellerFlight = activatedStep.Kind == PieceKind.Propeller
                    && activatedStep.TargetX.HasValue && activatedStep.TargetY.HasValue;

                if (isPropellerFlight)
                {
                    Vector3 targetPos = player.CellToWorld(activatedStep.TargetX.Value, activatedStep.TargetY.Value);
                    var propellerConfig = player.PropellerConfig;

                    commands.Add(new AreaDamageViewCommand(activatedStep.NeighborCells, player, 0f));
                    commands.Add(new PropellerFlyViewCommand(activatedStep.PieceId, targetPos, player, propellerConfig));
                    commands.Add(new AreaDamageViewCommand(
                        new List<(int x, int y)> { (activatedStep.TargetX.Value, activatedStep.TargetY.Value) },
                        player, propellerConfig.ImpactFlashDuration));
                }
                else
                {
                    commands.Add(new SpecialActivationFlashViewCommand(activatedStep.PieceId, activatedStep.X, activatedStep.Y, player, stepDuration));
                }
            }
        }

        return commands;
    }
}

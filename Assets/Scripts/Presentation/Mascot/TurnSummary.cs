using System.Collections.Generic;

public readonly struct TurnSummary
{
    public readonly int ClearedCount;
    public readonly bool HasCombo;
    public readonly bool HasLightBallCombo;
    public readonly bool CreatedSpecial;
    public readonly int ToysCollected;

    private TurnSummary(int cleared, bool combo, bool lightBallCombo, bool createdSpecial, int toys)
    {
        ClearedCount = cleared;
        HasCombo = combo;
        HasLightBallCombo = lightBallCombo;
        CreatedSpecial = createdSpecial;
        ToysCollected = toys;
    }

    public static TurnSummary From(List<TurnStepData> steps)
    {
        int cleared = 0;
        int toys = 0;
        bool combo = false;
        bool lightBallCombo = false;
        bool createdSpecial = false;

        foreach (TurnStepData step in steps)
        {
            switch (step)
            {
                case ClearPieceData clear:
                    cleared += clear.Hits.Count;
                    break;
                case SpecialActivationData activation when activation.IsCombo:
                    combo = true;
                    if (activation.Kind == PieceKind.LightBall || activation.ComboPartnerKind == PieceKind.LightBall)
                    {
                        lightBallCombo = true;
                    }
                    break;
                case SpecialCreatedData _:
                    createdSpecial = true;
                    break;
                case CollectStepData collect:
                    toys += collect.Hits.Count;
                    break;
            }
        }

        return new TurnSummary(cleared, combo, lightBallCombo, createdSpecial, toys);
    }
}
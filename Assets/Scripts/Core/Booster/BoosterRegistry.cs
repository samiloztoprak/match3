using System.Collections.Generic;

public static class BoosterRegistry
{
    private static readonly Dictionary<BoosterType, IBoosterEffect> Effects = new Dictionary<BoosterType, IBoosterEffect>
    {
        { BoosterType.Hammer, new HammerBooster() },
        { BoosterType.Row, new LineBooster(BoosterType.Row, RocketDirection.Horizontal) },
        { BoosterType.Column, new LineBooster(BoosterType.Column, RocketDirection.Vertical) },
    };

    public static IBoosterEffect Get(BoosterType type)
    {
        return Effects.TryGetValue(type, out IBoosterEffect effect) ? effect : null;
    }
}
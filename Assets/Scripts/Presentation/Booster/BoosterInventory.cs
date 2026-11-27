using System;

public static class BoosterInventory
{
    public static event Action<BoosterType, int> Changed;

    public static int Get(BoosterType type) => MetaProgress.State.GetBooster(type);

    internal static void NotifyChanged(BoosterType type, int count) => Changed?.Invoke(type, count);

    public static void Add(BoosterType type, int amount)
    {
        MetaProgressState next = MetaProgress.BeginUpdate();
        next.SetBooster(type, next.GetBooster(type) + amount);
        MetaProgress.Commit(next);
    }

    public static bool TryUse(BoosterType type)
    {
        if (Get(type) <= 0) return false;
        Add(type, -1);
        return true;
    }
}

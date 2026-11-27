using System;

public static class WheelProgress
{
    public static event Action Changed;

    public static int StarBank => MetaProgress.State.StarBank;
    public static int Spins => MetaProgress.State.Spins;
    public static int TotalStars => MetaProgress.State.TotalStars;

    internal static void NotifyChanged() => Changed?.Invoke();

    public static void AddStars(int stars, int starsPerSpin, bool queueMenuFly = true)
    {
        if (stars <= 0) return;
        MetaProgressState next = MetaProgress.BeginUpdate();
        next.AddStars(stars, starsPerSpin, queueMenuFly);
        MetaProgress.Commit(next);
    }

    public static bool TryUseSpin()
    {
        if (Spins <= 0) return false;
        MetaProgressState next = MetaProgress.BeginUpdate();
        next.Spins--;
        MetaProgress.Commit(next);
        return true;
    }

    public static int ConsumePendingFly()
    {
        int stars = MetaProgress.State.PendingFly;
        if (stars <= 0) return 0;

        MetaProgressState next = MetaProgress.BeginUpdate();
        next.PendingFly = 0;
        MetaProgress.Commit(next);
        return stars;
    }
}

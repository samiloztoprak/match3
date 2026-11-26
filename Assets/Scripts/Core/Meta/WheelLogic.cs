using System.Collections.Generic;

public static class WheelLogic
{
    public static int PickSegment(IReadOnlyList<int> weights, IRandom random)
    {
        int total = 0;
        foreach (int w in weights)
        {
            total += w > 0 ? w : 0;
        }
        
        if (total <= 0)
        {
            return 0;
        }

        int roll = random.Next(total);
        for (int i = 0; i < weights.Count; i++)
        {
            int w = weights[i] > 0 ? weights[i] : 0;
            if (roll < w)
            {
                return i;
            }
            roll -= w;
        }
        return weights.Count - 1;
    }

    public static (int bank, int spinsGained) AddStars(int bank, int added, int starsPerSpin)
    {
        if (starsPerSpin <= 0)
        {
            return (bank, 0);
        }
        int total = bank + added;
        return (total % starsPerSpin, total / starsPerSpin);
    }
}
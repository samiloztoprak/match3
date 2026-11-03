using System;

public sealed class SystemRandom : IRandom
{
    private readonly Random _random;

    public SystemRandom()
    {
        _random = new Random();
    }

    public SystemRandom(int seed)
    {
        _random = new Random(seed);
    }

    public int Next(int maxExclusive)
    {
        return _random.Next(maxExclusive);
    }
}
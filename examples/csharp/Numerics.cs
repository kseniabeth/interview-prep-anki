using System;
public static class Numerics
{
    // Works for all signed int bounds, unlike int subtraction across the full range.
    public static int Midpoint(int lo, int hi)
    {
        if (lo > hi) throw new ArgumentException("lo must not exceed hi.");
        return (int)((long)lo + ((long)hi - lo) / 2);
    }
    public static long Sum(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values); long sum = 0;
        foreach (int value in values) sum += value; return sum;
    }
    public static long Product(int a, int b) => (long)a * b;
    public static int CheckedIncrement(int value) => checked(value + 1);
    // Demonstrates guarding an infinity sentinel before arithmetic.
    public static long AddFiniteDistance(long distance, int weight)
    {
        if (weight < 0) throw new ArgumentOutOfRangeException(nameof(weight));
        if (distance == long.MaxValue) return long.MaxValue;
        return checked(distance + weight);
    }
}

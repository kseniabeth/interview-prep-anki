using System;
public static class Greedy
{
    // At most one buy then one sell. Empty/no profitable trade -> 0.
    public static long BestSingleTrade(int[] prices)
    {
        ArgumentNullException.ThrowIfNull(prices);
        long lowest = long.MaxValue, best = 0;
        foreach (int price in prices) { lowest = Math.Min(lowest,price); best = Math.Max(best,(long)price-lowest); }
        return best;
    }
    // Max NONEMPTY contiguous subarray sum. Empty input is invalid.
    public static long MaximumSubarray(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Length == 0) throw new ArgumentException("Nonempty array required.");
        long endingHere = values[0], best = values[0];
        for (int i = 1; i < values.Length; i++) { endingHere = Math.Max(values[i],endingHere + values[i]); best = Math.Max(best,endingHere); }
        return best;
    }
    // Each nonnegative entry is maximum forward jump. Empty input -> false.
    public static bool CanReachEnd(int[] jumps)
    {
        ArgumentNullException.ThrowIfNull(jumps);
        foreach (int jump in jumps) if (jump < 0) throw new ArgumentException("Nonnegative jumps required.");
        long farthest = 0;
        for (int i = 0; i < jumps.Length && i <= farthest; i++)
        {
            farthest = Math.Max(farthest,(long)i+jumps[i]);
            if (farthest >= jumps.Length-1) return true;
        }
        return false;
    }
}

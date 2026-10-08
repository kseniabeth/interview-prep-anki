using System;
using System.Collections.Generic;
public static class Windows
{
    public static long MaxFixedSum(int[] values, int k)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (k < 1 || k > values.Length) throw new ArgumentOutOfRangeException(nameof(k));
        long sum = 0, best = long.MinValue;
        for (int right = 0; right < values.Length; right++)
        {
            sum += values[right];
            if (right >= k) sum -= values[right - k];
            if (right >= k - 1) best = Math.Max(best, sum);
        }
        return best;
    }
    // UTF-16 code units; returns a length, not a substring or start index.
    public static int LongestAtMostKDistinct(string text, int k)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (k < 0) throw new ArgumentOutOfRangeException(nameof(k));
        var counts = new Dictionary<char, int>();
        int left = 0, best = 0;
        for (int right = 0; right < text.Length; right++)
        {
            counts[text[right]] = counts.GetValueOrDefault(text[right]) + 1;
            while (counts.Count > k)
            {
                char c = text[left++];
                if (--counts[c] == 0) counts.Remove(c);
            }
            best = Math.Max(best, right - left + 1);
        }
        return best;
    }
    // Shortest subarray with sum >= target. Requires strictly positive values.
    public static int MinPositiveLength(int[] values, long target)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (target <= 0) throw new ArgumentOutOfRangeException(nameof(target));
        foreach (int value in values) if (value <= 0) throw new ArgumentException("Positive values required.");
        int left = 0, best = int.MaxValue;
        long sum = 0;
        for (int right = 0; right < values.Length; right++)
        {
            sum += values[right];
            while (sum >= target)
            {
                best = Math.Min(best, right - left + 1);
                sum -= values[left++];
            }
        }
        return best == int.MaxValue ? 0 : best;
    }
}

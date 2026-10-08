using System;
using System.Collections.Generic;
public static class Hashing
{
    // Returns distinct original indices; null means no pair. Does not mutate input.
    public static (int Left, int Right)? TwoSum(int[] values, int target)
    {
        ArgumentNullException.ThrowIfNull(values);
        var firstIndex = new Dictionary<long, int>();
        for (int i = 0; i < values.Length; i++)
        {
            long needed = (long)target - values[i];
            if (firstIndex.TryGetValue(needed, out int j)) return (j, i);
            firstIndex.TryAdd(values[i], i);
        }
        return null;
    }
    // Compares UTF-16 code-unit frequencies, ordinally. Case and spaces matter.
    public static bool AreAnagrams(string a, string b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        if (a.Length != b.Length) return false;
        var counts = new Dictionary<char, int>();
        foreach (char c in a) counts[c] = counts.GetValueOrDefault(c) + 1;
        foreach (char c in b)
        {
            if (!counts.TryGetValue(c, out int count)) return false;
            if (count == 1) counts.Remove(c); else counts[c] = count - 1;
        }
        return counts.Count == 0;
    }
}

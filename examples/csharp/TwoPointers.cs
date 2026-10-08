using System;
public static class TwoPointers
{
    // Precondition: sorted ascending. Returns zero-based indices or null.
    public static (int Left, int Right)? SortedPair(int[] values, long target)
    {
        ArgumentNullException.ThrowIfNull(values);
        int left = 0, right = values.Length - 1;
        while (left < right)
        {
            long sum = (long)values[left] + values[right];
            if (sum == target) return (left, right);
            if (sum < target) left++; else right--;
        }
        return null;
    }
    // Exact ordinal UTF-16 palindrome: no filtering or case folding.
    public static bool IsPalindrome(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        int left = 0, right = text.Length - 1;
        while (left < right)
            if (text[left++] != text[right--]) return false;
        return true;
    }
    public static void Reverse(char[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        int left = 0, right = values.Length - 1;
        while (left < right)
        {
            (values[left], values[right]) = (values[right], values[left]);
            left++; right--;
        }
    }
    // Stable in-place compaction; prefix [0, returned length) contains nonzero values.
    public static int CompactNonzero(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        int write = 0;
        foreach (int value in values) if (value != 0) values[write++] = value;
        return write;
    }
}

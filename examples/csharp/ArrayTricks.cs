using System;
public static class ArrayTricks
{
    // Contract: Length=n+1, every value in 1..n, exactly one distinct duplicated value.
    // Does not modify input. Range validated; uniqueness assumption is not validated.
    public static int Duplicate(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Length < 2) throw new ArgumentException("At least two elements required.");
        foreach (int x in values) if (x < 1 || x >= values.Length) throw new ArgumentException("Values must be in 1..n.");
        int slow = values[0], fast = values[0];
        do { slow = values[slow]; fast = values[values[fast]]; } while (slow != fast);
        slow = values[0];
        while (slow != fast) { slow = values[slow]; fast = values[fast]; }
        return slow;
    }
    // Validates the candidate; returns null if no strict majority exists.
    public static int? Majority(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        int candidate = 0, balance = 0;
        foreach (int value in values)
        {
            if (balance == 0) candidate = value;
            balance += value == candidate ? 1 : -1;
        }
        int frequency = 0; foreach (int value in values) if (value == candidate) frequency++;
        return frequency > values.Length / 2 ? candidate : null;
    }
    // No division. Empty -> empty; singleton -> [1]. All intermediate products must fit long.
    public static long[] ProductExceptSelf(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var result = new long[values.Length]; long product = 1;
        for (int i = 0; i < values.Length; i++)
        {
            result[i] = product;
            if (i < values.Length-1) product = checked(product * values[i]);
        }
        product = 1;
        for (int i = values.Length-1; i >= 0; i--)
        {
            result[i] = checked(result[i] * product);
            if (i > 0) product = checked(product * values[i]);
        }
        return result;
    }
    // Every value appears twice except one. The contract is not validated.
    public static int SingleByXor(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        int answer = 0; foreach (int value in values) answer ^= value; return answer;
    }
    public static int SetBitCount(uint value)
    {
        int count = 0;
        while (value != 0) { value &= value - 1; count++; }
        return count;
    }
}

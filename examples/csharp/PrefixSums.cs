using System;
using System.Collections.Generic;
public static class PrefixSums
{
    // All inputs and target are int; long safely holds sums/counts for int-indexed arrays.
    public static long CountTargetSum(int[] values, int target)
    {
        ArgumentNullException.ThrowIfNull(values);
        var counts = new Dictionary<long, long> { [0] = 1 };
        long sum = 0, answer = 0;
        foreach (int value in values)
        {
            sum += value;
            answer += counts.GetValueOrDefault(sum - target);
            counts[sum] = counts.GetValueOrDefault(sum) + 1;
        }
        return answer;
    }
    // prefix[i] sums values[0..i); query [left,right) as prefix[right]-prefix[left].
    public static long[] Build(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var prefix = new long[checked(values.Length + 1)];
        for (int i = 0; i < values.Length; i++) prefix[i + 1] = prefix[i] + values[i];
        return prefix;
    }
}

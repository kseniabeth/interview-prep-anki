using System;
public static class BinarySearch
{
    // Sorted ascending; any matching zero-based index, otherwise -1.
    public static int Exact(int[] values, int target)
    {
        ArgumentNullException.ThrowIfNull(values);
        int lo = 0, hi = values.Length - 1;
        while (lo <= hi)
        {
            int mid = lo + (hi - lo) / 2;
            if (values[mid] == target) return mid;
            if (values[mid] < target) lo = mid + 1; else hi = mid - 1;
        }
        return -1;
    }
    // Sorted ascending; returns first index with value >= target, or Length.
    public static int LowerBound(int[] values, int target)
    {
        ArgumentNullException.ThrowIfNull(values);
        int lo = 0, hi = values.Length;
        while (lo < hi)
        {
            int mid = lo + (hi - lo) / 2;
            if (values[mid] >= target) hi = mid; else lo = mid + 1;
        }
        return lo;
    }
    // Pred must be false then true over [0,n); return n when all false.
    public static int FirstTrue(int n, Func<int, bool> pred)
    {
        if (n < 0) throw new ArgumentOutOfRangeException(nameof(n));
        ArgumentNullException.ThrowIfNull(pred);
        int lo = 0, hi = n;
        while (lo < hi)
        {
            int mid = lo + (hi - lo) / 2;
            if (pred(mid)) hi = mid; else lo = mid + 1;
        }
        return lo;
    }
    // Distinct values, sorted then rotated. Duplicates need a different ambiguity case.
    public static int Rotated(int[] values, int target)
    {
        ArgumentNullException.ThrowIfNull(values);
        int lo = 0, hi = values.Length - 1;
        while (lo <= hi)
        {
            int mid = lo + (hi - lo) / 2;
            if (values[mid] == target) return mid;
            if (values[lo] <= values[mid])
            {
                if (values[lo] <= target && target < values[mid]) hi = mid - 1;
                else lo = mid + 1;
            }
            else
            {
                if (values[mid] < target && target <= values[hi]) lo = mid + 1;
                else hi = mid - 1;
            }
        }
        return -1;
    }
    // Ordered positive packages, cannot split/reorder; empty input needs capacity 0.
    public static long MinimumShippingCapacity(int[] weights, int days)
    {
        ArgumentNullException.ThrowIfNull(weights);
        if (days < 1) throw new ArgumentOutOfRangeException(nameof(days));
        long lo = 0, hi = 0;
        foreach (int w in weights)
        {
            if (w <= 0) throw new ArgumentException("Positive weights required.");
            lo = Math.Max(lo, w); hi += w;
        }
        bool Feasible(long capacity)
        {
            long load = 0; int usedDays = 1;
            foreach (int w in weights)
            {
                if (load + w > capacity) { if (++usedDays > days) return false; load = 0; }
                load += w;
            }
            return true;
        }
        while (lo < hi)
        {
            long mid = lo + (hi - lo) / 2;
            if (Feasible(mid)) hi = mid; else lo = mid + 1;
        }
        return lo;
    }

    // Positive piles, one pile per hour. -1 when hours < number of piles.
    public static int MinimumEatingSpeed(int[] piles, int hours)
    {
        ArgumentNullException.ThrowIfNull(piles);
        if (hours < 0) throw new ArgumentOutOfRangeException(nameof(hours));
        int hi = 0;
        foreach (int pile in piles) { if (pile <= 0) throw new ArgumentException("Positive piles required."); hi = Math.Max(hi,pile); }
        if (piles.Length == 0) return 0;
        if (hours < piles.Length) return -1;
        int lo = 1;
        bool Feasible(int speed)
        {
            long used = 0;
            foreach (int pile in piles) { used += ((long)pile + speed - 1) / speed; if (used > hours) return false; }
            return true;
        }
        while (lo < hi) { int mid = lo + (hi-lo)/2; if (Feasible(mid)) hi = mid; else lo = mid+1; }
        return lo;
    }
    // Exactly groups nonempty contiguous parts; nonnegative values; 1 <= groups <= Length.
    public static long MinimumLargestSplit(int[] values, int groups)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (groups < 1 || groups > values.Length) throw new ArgumentOutOfRangeException(nameof(groups));
        long lo = 0, hi = 0;
        foreach (int value in values) { if (value < 0) throw new ArgumentException("Nonnegative values required."); lo = Math.Max(lo,value); hi += value; }
        bool Feasible(long cap)
        {
            int used = 1; long sum = 0;
            foreach (int value in values) { if (sum+value > cap) { used++; sum=0; if (used > groups) return false; } sum+=value; }
            return true;
        }
        while (lo < hi) { long mid = lo + (hi-lo)/2; if (Feasible(mid)) hi=mid; else lo=mid+1; }
        return lo;
    }

}

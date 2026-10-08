using System;
using System.Collections.Generic;
public static class Heaps
{
    public static int KthLargest(int[] values, int k)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (k < 1 || k > values.Length) throw new ArgumentOutOfRangeException(nameof(k));
        var heap = new PriorityQueue<int, int>();
        foreach (int value in values)
        {
            heap.Enqueue(value, value);
            if (heap.Count > k) heap.Dequeue();
        }
        return heap.Peek();
    }
    // Each input array sorted ascending. Does not mutate inputs; empty arrays allowed.
    public static int[] MergeSortedArrays(int[][] arrays)
    {
        ArgumentNullException.ThrowIfNull(arrays);
        var heap = new PriorityQueue<(int Array, int Index), int>();
        for (int i = 0; i < arrays.Length; i++)
        {
            ArgumentNullException.ThrowIfNull(arrays[i]);
            if (arrays[i].Length > 0) heap.Enqueue((i, 0), arrays[i][0]);
        }
        var result = new List<int>();
        while (heap.TryDequeue(out var entry, out int value))
        {
            result.Add(value);
            int next = entry.Index + 1;
            if (next < arrays[entry.Array].Length)
                heap.Enqueue((entry.Array, next), arrays[entry.Array][next]);
        }
        return result.ToArray();
    }
    // Result order unspecified. Ties may choose any value; k cannot exceed distinct count.
    public static int[] TopFrequent(int[] values, int k)
    {
        ArgumentNullException.ThrowIfNull(values);
        var counts = new Dictionary<int, int>();
        foreach (int value in values) counts[value] = counts.GetValueOrDefault(value) + 1;
        if (k < 0 || k > counts.Count) throw new ArgumentOutOfRangeException(nameof(k));
        var heap = new PriorityQueue<int, int>();
        foreach (var pair in counts)
        {
            heap.Enqueue(pair.Key, pair.Value);
            if (heap.Count > k) heap.Dequeue();
        }
        var result = new int[k];
        for (int i = 0; i < k; i++) result[i] = heap.Dequeue();
        return result;
    }
    public sealed class RunningMedian
    {
        private readonly PriorityQueue<int, int> lower = new(Comparer<int>.Create((a, b) => b.CompareTo(a)));
        private readonly PriorityQueue<int, int> upper = new();
        public void Add(int value)
        {
            if (lower.Count == 0 || value <= lower.Peek()) lower.Enqueue(value, value);
            else upper.Enqueue(value, value);
            if (lower.Count > upper.Count + 1) { int x = lower.Dequeue(); upper.Enqueue(x, x); }
            if (upper.Count > lower.Count) { int x = upper.Dequeue(); lower.Enqueue(x, x); }
        }
        public double Median()
        {
            if (lower.Count == 0) throw new InvalidOperationException("No values yet.");
            return lower.Count > upper.Count ? lower.Peek() : ((long)lower.Peek() + upper.Peek()) / 2.0;
        }
    }

    // Returns k smallest values in unspecified order; duplicates count separately.
    public static int[] KSmallest(int[] values, int k)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (k < 0 || k > values.Length) throw new ArgumentOutOfRangeException(nameof(k));
        var heap = new PriorityQueue<int,int>(Comparer<int>.Create((a,b)=>b.CompareTo(a)));
        foreach (int value in values) { heap.Enqueue(value,value); if (heap.Count > k) heap.Dequeue(); }
        var answer = new int[k]; for (int i = 0; i < k; i++) answer[i] = heap.Dequeue(); return answer;
    }

}

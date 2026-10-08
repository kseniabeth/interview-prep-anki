using System;
using System.Collections.Generic;
public static class CollectionApis
{
    public static int MinHeapRoot()
    {
        var heap = new PriorityQueue<string,int>();
        heap.Enqueue("later",5); heap.Enqueue("earlier",1);
        if (!heap.TryDequeue(out string? item,out int priority) || item != "earlier") throw new InvalidOperationException();
        return priority;
    }
    public static int MaxHeapRoot(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Length == 0) throw new ArgumentException("Nonempty input required.");
        var heap = new PriorityQueue<int,int>(Comparer<int>.Create((a,b) => b.CompareTo(a)));
        foreach (int value in values) heap.Enqueue(value,value);
        return heap.Peek();
    }
    public static Dictionary<char,int> Frequencies(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var counts = new Dictionary<char,int>();
        foreach (char c in text)
        {
            counts.TryGetValue(c,out int previous);
            counts[c] = previous + 1;
        }
        return counts;
    }
    public static int QueueOrder()
    {
        var queue = new Queue<int>(); queue.Enqueue(10); queue.Enqueue(20);
        int first = queue.Peek();
        if (queue.Count != 2 || queue.Dequeue() != first) throw new InvalidOperationException();
        return queue.TryDequeue(out int next) ? next : -1;
    }
    public static int StackOrder()
    {
        var stack = new Stack<int>(); stack.Push(10); stack.Push(20);
        int top = stack.Peek();
        if (stack.Count != 2 || stack.Pop() != top) throw new InvalidOperationException();
        return stack.TryPop(out int next) ? next : -1;
    }
    public static int DequeOrder()
    {
        var deque = new LinkedList<int>(); deque.AddLast(2); deque.AddFirst(1); deque.AddLast(3);
        deque.RemoveFirst(); deque.RemoveLast(); return deque.First!.Value;
    }
    public static int DistinctCount(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var seen = new HashSet<int>();
        foreach (int value in values) { bool newlyAdded = seen.Add(value); _ = newlyAdded; }
        return seen.Count;
    }
    // A snapshot avoids dependence on collection/version-specific mutation rules.
    public static void RemoveNonpositive(Dictionary<string,int> counts)
    {
        ArgumentNullException.ThrowIfNull(counts);
        var keys = new List<string>(counts.Keys);
        foreach (string key in keys) if (counts[key] <= 0) counts.Remove(key);
    }
    public static int[] OrderedUnique(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var sorted = new SortedSet<int>(values); var result = new int[sorted.Count]; sorted.CopyTo(result); return result;
    }
    public static SortedDictionary<string,int> OrderedMap()
    {
        return new SortedDictionary<string,int>(StringComparer.Ordinal) { ["beta"] = 2, ["alpha"] = 1 };
    }
}

using System;
using System.Collections.Generic;
public static class Caches
{
    // Single-threaded interview cache. Capacity zero stores nothing.
    public sealed class Lru
    {
        private readonly int capacity;
        private readonly Dictionary<int,LinkedListNode<(int Key,int Value)>> index = new();
        private readonly LinkedList<(int Key,int Value)> recency = new();
        public Lru(int capacity) { if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity)); this.capacity = capacity; }
        public bool TryGet(int key, out int value)
        {
            if (!index.TryGetValue(key,out var node)) { value = default; return false; }
            recency.Remove(node); recency.AddFirst(node); value = node.Value.Value; return true;
        }
        public void Put(int key, int value)
        {
            if (capacity == 0) return;
            if (index.TryGetValue(key,out var node))
            {
                node.Value = (key,value); recency.Remove(node); recency.AddFirst(node); return;
            }
            node = recency.AddFirst((key,value)); index.Add(key,node);
            if (index.Count <= capacity) return;
            var evicted = recency.Last!; index.Remove(evicted.Value.Key); recency.RemoveLast();
        }
    }
    public sealed class MinStack
    {
        private readonly Stack<(int Value,int Min)> values = new();
        public int Count => values.Count;
        public void Push(int value) => values.Push((value,values.Count == 0 ? value : Math.Min(value,values.Peek().Min)));
        public int Pop() => values.Pop().Value;
        public int Peek() => values.Peek().Value;
        public int Minimum() => values.Peek().Min;
    }
}

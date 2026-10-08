using System;
using System.Collections.Generic;
public static class Graphs
{
    // Each edge is (prerequisite, dependent). Vertices are exactly 0..n-1.
    // Returns null for any directed cycle, otherwise all n vertices in valid order.
    public static int[]? TopologicalOrder(int n, (int From, int To)[] edges)
    {
        if (n < 0) throw new ArgumentOutOfRangeException(nameof(n));
        ArgumentNullException.ThrowIfNull(edges);
        var adjacent = new List<int>[n]; var indegree = new int[n];
        for (int i = 0; i < n; i++) adjacent[i] = new List<int>();
        foreach (var e in edges)
        {
            if (e.From < 0 || e.From >= n || e.To < 0 || e.To >= n) throw new ArgumentException("Invalid vertex.");
            adjacent[e.From].Add(e.To); indegree[e.To]++;
        }
        var queue = new Queue<int>();
        for (int i = 0; i < n; i++) if (indegree[i] == 0) queue.Enqueue(i);
        var order = new List<int>();
        while (queue.TryDequeue(out int u))
        {
            order.Add(u);
            foreach (int v in adjacent[u]) if (--indegree[v] == 0) queue.Enqueue(v);
        }
        return order.Count == n ? order.ToArray() : null;
    }
    // Nonnegative INT edge weights; directed edges; unreachable = long.MaxValue.
    // Strict relaxation + stale-entry skipping allows zero-weight edges/cycles.
    public static long[] Dijkstra(int n, (int From, int To, int Weight)[] edges, int source)
    {
        if (n < 1 || source < 0 || source >= n) throw new ArgumentOutOfRangeException(nameof(source));
        ArgumentNullException.ThrowIfNull(edges);
        var adjacent = new List<(int To, int Weight)>[n];
        for (int i = 0; i < n; i++) adjacent[i] = new List<(int, int)>();
        foreach (var e in edges)
        {
            if (e.From < 0 || e.From >= n || e.To < 0 || e.To >= n || e.Weight < 0) throw new ArgumentException("Invalid edge.");
            adjacent[e.From].Add((e.To,e.Weight));
        }
        var distance = new long[n]; Array.Fill(distance, long.MaxValue); distance[source] = 0;
        var heap = new PriorityQueue<int, long>(); heap.Enqueue(source, 0);
        while (heap.TryDequeue(out int u, out long poppedDistance))
        {
            if (poppedDistance != distance[u]) continue;
            foreach (var edge in adjacent[u])
            {
                long candidate = checked(poppedDistance + edge.Weight);
                if (candidate >= distance[edge.To]) continue;
                distance[edge.To] = candidate; heap.Enqueue(edge.To, candidate);
            }
        }
        return distance;
    }
    // All connected components, including isolated vertices. Undirected edges.
    public static int Components(int n, (int A, int B)[] edges)
    {
        if (n < 0) throw new ArgumentOutOfRangeException(nameof(n));
        ArgumentNullException.ThrowIfNull(edges);
        var adjacent = new List<int>[n];
        for (int i = 0; i < n; i++) adjacent[i] = new List<int>();
        foreach (var e in edges)
        {
            if (e.A < 0 || e.A >= n || e.B < 0 || e.B >= n) throw new ArgumentException("Invalid vertex.");
            adjacent[e.A].Add(e.B); adjacent[e.B].Add(e.A);
        }
        var seen = new bool[n]; var stack = new Stack<int>(); int count = 0;
        for (int start = 0; start < n; start++)
        {
            if (seen[start]) continue;
            count++; seen[start] = true; stack.Push(start);
            while (stack.TryPop(out int u))
                foreach (int v in adjacent[u]) if (!seen[v]) { seen[v] = true; stack.Push(v); }
        }
        return count;
    }
}

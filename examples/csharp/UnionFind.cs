using System;
public sealed class UnionFind
{
    private readonly int[] parent;
    private readonly int[] size;
    public int Components { get; private set; }
    public UnionFind(int n)
    {
        if (n < 0) throw new ArgumentOutOfRangeException(nameof(n));
        parent = new int[n]; size = new int[n]; Components = n;
        for (int i = 0; i < n; i++) { parent[i] = i; size[i] = 1; }
    }
    public int Find(int x)
    {
        if (x < 0 || x >= parent.Length) throw new ArgumentOutOfRangeException(nameof(x));
        while (x != parent[x]) { parent[x] = parent[parent[x]]; x = parent[x]; }
        return x;
    }
    // False means already connected. With undirected edge insertion, this closes a cycle.
    public bool Union(int a, int b)
    {
        int ra = Find(a), rb = Find(b);
        if (ra == rb) return false;
        if (size[ra] < size[rb]) (ra,rb) = (rb,ra);
        parent[rb] = ra; size[ra] += size[rb]; Components--;
        return true;
    }
}

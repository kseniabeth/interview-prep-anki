using System;
using System.Collections.Generic;
public static class GridTraversal
{
    private static readonly (int R, int C)[] Directions = { (1,0), (-1,0), (0,1), (0,-1) };
    private static int Width<T>(T[][] grid)
    {
        ArgumentNullException.ThrowIfNull(grid);
        int width = grid.Length == 0 ? 0 : (grid[0] ?? throw new ArgumentException("Null row.")).Length;
        foreach (var row in grid) if (row == null || row.Length != width) throw new ArgumentException("Rectangular grid required.");
        return width;
    }
    // 0 = open, all other values = blocked. Returns NUMBER OF EDGES, or -1.
    // Does not mutate grid. A source equal to target returns 0 if it is open.
    public static int ShortestPath(int[][] grid, (int R, int C) source, (int R, int C) target)
    {
        int columns = Width(grid), rows = grid.Length;
        bool Inside(int r, int c) => r >= 0 && r < rows && c >= 0 && c < columns;
        if (!Inside(source.R, source.C) || !Inside(target.R, target.C)) return -1;
        if (grid[source.R][source.C] != 0 || grid[target.R][target.C] != 0) return -1;
        var seen = new bool[rows, columns];
        var queue = new Queue<(int R, int C)>();
        seen[source.R, source.C] = true;
        queue.Enqueue(source);
        int distance = 0;
        while (queue.Count > 0)
        {
            int levelSize = queue.Count;
            for (int i = 0; i < levelSize; i++)
            {
                var cell = queue.Dequeue();
                if (cell == target) return distance;
                foreach (var d in Directions)
                {
                    int r = cell.R + d.R, c = cell.C + d.C;
                    if (!Inside(r, c) || seen[r, c] || grid[r][c] != 0) continue;
                    seen[r, c] = true;
                    queue.Enqueue((r, c));
                }
            }
            distance++;
        }
        return -1;
    }
    // Mutates land '1' to water '0'. Iterative DFS avoids deep recursion.
    public static int Islands(char[][] grid)
    {
        int columns = Width(grid), rows = grid.Length, islands = 0;
        var pending = new Stack<(int R, int C)>();
        for (int r = 0; r < rows; r++) for (int c = 0; c < columns; c++)
        {
            if (grid[r][c] != '1') continue;
            islands++; grid[r][c] = '0'; pending.Push((r,c));
            while (pending.TryPop(out var cell))
                foreach (var d in Directions)
                {
                    int nr = cell.R + d.R, nc = cell.C + d.C;
                    if (nr < 0 || nr >= rows || nc < 0 || nc >= columns || grid[nr][nc] != '1') continue;
                    grid[nr][nc] = '0'; pending.Push((nr,nc));
                }
        }
        return islands;
    }
    // Exact recursive template, for shallow grids only. Mutates visited land.
    public static void RecursiveFloodFill(char[][] grid, int startRow, int startColumn)
    {
        int columns = Width(grid), rows = grid.Length;
        void Visit(int r, int c)
        {
            if (r < 0 || r >= rows || c < 0 || c >= columns || grid[r][c] != '1') return;
            grid[r][c] = '0';
            Visit(r + 1,c); Visit(r - 1,c); Visit(r,c + 1); Visit(r,c - 1);
        }
        Visit(startRow, startColumn);
    }
    // 0 empty, 1 fresh, 2 rotten. Mutates grid. -1 if fresh cells are unreachable.
    public static int RottingMinutes(int[][] grid)
    {
        int columns = Width(grid), rows = grid.Length, fresh = 0, minutes = 0;
        var queue = new Queue<(int R, int C)>();
        for (int r = 0; r < rows; r++) for (int c = 0; c < columns; c++)
        {
            if (grid[r][c] == 2) queue.Enqueue((r,c));
            else if (grid[r][c] == 1) fresh++;
        }
        while (queue.Count > 0 && fresh > 0)
        {
            int levelSize = queue.Count;
            for (int i = 0; i < levelSize; i++)
            {
                var cell = queue.Dequeue();
                foreach (var d in Directions)
                {
                    int r = cell.R + d.R, c = cell.C + d.C;
                    if (r < 0 || r >= rows || c < 0 || c >= columns || grid[r][c] != 1) continue;
                    grid[r][c] = 2; fresh--; queue.Enqueue((r,c));
                }
            }
            minutes++;
        }
        return fresh == 0 ? minutes : -1;
    }
}

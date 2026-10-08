using System;
using System.Collections.Generic;
public static class Matrices
{
    private static int Width(int[][] grid)
    {
        ArgumentNullException.ThrowIfNull(grid);
        int n = grid.Length == 0 ? 0 : (grid[0] ?? throw new ArgumentException("Null row.")).Length;
        foreach (var row in grid) if (row == null || row.Length != n) throw new ArgumentException("Rectangular grid required.");
        return n;
    }
    public static int[] Spiral(int[][] grid)
    {
        int n = Width(grid); var result = new List<int>();
        int top = 0, bottom = grid.Length-1, left = 0, right = n-1;
        while (top <= bottom && left <= right)
        {
            for (int c = left; c <= right; c++) result.Add(grid[top][c]); top++;
            for (int r = top; r <= bottom; r++) result.Add(grid[r][right]); right--;
            if (top <= bottom) { for (int c = right; c >= left; c--) result.Add(grid[bottom][c]); bottom--; }
            if (left <= right) { for (int r = bottom; r >= top; r--) result.Add(grid[r][left]); left++; }
        }
        return result.ToArray();
    }
    // Mutates SQUARE grid, clockwise rotation.
    public static void RotateClockwise(int[][] grid)
    {
        int n = Width(grid); if (grid.Length != n) throw new ArgumentException("Square grid required.");
        for (int r = 0; r < n; r++) for (int c = r+1; c < n; c++) (grid[r][c],grid[c][r]) = (grid[c][r],grid[r][c]);
        foreach (var row in grid) Array.Reverse(row);
    }
    // Mutates grid. O(1) extra space, marking original zero rows/columns.
    public static void SetZeroes(int[][] grid)
    {
        int n = Width(grid), m = grid.Length; if (m == 0 || n == 0) return;
        bool firstRow = false, firstColumn = false;
        for (int c = 0; c < n; c++) if (grid[0][c] == 0) firstRow = true;
        for (int r = 0; r < m; r++) if (grid[r][0] == 0) firstColumn = true;
        for (int r = 1; r < m; r++) for (int c = 1; c < n; c++) if (grid[r][c] == 0) { grid[r][0] = 0; grid[0][c] = 0; }
        for (int r = 1; r < m; r++) for (int c = 1; c < n; c++) if (grid[r][0] == 0 || grid[0][c] == 0) grid[r][c] = 0;
        if (firstRow) Array.Fill(grid[0],0);
        if (firstColumn) for (int r = 0; r < m; r++) grid[r][0] = 0;
    }
}

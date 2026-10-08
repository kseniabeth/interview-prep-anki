using System;
using System.Collections.Generic;
public static class Backtracking
{
    // Input positions treated as distinct. Duplicate values can produce duplicate subsets.
    public static List<int[]> Subsets(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var answer = new List<int[]>(); var path = new List<int>();
        void Visit(int start)
        {
            answer.Add(path.ToArray());
            for (int i = start; i < values.Length; i++)
            {
                path.Add(values[i]); Visit(i + 1); path.RemoveAt(path.Count - 1);
            }
        }
        Visit(0); return answer;
    }
    // n pairs; Catalan output growth, use only small n. n=0 yields one empty string.
    public static List<string> Parentheses(int n)
    {
        if (n < 0) throw new ArgumentOutOfRangeException(nameof(n));
        var answer = new List<string>(); var buffer = new char[checked(2 * n)];
        void Visit(int open, int close)
        {
            int index = open + close;
            if (index == buffer.Length) { answer.Add(new string(buffer)); return; }
            if (open < n) { buffer[index] = '('; Visit(open + 1,close); }
            if (close < open) { buffer[index] = ')'; Visit(open,close + 1); }
        }
        Visit(0,0); return answer;
    }
    // Rectangular board, orthogonal moves, no cell reuse. Preserves board.
    public static bool WordExists(char[][] board, string word)
    {
        ArgumentNullException.ThrowIfNull(board); ArgumentNullException.ThrowIfNull(word);
        int rows = board.Length, columns = rows == 0 ? 0 : (board[0] ?? throw new ArgumentException("Null row.")).Length;
        foreach (var row in board) if (row == null || row.Length != columns) throw new ArgumentException("Rectangular board required.");
        if (word.Length == 0) return true;
        if ((long)rows * columns < word.Length) return false;
        var used = new bool[rows,columns];
        bool Search(int r, int c, int index)
        {
            if (r < 0 || r >= rows || c < 0 || c >= columns || used[r,c] || board[r][c] != word[index]) return false;
            if (index == word.Length - 1) return true;
            used[r,c] = true;
            bool found = Search(r+1,c,index+1) || Search(r-1,c,index+1) || Search(r,c+1,index+1) || Search(r,c-1,index+1);
            used[r,c] = false;
            return found;
        }
        for (int r = 0; r < rows; r++) for (int c = 0; c < columns; c++) if (Search(r,c,0)) return true;
        return false;
    }

    // Input positions distinct; equal input values may produce duplicate value sequences.
    public static List<int[]> Permutations(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var answer = new List<int[]>(); var used = new bool[values.Length]; var path = new int[values.Length];
        void Visit(int depth)
        {
            if (depth == values.Length) { answer.Add((int[])path.Clone()); return; }
            for (int i = 0; i < values.Length; i++)
            {
                if (used[i]) continue; used[i] = true; path[depth] = values[i]; Visit(depth+1); used[i] = false;
            }
        }
        Visit(0); return answer;
    }
    // Choose k positions in increasing index order. k > Length yields no combinations.
    public static List<int[]> Combinations(int[] values, int k)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (k < 0) throw new ArgumentOutOfRangeException(nameof(k));
        var answer = new List<int[]>(); if (k > values.Length) return answer;
        var path = new List<int>();
        void Visit(int start)
        {
            if (path.Count == k) { answer.Add(path.ToArray()); return; }
            int needed = k - path.Count;
            for (int i = start; i <= values.Length-needed; i++) { path.Add(values[i]); Visit(i+1); path.RemoveAt(path.Count-1); }
        }
        Visit(0); return answer;
    }

}

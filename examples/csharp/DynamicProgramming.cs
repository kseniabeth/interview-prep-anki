using System;
using System.Collections.Generic;
using System.Numerics;
public static class DynamicProgramming
{
    // Ways to reach n using steps 1 or 2. One empty way for n=0; arbitrary precision.
    public static BigInteger StairWays(int n)
    {
        if (n < 0) throw new ArgumentOutOfRangeException(nameof(n));
        BigInteger previous = 1, current = 1;
        for (int i = 2; i <= n; i++) (previous,current) = (current,previous + current);
        return current;
    }
    // May take no item. Empty/all-negative input returns 0; no adjacent selections.
    public static long Rob(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        long twoBack = 0, oneBack = 0;
        foreach (int value in values) { long best = Math.Max(oneBack,twoBack + value); twoBack = oneBack; oneBack = best; }
        return oneBack;
    }
    // Nonnegative costs. Start at 0 or 1, pay when leaving, move 1 or 2, exit at Length.
    public static long MinStairCost(int[] cost)
    {
        ArgumentNullException.ThrowIfNull(cost);
        foreach (int value in cost) if (value < 0) throw new ArgumentException("Nonnegative costs required.");
        long twoBack = 0, oneBack = 0;
        for (int i = 2; i <= cost.Length; i++)
        {
            long next = Math.Min(oneBack + cost[i-1],twoBack + cost[i-2]);
            twoBack = oneBack; oneBack = next;
        }
        return oneBack;
    }
    // Positive distinct or repeated denominations, unlimited reuse; -1 if impossible.
    public static int FewestCoins(int[] coins, int amount)
    {
        ArgumentNullException.ThrowIfNull(coins);
        if (amount < 0 || amount == int.MaxValue) throw new ArgumentOutOfRangeException(nameof(amount));
        foreach (int coin in coins) if (coin <= 0) throw new ArgumentException("Positive coins required.");
        var dp = new int[amount + 1]; Array.Fill(dp,amount + 1); dp[0] = 0;
        for (int total = 1; total <= amount; total++)
            foreach (int coin in coins) if (coin <= total) dp[total] = Math.Min(dp[total],dp[total-coin] + 1);
        return dp[amount] > amount ? -1 : dp[amount];
    }
    // Each positive item can be used at most once; empty subset reaches zero.
    public static bool SubsetSumOnce(int[] values, int target)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (target < 0 || target == int.MaxValue) throw new ArgumentOutOfRangeException(nameof(target));
        var possible = new bool[target + 1]; possible[0] = true;
        foreach (int value in values)
        {
            if (value <= 0) throw new ArgumentException("Positive items required.");
            for (int sum = target; sum >= value; sum--) possible[sum] |= possible[sum-value];
        }
        return possible[target];
    }
    // STRICT increasing subsequence length; tails is not necessarily an actual subsequence.
    public static int LisLength(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var tails = new int[values.Length]; int length = 0;
        foreach (int value in values)
        {
            int lo = 0, hi = length;
            while (lo < hi) { int mid = lo + (hi-lo)/2; if (tails[mid] >= value) hi = mid; else lo = mid+1; }
            tails[lo] = value; if (lo == length) length++;
        }
        return length;
    }
    // Unit-cost insertion, deletion, replacement; UTF-16 code units.
    public static int EditDistance(string a, string b)
    {
        ArgumentNullException.ThrowIfNull(a); ArgumentNullException.ThrowIfNull(b);
        var dp = new int[checked(a.Length+1),checked(b.Length+1)];
        for (int i = 0; i <= a.Length; i++) dp[i,0] = i;
        for (int j = 0; j <= b.Length; j++) dp[0,j] = j;
        for (int i = 1; i <= a.Length; i++) for (int j = 1; j <= b.Length; j++)
            dp[i,j] = a[i-1] == b[j-1] ? dp[i-1,j-1] : 1 + Math.Min(dp[i-1,j-1],Math.Min(dp[i-1,j],dp[i,j-1]));
        return dp[a.Length,b.Length];
    }
    public static int MemoizedLcs(string a, string b)
    {
        ArgumentNullException.ThrowIfNull(a); ArgumentNullException.ThrowIfNull(b);
        var memo = new Dictionary<(int I,int J),int>();
        int Solve(int i, int j)
        {
            if (i == a.Length || j == b.Length) return 0;
            if (memo.TryGetValue((i,j),out int value)) return value;
            return memo[(i,j)] = a[i] == b[j] ? 1 + Solve(i+1,j+1) : Math.Max(Solve(i+1,j),Solve(i,j+1));
        }
        return Solve(0,0);
    }
    // Right/down moves, no obstacles; zero dimension -> 0. BigInteger avoids count overflow.
    public static BigInteger UniquePaths(int rows, int columns)
    {
        if (rows < 0 || columns < 0) throw new ArgumentOutOfRangeException(nameof(rows));
        if (rows == 0 || columns == 0) return 0;
        var dp = new BigInteger[columns]; dp[0] = 1;
        for (int r = 0; r < rows; r++) for (int c = 1; c < columns; c++) dp[c] += dp[c-1];
        return dp[columns-1];
    }

    // Preserve order within a and b; UTF-16 code-unit contract.
    public static bool IsInterleaving(string a, string b, string target)
    {
        ArgumentNullException.ThrowIfNull(a); ArgumentNullException.ThrowIfNull(b); ArgumentNullException.ThrowIfNull(target);
        if ((long)a.Length + b.Length != target.Length) return false;
        var dp = new bool[a.Length+1,b.Length+1]; dp[0,0] = true;
        for (int i = 0; i <= a.Length; i++) for (int j = 0; j <= b.Length; j++)
        {
            if (i > 0) dp[i,j] |= dp[i-1,j] && a[i-1] == target[i+j-1];
            if (j > 0) dp[i,j] |= dp[i,j-1] && b[j-1] == target[i+j-1];
        }
        return dp[a.Length,b.Length];
    }
    // Right/down only; rectangular nonempty grid; includes start and destination costs.
    public static long MinimumPathSum(int[][] grid)
    {
        ArgumentNullException.ThrowIfNull(grid);
        if (grid.Length == 0 || grid[0] == null || grid[0].Length == 0) throw new ArgumentException("Nonempty grid required.");
        int columns = grid[0].Length;
        foreach (var row in grid) if (row == null || row.Length != columns) throw new ArgumentException("Rectangular grid required.");
        var dp = new long[columns];
        for (int r = 0; r < grid.Length; r++) for (int c = 0; c < columns; c++)
        {
            if (r == 0 && c == 0) dp[c] = grid[r][c];
            else if (r == 0) dp[c] = dp[c-1] + grid[r][c];
            else if (c == 0) dp[c] += grid[r][c];
            else dp[c] = Math.Min(dp[c],dp[c-1]) + grid[r][c];
        }
        return dp[columns-1];
    }

}

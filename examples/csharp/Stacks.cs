using System;
using System.Collections.Generic;
public static class Stacks
{
    // Returns next STRICTLY greater VALUES, not indices or distances; -1 means absent.
    public static int[] NextGreaterValues(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var answer = new int[values.Length];
        Array.Fill(answer, -1);
        var pending = new Stack<int>();
        for (int i = 0; i < values.Length; i++)
        {
            while (pending.Count > 0 && values[pending.Peek()] < values[i])
                answer[pending.Pop()] = values[i];
            pending.Push(i);
        }
        return answer;
    }
    // Any non-bracket character is invalid under this contract.
    public static bool ValidBrackets(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var pending = new Stack<char>();
        foreach (char c in text)
        {
            if (c is '(' or '[' or '{') { pending.Push(c); continue; }
            char expected = c switch { ')' => '(', ']' => '[', '}' => '{', _ => '\0' };
            if (expected == '\0' || !pending.TryPop(out char open) || open != expected) return false;
        }
        return pending.Count == 0;
    }
    // Nonnegative heights. Returned area is long; virtual final zero flushes the stack.
    public static long LargestRectangle(int[] heights)
    {
        ArgumentNullException.ThrowIfNull(heights);
        foreach (int h in heights) if (h < 0) throw new ArgumentException("Nonnegative heights required.");
        var pending = new Stack<int>();
        long best = 0;
        for (int i = 0; i <= heights.Length; i++)
        {
            int height = i == heights.Length ? 0 : heights[i];
            while (pending.Count > 0 && heights[pending.Peek()] > height)
            {
                int h = heights[pending.Pop()];
                int left = pending.Count == 0 ? -1 : pending.Peek();
                best = Math.Max(best, (long)h * (i - left - 1));
            }
            if (i < heights.Length) pending.Push(i);
        }
        return best;
    }
    public static long TrappedWater(int[] heights)
    {
        ArgumentNullException.ThrowIfNull(heights);
        foreach (int h in heights) if (h < 0) throw new ArgumentException("Nonnegative heights required.");
        int left = 0, right = heights.Length - 1, leftMax = 0, rightMax = 0;
        long water = 0;
        while (left <= right)
        {
            if (leftMax <= rightMax)
            {
                leftMax = Math.Max(leftMax, heights[left]);
                water += leftMax - heights[left++];
            }
            else
            {
                rightMax = Math.Max(rightMax, heights[right]);
                water += rightMax - heights[right--];
            }
        }
        return water;
    }

    // Distance to next strictly warmer day; 0 means no warmer future day.
    public static int[] DaysUntilWarmer(int[] temperatures)
    {
        ArgumentNullException.ThrowIfNull(temperatures);
        var answer = new int[temperatures.Length]; var pending = new Stack<int>();
        for (int i = 0; i < temperatures.Length; i++)
        {
            while (pending.Count > 0 && temperatures[pending.Peek()] < temperatures[i])
            { int previous = pending.Pop(); answer[previous] = i - previous; }
            pending.Push(i);
        }
        return answer;
    }
    // Consecutive days ending today whose price is <= today's, including today.
    public static int[] StockSpans(int[] prices)
    {
        ArgumentNullException.ThrowIfNull(prices);
        var answer = new int[prices.Length]; var greater = new Stack<int>();
        for (int i = 0; i < prices.Length; i++)
        {
            while (greater.Count > 0 && prices[greater.Peek()] <= prices[i]) greater.Pop();
            answer[i] = greater.Count == 0 ? i + 1 : i - greater.Peek();
            greater.Push(i);
        }
        return answer;
    }

}

using System;
using System.Collections.Generic;
public static class Intervals
{
    // Closed intervals; touching endpoints merge. Does not mutate input; output is ordered by start.
    public static (int Start, int End)[] Merge((int Start, int End)[] intervals)
    {
        ArgumentNullException.ThrowIfNull(intervals);
        var sorted = ((int Start, int End)[])intervals.Clone();
        foreach (var x in sorted) if (x.Start > x.End) throw new ArgumentException("Invalid interval.");
        Array.Sort(sorted, (a, b) => a.Start.CompareTo(b.Start));
        var result = new List<(int Start, int End)>();
        foreach (var x in sorted)
        {
            if (result.Count == 0 || x.Start > result[^1].End) result.Add(x);
            else result[^1] = (result[^1].Start, Math.Max(result[^1].End, x.End));
        }
        return result.ToArray();
    }
    // Meetings use [start,end); positive duration; ending at t frees room at t.
    public static int MeetingRooms((int Start, int End)[] meetings)
    {
        ArgumentNullException.ThrowIfNull(meetings);
        var sorted = ((int Start, int End)[])meetings.Clone();
        foreach (var x in sorted) if (x.Start >= x.End) throw new ArgumentException("Positive durations required.");
        Array.Sort(sorted, (a, b) => a.Start.CompareTo(b.Start));
        var active = new PriorityQueue<int, int>();
        int best = 0;
        foreach (var x in sorted)
        {
            while (active.Count > 0 && active.Peek() <= x.Start) active.Dequeue();
            active.Enqueue(x.End, x.End);
            best = Math.Max(best, active.Count);
        }
        return best;
    }
    // Maximum COUNT of half-open, positive-duration intervals. Not weighted profit.
    public static int MaxNonOverlapping((int Start, int End)[] intervals)
    {
        ArgumentNullException.ThrowIfNull(intervals);
        var sorted = ((int Start, int End)[])intervals.Clone();
        foreach (var x in sorted) if (x.Start >= x.End) throw new ArgumentException("Positive durations required.");
        Array.Sort(sorted, (a, b) => a.End.CompareTo(b.End));
        long lastEnd = long.MinValue; int count = 0;
        foreach (var x in sorted) if (x.Start >= lastEnd) { count++; lastEnd = x.End; }
        return count;
    }
}

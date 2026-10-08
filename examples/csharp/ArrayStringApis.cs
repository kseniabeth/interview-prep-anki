using System;
using System.Text;
public static class ArrayStringApis
{
    public static int[][] SortIntervalsByStart(int[][] intervals)
    {
        ArgumentNullException.ThrowIfNull(intervals);
        var result = (int[][])intervals.Clone();
        foreach (var row in result) if (row == null || row.Length < 2) throw new ArgumentException("Two entries required.");
        Array.Sort(result,(a,b) => a[0].CompareTo(b[0])); return result;
    }
    public static int[] Descending(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var result = (int[])values.Clone(); Array.Sort(result); Array.Reverse(result); return result;
    }
    public static string Repeat(char value, int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        var builder = new StringBuilder(count);
        for (int i = 0; i < count; i++) builder.Append(value);
        return builder.ToString();
    }
    public static int LowercaseIndex(char c)
    {
        if (c < 'a' || c > 'z') throw new ArgumentOutOfRangeException(nameof(c)); return c - 'a';
    }
    public static char LowercaseCharacter(int index)
    {
        if (index < 0 || index >= 26) throw new ArgumentOutOfRangeException(nameof(index)); return (char)('a'+index);
    }
    // Sorted input required; if present, may return ANY equal index, not lower bound.
    public static int FindOrInsertionPoint(int[] values, int target)
    {
        ArgumentNullException.ThrowIfNull(values);
        int result = Array.BinarySearch(values,target); return result >= 0 ? result : ~result;
    }
    public static bool[][] AllocateGrid(int rows, int columns)
    {
        if (rows < 0 || columns < 0) throw new ArgumentOutOfRangeException(nameof(rows));
        var grid = new bool[rows][];
        for (int r = 0; r < rows; r++) grid[r] = new bool[columns];
        return grid;
    }
    public static int[] Filled(int length, int value)
    {
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        var array = new int[length]; Array.Fill(array,value); return array;
    }
    public static (int Row,int Column)[] SortCoordinates((int Row,int Column)[] coordinates)
    {
        ArgumentNullException.ThrowIfNull(coordinates);
        var copy = ((int Row,int Column)[])coordinates.Clone(); Array.Sort(copy); return copy;
    }
    public static string JoinIntegers(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values); return string.Join(",",values);
    }
    public static string ReverseCopy(string text)
    {
        ArgumentNullException.ThrowIfNull(text); char[] chars = text.ToCharArray(); Array.Reverse(chars); return new string(chars);
    }
}

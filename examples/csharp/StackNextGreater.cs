using System;
using System.Collections.Generic;

public static class StackNextGreater
{
    // Returns the first strictly greater VALUE to the right of each input.
    // -1 means absent. Use nullable results if -1 must be distinguishable
    // from a genuine answer (for example, in input [-2, -1]).
    public static int[] Values(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var answer = new int[values.Length];
        Array.Fill(answer, -1);
        var waitingIndices = new Stack<int>();

        for (int i = 0; i < values.Length; i++)
        {
            int currentValue = values[i];

            while (waitingIndices.Count > 0 &&
                   values[waitingIndices.Peek()] < currentValue)
            {
                int waitingIndex = waitingIndices.Pop();
                answer[waitingIndex] = currentValue;
            }

            // The current position now waits for a greater value of its own.
            waitingIndices.Push(i);
        }

        return answer;
    }
}

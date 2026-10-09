using System;
using System.Collections.Generic;

public static class StackBasics
{
    public static void Demonstrate()
    {
        var stack = new Stack<int>();
        stack.Push(10); // Bottom [10] top.
        stack.Push(20); // Bottom [10, 20] top.

        Console.WriteLine($"Top: {stack.Peek()}");       // 20; nothing is removed.
        Console.WriteLine($"Count: {stack.Count}");      // Still 2.
        Console.WriteLine($"Removed: {stack.Pop()}");    // 20; only 10 remains.
        Console.WriteLine($"Now top: {stack.Peek()}");   // 10.

        if (stack.Count > 0)
        {
            Console.WriteLine($"Removed: {stack.Pop()}"); // 10; now empty.
        }

        // TryPop returns false instead of throwing when the stack is empty.
        if (!stack.TryPop(out int item))
        {
            Console.WriteLine("The stack is empty.");
        }
    }
}

using System;
using System.Collections.Generic;

public static class StackBrackets
{
    // Accepts only (), [], and {}. Empty input is balanced; null is rejected.
    public static bool IsBalanced(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var openers = new Stack<char>();

        foreach (char current in text)
        {
            if (current == '(' || current == '[' || current == '{')
            {
                openers.Push(current);
                continue; // This opener will wait for a later closer.
            }

            char expectedOpener;
            switch (current)
            {
                case ')': expectedOpener = '('; break;
                case ']': expectedOpener = '['; break;
                case '}': expectedOpener = '{'; break;
                default: return false; // A non-bracket character is invalid here.
            }

            if (openers.Count == 0)
            {
                return false; // There is no opener for this closer.
            }

            char mostRecentOpener = openers.Pop();
            if (mostRecentOpener != expectedOpener)
            {
                return false; // The bracket types do not match.
            }
        }

        return openers.Count == 0; // No opener may be left waiting.
    }
}

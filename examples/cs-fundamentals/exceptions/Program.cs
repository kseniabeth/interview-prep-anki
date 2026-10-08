using System;

static bool TryReadPositive(string text, out int value)
{
    return int.TryParse(text, out value) && value > 0;
}

Console.WriteLine(TryReadPositive("12", out int n)); // True

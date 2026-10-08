using System;
using System.Text;

string s = "🙂";
int scalars = 0;
foreach (Rune r in s.EnumerateRunes()) scalars++;
Console.WriteLine($"{s.Length}, {scalars}"); // 2, 1

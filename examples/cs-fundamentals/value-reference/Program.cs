using System;

int x = 3;
int y = x;
y = 9;
if (y != 9) throw new Exception("Independent value copy failed");
int[] a = { 3 };
int[] b = a;
b[0] = 9;
Console.WriteLine($"{x}, {a[0]}"); // 3, 9

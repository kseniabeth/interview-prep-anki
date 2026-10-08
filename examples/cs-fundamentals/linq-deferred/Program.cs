using System;
using System.Collections.Generic;
using System.Linq;

var numbers = new List<int> { 1, 2 };
var evens = numbers.Where(n => n % 2 == 0);
numbers.Add(4);
Console.WriteLine(string.Join(",", evens)); // 2,4

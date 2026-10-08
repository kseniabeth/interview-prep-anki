using System;
using System.Collections.Generic;

var visited = new HashSet<Cell> { new Cell(2, 4) };
Console.WriteLine(visited.Contains(new Cell(2, 4))); // True

public readonly record struct Cell(int Row, int Column);

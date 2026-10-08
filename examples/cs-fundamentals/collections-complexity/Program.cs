using System;
using System.Collections.Generic;

var ids = new HashSet<int> { 10, 20, 30 };
Console.WriteLine(ids.Contains(20)); // True
var queue = new Queue<int>();
queue.Enqueue(10);
queue.Enqueue(20);
Console.WriteLine(queue.Dequeue()); // 10

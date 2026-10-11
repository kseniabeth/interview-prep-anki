using System;
using System.Collections.Generic;
using System.Linq;

var orders = new List<OrderRow>
{
    new(105, new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero)),
    new(104, new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero)),
    new(103, new DateTimeOffset(2026, 1, 1, 9, 59, 0, TimeSpan.Zero)),
    new(102, new DateTimeOffset(2026, 1, 1, 9, 58, 0, TimeSpan.Zero))
};

var first = ReadPage(orders, after: null, limit: 2);
Console.WriteLine("Page 1: " + string.Join(", ", first.Select(x => x.Id)));
// Keep the full position of the LAST returned row, not just its time.
var cursor = new OrderCursor(first[^1].CreatedAt, first[^1].Id);

// A new order arrives before the caller asks for page 2.
orders.Add(new OrderRow(106, new DateTimeOffset(2026, 1, 1, 10, 1, 0, TimeSpan.Zero)));
var second = ReadPage(orders, cursor, limit: 2);
Console.WriteLine("Page 2: " + string.Join(", ", second.Select(x => x.Id)));
Check(second.Select(x => x.Id).SequenceEqual(new[] { 103, 102 }),
    "The new first row must not cause a repeated order");

// A one-row page ends at 105. The next page must still include 104,
// even though they share the same CreatedAt value.
var tiedTime = ReadPage(orders, new OrderCursor(first[0].CreatedAt, 105), 1);
Check(tiedTime.Single().Id == 104, "The ID tie-breaker skipped an order");
Check(ReadPage(orders, new OrderCursor(second[^1].CreatedAt, 102), 2).Length == 0,
    "The last page should have no more rows");
Console.WriteLine("PASS: insertion, timestamp tie, end of results");

static OrderRow[] ReadPage(IEnumerable<OrderRow> source, OrderCursor? after, int limit)
{
    if (limit < 1 || limit > 100) throw new ArgumentOutOfRangeException(nameof(limit));
    // Descending order means rows AFTER the cursor have smaller keys.
    return source
        .Where(row => after is null
            || row.CreatedAt < after.CreatedAt
            || (row.CreatedAt == after.CreatedAt && row.Id < after.Id))
        .OrderByDescending(row => row.CreatedAt)
        .ThenByDescending(row => row.Id)
        .Take(limit)
        .ToArray();
}

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

// This is the paging rule in memory, not an HTTP server or cursor-token encoder.
public sealed record OrderRow(int Id, DateTimeOffset CreatedAt);
public sealed record OrderCursor(DateTimeOffset CreatedAt, int Id);

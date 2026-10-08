using System;
using System.Text.Json;

var dto = new OrderSummary(501, 25.50m);
string json = JsonSerializer.Serialize(dto);
Console.WriteLine(json); // {"OrderId":501,"Total":25.50}

public sealed record OrderSummary(int OrderId, decimal Total);

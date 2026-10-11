using System;
using System.Text.Json;

// This stands in for a database row. Its internal note must stay on the server.
var entity = new OrderEntity(501, 25.50m, "Manual fraud review");

// Choose exactly what the caller can see.
var response = new OrderResponse(entity.Id, entity.Total);
var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
string json = JsonSerializer.Serialize(response, options);
Console.WriteLine(json); // {"orderId":501,"total":25.50}
Check(!json.Contains("internalNote"), "Internal data leaked");

// Go in the other direction: JSON text becomes a C# object.
var copy = JsonSerializer.Deserialize<OrderResponse>(json, options)
    ?? throw new InvalidOperationException("Expected an order");
Check(copy == response, "Round trip changed the order");

// An old reader ignores a newly added field with these default options.
string newJson = """{"orderId":501,"total":25.50,"currency":"USD"}""";
var oldReader = JsonSerializer.Deserialize<OrderResponse>(newJson, options);
Check(oldReader == response, "Old reader could not read the new response");

// A new reader also needs a plan for old messages without that field.
// USD is valid here ONLY because all orders in this example were already USD.
var newReader = JsonSerializer.Deserialize<OrderResponseV2>(json, options)
    ?? throw new InvalidOperationException("Expected an order");
Check(newReader.Currency == "USD", "Missing-field default was not applied");

// Parsing alone does not prove the data is complete or meaningful.
var incomplete = JsonSerializer.Deserialize<OrderResponse>("""{"orderId":501}""", options);
Check(incomplete is not null && incomplete.Total == 0m,
    "This .NET 8 record leaves an omitted decimal at zero");
Console.WriteLine("PASS: DTO boundary, round trip, old/new readers, missing field");

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

public sealed record OrderEntity(int Id, decimal Total, string InternalNote);
public sealed record OrderResponse(int OrderId, decimal Total);
public sealed class OrderResponseV2
{
    public int OrderId { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = "USD";
}

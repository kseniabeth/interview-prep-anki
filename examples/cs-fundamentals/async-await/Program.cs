using System.Threading.Tasks;

static async Task WaitForBothAsync()
{
    Task first = Task.Delay(100);
    Task second = Task.Delay(150);
    await Task.WhenAll(first, second);
}

await WaitForBothAsync();
System.Console.WriteLine("Both completed");

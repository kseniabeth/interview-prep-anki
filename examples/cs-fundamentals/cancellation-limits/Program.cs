using System;
using System.Threading;
using System.Threading.Tasks;

static async Task RunOneAsync(SemaphoreSlim sharedGate, CancellationToken token)
{
    await sharedGate.WaitAsync(token);
    try
    {
        await Task.Delay(TimeSpan.FromMilliseconds(10), token);
    }
    finally
    {
        sharedGate.Release();
    }
}

using var gate = new SemaphoreSlim(1, 1);
await RunOneAsync(gate, CancellationToken.None);
if (gate.CurrentCount != 1) throw new Exception("Slot leaked");
using var canceled = new CancellationTokenSource();
canceled.Cancel();
try
{
    await RunOneAsync(gate, canceled.Token);
    throw new Exception("Expected cancellation");
}
catch (OperationCanceledException)
{
    if (gate.CurrentCount != 1) throw new Exception("Incorrect release");
}
Console.WriteLine("Slot released; cancellation observed");

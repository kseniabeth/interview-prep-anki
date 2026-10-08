public sealed class Account
{
    private readonly object gate = new();
    private int balance = 10;

    public bool TrySpend(int amount)
    {
        if (amount <= 0) return false;
        lock (gate)
        {
            if (balance < amount) return false;
            balance -= amount;
            return true;
        }
    }
}

public static class Program
{
    public static void Main()
    {
        var account = new Account();
        int successes = 0;
        System.Threading.Tasks.Parallel.For(0, 100, _ =>
        {
            if (account.TrySpend(1))
                System.Threading.Interlocked.Increment(ref successes);
        });
        if (successes != 10 || account.TrySpend(1) || account.TrySpend(-1))
            throw new System.Exception("Balance invariant failed");
        System.Console.WriteLine("Exactly 10 successful spends");
    }
}

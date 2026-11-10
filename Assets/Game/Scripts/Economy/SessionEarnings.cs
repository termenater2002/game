using System;

public sealed class SessionEarnings
{
    public decimal Balance { get; private set; }
    public event Action<decimal> Changed;
    public void Add(decimal amount)
    {
        if (amount <= 0m) return;
        Balance += amount;
        Changed?.Invoke(Balance);
    }
    public void Reset() { Balance = 0m; Changed?.Invoke(Balance); }
}

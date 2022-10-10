using System;
using static System.Console;

namespace UnitTestSamples
{
  public class BankAccount
  {
    public int Balance { get; protected set; }

    public BankAccount(int startingBalance)
    {
      Balance = startingBalance;
    }

    public void Deposit(int amount)
    {
      if (amount <= 0)
        throw new ArgumentException("Deposit amount must be positive",
          nameof(amount));

      Balance += amount;
    }

    public bool Withdraw(int amount)
    {
      // start with return false

      if (Balance >= amount)
      {
        Balance -= amount;
        return true;
      }
      return false;
    }
  }

  public class BankAccountWithOverdraft : BankAccount
  {
    public int MinimumBalance { get; }

    public BankAccountWithOverdraft(int startingBalance, int minimumBalance)
      : base(startingBalance)
    {
      MinimumBalance = minimumBalance;
    }

    public new bool Withdraw(int amount)
    {
      if (Balance - amount > MinimumBalance)
      {
        Balance -= amount;
        return true;
      }
      return false;
    }
  }
}
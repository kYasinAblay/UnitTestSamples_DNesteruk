using System;
using System.Collections.Generic;
using System.Dynamic;
using ImpromptuInterface;
using NUnit.Framework;

namespace TestDoubles
{
  public interface ILog
  {
    // true if writing succeeded, false otherwise
    // start with void
    bool Write(string msg);
  }

  public class ConsoleLog : ILog
  {
    public bool Write(string msg)
    {
      Console.WriteLine(msg);
      return true;
    }
  }

  class BankAccount
  {
    public int Balance { get; set; }
    private readonly ILog log;

    public BankAccount(ILog log)
    {
      this.log = log;
    }

    public void Deposit(int amount)
    {
      if (log.Write($"User has withdrawn {amount}"))
      {
        Balance += amount;
      }
    }
  }

  // fake, also Null Object
  public class NullLog : ILog
  {
    public bool Write(string msg)
    {
      // nothing here
      return false;
    }
  }

  public class NullLogWithResult : ILog
  {
    private bool expectedResult;

    public NullLogWithResult(bool expectedResult)
    {
      this.expectedResult = expectedResult;
    }

    public bool Write(string msg)
    {
      return expectedResult;
    }
  }

  public class LogMock : ILog
  {
    public Dictionary<string, int> MethodCallCount;
    private bool expectedResult;

    public LogMock(bool expectedResult)
    {
      this.expectedResult = expectedResult;
      MethodCallCount = new Dictionary<string, int>();
    }

    private void AddOrIncrement(string methodName)
    {
      if (MethodCallCount.ContainsKey(methodName)) MethodCallCount[methodName]++;
      else MethodCallCount.Add(methodName, 1);
    }


    public bool Write(string msg)
    {
      AddOrIncrement(nameof(Write));
      return expectedResult;
    }
  }

  public class Null<T> : DynamicObject where T : class
  {
    public static T Instance
    {
      get
      {
        if (!typeof(T).IsInterface)
          throw new ArgumentException("I must be an interface type");

        return new Null<T>().ActLike<T>();
      }
    }

    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
    {
      result = Activator.CreateInstance(
        typeof(T).GetMethod(binder.Name).ReturnType
        );
      return true;
    }
  }

  [TestFixture]
  public class BankAccountTests
  {
    private BankAccount ba;
    
    [Test]
    public void DepositIntegrationTest()
    {
      ba = new BankAccount(new ConsoleLog()) {Balance = 100};
      ba.Deposit(100);
      Assert.That(ba.Balance, Is.EqualTo(200));
    }

    [Test]
    public void DepositUnitTestWithFake()
    {
      var log = new NullLog();
      ba = new BankAccount(log) {Balance = 100};
      ba.Deposit(100);
      Assert.That(ba.Balance, Is.EqualTo(200));
    }

    [Test]
    public void DepositUnitTestWithDynamic()
    {
      var log = Null<ILog>.Instance;
      ba = new BankAccount(log) {Balance = 100};
      ba.Deposit(100);
      Assert.That(ba.Balance, Is.EqualTo(200));
    }

    [Test]
    public void DepositUnitTestWithStub()
    {
      var log = new NullLogWithResult(true);
      ba = new BankAccount(log) { Balance = 100 };
      ba.Deposit(100); // but was log.Write() actually called
      Assert.That(ba.Balance, Is.EqualTo(200));
    }

    [Test]
    public void DepositTestWithMock()
    {
      var log = new LogMock(true);
      ba = new BankAccount(log) {Balance = 100};
      ba.Deposit(100);
      Assert.That(ba.Balance, Is.EqualTo(200));
      Assert.That(
        log.MethodCallCount[nameof(LogMock.Write)], // will NRE if not called
        Is.EqualTo(1));
    }
  }
}

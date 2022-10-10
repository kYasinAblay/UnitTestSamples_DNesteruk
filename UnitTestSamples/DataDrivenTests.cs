using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace UnitTestSamples
{
  [TestFixture]
  public class DataDrivenTests
  {
    private BankAccount ba;

    [SetUp]
    public void SetUp()
    {
      ba = new BankAccount(100);
    }

    [Test] // will fail with no argument
    [TestCase(50, true, 50)]
    [TestCase(100, true, 0)] // can run or debug an individual test case
    [TestCase(1000, false, 100)]
    public void TestMultipleWithdrawalScenarios(
      int amountToWithdraw, bool shouldSucceed, int expectedBalance)
    {
      var result = ba.Withdraw(amountToWithdraw);
      Warn.If(!result, "Failed for some reason");
      Assert.Multiple(() =>
      {
        //Assert.That(result, Is.EqualTo(shouldSucceed));

        // warning is softer than a fail

        // you CANNOT use a Warn in a multiple assertion block!
        Assert.That(expectedBalance, Is.EqualTo(ba.Balance));
      });
    }
  }

  [TestFixtureSource(typeof(BankAccountWithOverdraftTestData), "Data")]
  public class BankAccountWithOverdraftTests
  {
    private BankAccount ba;

    public BankAccountWithOverdraftTests(int startingBalance)
    {
      ba = new BankAccountWithOverdraft(startingBalance, 0);
    }

    public BankAccountWithOverdraftTests(int startingBalance, int minBalance)
    {
      ba = new BankAccountWithOverdraft(startingBalance, minBalance);
    }

    [Test]
    public void MinimumBalanceIsNonPositive()
    {
      Assert.That(ba.Balance, Is.LessThanOrEqualTo(0));
    }
  }

  public class BankAccountWithOverdraftTestData
  {
    public static IEnumerable Data
    {
      get
      {
        yield return new TestFixtureData(100);
        yield return new TestFixtureData(0, -100);
        yield return new TestFixtureData(50, -50);
      }
    }
  }
}
using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace UnitTestSamples
{
  [TestFixture] // used to define a grouping of tests
  [Author("Dmitri Nesteruk")]
  public class BankAccountTests // can run all tests
  {
    [Test]
    [Author("John Doe")]
    public void TestThatFails()
    {
      //throw new Exception("Something went wrong");
      //Assert.Fail();

      // indicate a problem

    }

    [Test]
    public void Inconclusive()
    {
      //Assert.Inconclusive();

      // a test with a warning is also considered inconclusive
      Assert.Warn("There's a problem here!");
    }

    [Test]
    public void Success()
    {
      // there's no Assert.Success, but a test with no
      // assertions is assumed to be successful
    }

    [Test] // used to define a single test
    public void BankAccountShouldIncreaseOnPositiveDeposit() // give the test a meaningful name
    {
      // typically just a single assertion

      // AAA = arrange act assert

      // Arrange: set up the unit under test
      var ba = new BankAccount(0);

      // Act: do something to change the system
      var amount = 100;
      ba.Deposit(amount);

      // Assert: check correct operation
      Assert.That(ba.Balance, Is.EqualTo(100),
        "Expected a balance of 100 after a deposit"); // can specify optional message if test fails
    }

    [Test] // exception handling
    public void BankAccountShouldThrowOnNegativeDeposit()
    {
      var ba = new BankAccount(0);

      // this form requires the exact type
      // allowed to break the rule of 'one assert per test' here

      //var ex = Assert.Throws<ArgumentException>(() => ba.Deposit(-1));

      // exception type must be EXACT, not a descendant
      var ex = Assert.Throws<Exception>(() => ba.Deposit(-1));

      // StringAssert is a special class for testing on strings
      // has Contains, StartsWith, EndsWith, Equals, IsMatch (Regex) etc.
      StringAssert.StartsWith("Deposit amount must be positive", ex.Message);

      // or combine everything
      Assert.Throws(Is.TypeOf<ArgumentException>()
        .And.Message.StartsWith("Deposit amount must be positive"),
        () => ba.Deposit(-1));
    }

    [Test]
    public void TestWithdrawalWithSufficientBalance()
    {
      var ba = new BankAccount(100);

      var success = ba.Withdraw(50);

      // without Assert.Multiple, we only get info about the first failure
      Assert.Multiple(() =>
      {
        Assert.IsTrue(success);
        Assert.That(ba.Balance, Is.EqualTo(50));
      });
    }
  }
}
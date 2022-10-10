using NUnit.Framework;

namespace UnitTestSamples
{
  [TestFixture]
  public class WarningsFixture
  {
    [Test]
    public void Test()
    {
      Warn.If(2+2 != 5);
      Warn.If(2+2, Is.Not.EqualTo(5));
      Warn.If(() => 2+2, Is.Not.EqualTo(5).After(2000));

      Warn.Unless(2+2 == 5);
      Warn.Unless(2+2, Is.EqualTo(5));
      Warn.Unless(() => 2+2, Is.EqualTo(5).After(3000));

      Assert.Warn("I'm warning you!");
    }
  }
}
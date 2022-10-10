using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UnitTestSamples.Coverage
{
  public static class Solve
  {
    public static Tuple<double, double> Quadratic(double a, double b, double c)
    {
      var disc = b * b - 4 * a * c;
      if (disc < 0)
      {
        throw new Exception("Cannot solve with complex roots");
      }
      else
      {
        var root = Math.Sqrt(disc);
        return Tuple.Create(
          (-b + root) / 2 / a, 
          (-b - root) / 2 / a);
      }
    }
  }

  [TestFixture]
  public class EquationTests
  {
    [Test]
    public void TestStuff()
    {
      var result = Solve.Quadratic(1, 10, 16);
      Assert.That(result.Item1, Is.EqualTo(-2));
    }
  }
}

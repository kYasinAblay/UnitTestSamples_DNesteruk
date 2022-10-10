using System;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace MoqSamples
{
  public class Bar : IEquatable<Bar>
  {
    // introduced later
    public string Name { get; set; }

    public bool Equals(Bar other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return string.Equals(Name, other.Name);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((Bar) obj);
    }

    public override int GetHashCode()
    {
      return (Name != null ? Name.GetHashCode() : 0);
    }

    public static bool operator ==(Bar left, Bar right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(Bar left, Bar right)
    {
      return !Equals(left, right);
    }
  }

  public interface IBaz {
    string Name { get; }
  }

  public interface IFoo
  {
    bool DoSomething(string value);
    string ProcessString(string value);
    bool TryParse(string value, out string outputValue);
    bool Submit(ref Bar bar);
    int GetCount();
    bool Add(int amount);

    string Name { get; set; }
    IBaz SomeBaz { get; }
    int SomeOtherProperty { get; set; }
  }

  public delegate void AlienAbductionEventHandler(int galaxy, bool returned);

  public interface IAnimal
  {
    event EventHandler FallsIll;
    void Stumble();

    event AlienAbductionEventHandler AbductedByAliens;
  }

  public class Doctor
  {
    public int TimesCured;
    public int AbductionsObserved;

    public Doctor(IAnimal animal)
    {
      animal.FallsIll += (sender, args) =>
      {
        Console.WriteLine("I will cure you!");
        TimesCured++;
      };

      animal.AbductedByAliens += (galaxy, returned) => ++AbductionsObserved;
    }
  }
  
  [TestFixture]
  public class MethodSamples
  {
    [Test]
    public void OrdinaryMethodCalls()
    {
      var mock = new Mock<IFoo>();

      //mock.Setup(foo => foo.DoSomething("ping")).Returns(true);
      //mock.Setup(foo => foo.DoSomething("pong")).Returns(true);

      //mock.Setup(foo => foo.DoSomething(It.IsIn("ping", "pong")))
      //    .Returns(true);

      // same as above, but lazy evaluated
      mock.Setup(foo => foo.DoSomething(
        It.IsIn("ping", "pong"))).Returns(() => true);

      // MATCHING METHOD INPUT VALUES

      // support for different argument values
      Assert.That(mock.Object.DoSomething("ping"), Is.True);
      Assert.That(mock.Object.DoSomething("pong"), Is.True);
      // if unspecified, this will yield default(T)
      Assert.That(mock.Object.DoSomething("something else"), Is.False);
      // value can be null, don't worry!
      Assert.That(mock.Object.DoSomething(null), Is.False);
    }

    [Test]
    public void ArgumentDependentMatching()
    {
      var mock = new Mock<IFoo>();

      // any value
      mock.Setup(foo =>
        foo.DoSomething(It.IsAny<string>())).Returns(true);

      // predicate
      mock.Setup(foo =>
        foo.Add(It.Is<int>(x => x % 2 == 0))).Returns(false);

      // ranges
      mock.Setup(foo=>foo.Add(It.IsInRange<int>(1,10, Range.Inclusive)))
        .Returns(false);

      // regex
      mock.Setup(foo => foo.DoSomething(It.IsRegex("[a-z]+")))
        .Returns(true);
    }

    [Test]
    public void OutArguments()
    {
      var mock = new Mock<IFoo>();
      var requiredOutput = "ok";
      mock.Setup(foo => foo.TryParse("ping", out requiredOutput))
        .Returns(true);

      // now call it and assert
      string result;
      Assert.IsTrue(mock.Object.TryParse("ping", out result));
      Assert.That(result, Is.EqualTo(requiredOutput));

      // unhandled cases give unpredictable results
      var thisShouldBeFalse = mock.Object.TryParse("pong", out result);
      Console.WriteLine(thisShouldBeFalse);
      Console.WriteLine(result); // weirdly this is still "ok"
    }

    [Test]
    public void RefArguments()
    {
      var mock = new Mock<IFoo>();
      var bar = new Bar() {Name = "abc"};
      mock.Setup(foo => foo.Submit(ref bar)).Returns(true);

      // this only works with this particular 'bar'
      // uses referential equality, not structural
      var someOtherBar = new Bar() {Name = "abc"};
      Assert.IsTrue(mock.Object.Submit(ref bar));
      Assert.IsFalse(mock.Object.Submit(ref someOtherBar));
    }

    [Test]
    public void FakingInvocationArguments()
    {
      var mock = new Mock<IFoo>();
      mock.Setup(x => x.ProcessString(It.IsAny<string>()))
        .Returns((string s) => s.ToLowerInvariant());
      Assert.That(mock.Object.ProcessString("ABC"), Is.EqualTo("abc"));
      // plenty of overloads available
    }

    [Test]
    public void ThrowingExceptions()
    {
      var mock = new Mock<IFoo>();
      mock.Setup(foo => foo.DoSomething("kill"))
        .Throws<InvalidOperationException>();
      Assert.Throws<InvalidOperationException>(
        () => mock.Object.DoSomething("kill"));

      // alternative syntax
      mock.Setup(foo => foo.DoSomething(null))
        .Throws(new ArgumentNullException("cmd"));
      Assert.Throws<ArgumentNullException>(() =>
      {
        mock.Object.DoSomething(null);
      }, "cmd");

      mock.Object.DoSomething("test"); // no exception
    }

    [Test]
    public void DifferentReturnsOnEachCall()
    {
      var mock = new Mock<IFoo>();
      var calls = 0;
      mock.Setup(foo => foo.GetCount())
        .Returns(() => calls)
        .Callback(() => ++calls);
      mock.Object.GetCount();
      mock.Object.GetCount();
      Assert.That(mock.Object.GetCount(), Is.EqualTo(2)); // 2 not 3!
    }
  }

  [TestFixture]
  public class NonMethodSamples
  {
    [Test]
    public void Properties()
    {
      var mock = new Mock<IFoo>();
      
      mock.Setup(foo => foo.Name).Returns("bar");

      // no tracking, so
      mock.Object.Name = "this will not be assigned";

      Assert.That(mock.Object.Name, Is.EqualTo("bar"));

      // recursive mocking of properties
      // subproperties get initialized
      mock.Setup(foo => foo.SomeBaz.Name).Returns("hello");
      Assert.That(mock.Object.SomeBaz.Name, Is.EqualTo("hello"));

      bool setterCalled = false;

      // whenever someone calls this setter, we can do something...
      mock.SetupSet(foo => { foo.Name = It.IsAny<string>(); })
      //                                ^^^ try "abc"
        .Callback<string>(value =>
        {
          setterCalled = true;
        });
      mock.Object.Name = "abc"; // argument must match if specified

      //Assert.IsTrue(setterCalled);

      mock.Object.Name = "def"; // required for test to pass
      mock.VerifySet(foo => { foo.Name = "def"; }, Times.AtLeastOnce,
        "You did not set foo.Name to 'def' at least once");
    }

    [Test]
    public void ValueTracking()
    {
      var mock = new Mock<IFoo>();

      // stub out a single property
      //mock.SetupProperty(f => f.Name);
      mock.SetupAllProperties();

      // now you can access the underlying object
      IFoo foo = mock.Object;

      foo.Name = "abc";
      Assert.That(mock.Object.Name, Is.EqualTo("abc"));

      foo.Name = "abcd";
      Assert.That(mock.Object.Name, Is.EqualTo("abcd"));

      foo.SomeOtherProperty = 123;
      // won't work if you only stubbed one property
      Assert.That(mock.Object.SomeOtherProperty, Is.EqualTo(123));
    }

    [Test]
    public void EventMocking()
    {
      var mock = new Mock<IAnimal>();
      var doctor = new Doctor(mock.Object);

      mock.Raise(a => a.FallsIll += null, new EventArgs());
      Assert.That(doctor.TimesCured, Is.EqualTo(1));

      // ensure that a method call results in an event being fired
      mock.Setup(a => a.Stumble())
        .Raises(a => a.FallsIll += null, new EventArgs());
      mock.Object.Stumble(); // causes the event
      Assert.That(doctor.TimesCured, Is.EqualTo(2));

      // fire an event with custom arguments
      mock.Raise(a => a.AbductedByAliens += null, 42, true);
      Assert.That(doctor.AbductionsObserved, Is.EqualTo(1));
    }

    [Test]
    public void Callbacks()
    {
      var mock = new Mock<IFoo>();

      int x = 0;
      mock.Setup(foo => foo.DoSomething("ping"))
        .Returns(true)
        .Callback(() => x++);
      mock.Object.DoSomething("ping");
      Assert.That(x, Is.EqualTo(1));

      // invocation arguments
      mock.Setup(foo => foo.DoSomething(It.IsAny<string>()))
        .Returns(true)
        .Callback((string s) => x += s.Length);

      mock.Setup(foo => foo.DoSomething(It.IsAny<string>()))
        .Returns(true)
        .Callback<string>(s => x += s.Length);

      // callbacks before and after invocation
      mock.Setup(foo => foo.DoSomething("pong"))
        .Callback(() => Console.WriteLine("before pong"))
        .Returns(true)
        .Callback(() => Console.WriteLine("after pong"));
      mock.Object.DoSomething("pong");
    }

    public class Consumer
    {
      private IFoo foo;

      public Consumer(IFoo foo)
      {
        this.foo = foo;
      }

      public void Hello()
      {
        foo.DoSomething("ping");
        var name = foo.Name;
        foo.SomeOtherProperty = 123;
      }
    }

    [Test]
    public void Verification()
    {
      var mock = new Mock<IFoo>();
      var consumer = new Consumer(mock.Object);

      consumer.Hello(); // later

      // ensure we called it with "ping"
      mock.Verify(foo => foo.DoSomething("ping"), Times.AtLeastOnce);

      // ensure we never called it with "pong"
      mock.Verify(foo=> foo.DoSomething("pong"), Times.Never);

      // verify getter invocation (similar for set)
      mock.VerifyGet(foo => foo.Name);

      // verify setter with argument matcher
      mock.VerifySet(foo => foo.SomeOtherProperty = It.IsInRange(100, 200, Range.Inclusive));
    }

    [Test]
    public void BehaviorCustomization()
    {
      // strict behavior
      //var mock = new Mock<IFoo>(MockBehavior.Strict);
      var mock = new Mock<IFoo>
      {
        DefaultValue = DefaultValue.Mock
      };

      mock.Setup(f => f.DoSomething("abc"))
        .Returns(true);

      mock.Object.DoSomething("abc");

      // automatic recursive mocking
      var baz = mock.Object.SomeBaz;

      var bazMock = Mock.Get(baz); // baz is reused
      bazMock.SetupGet(f => f.Name).Returns("abc");

      // centralization of mock instance creation/mgmt
      var repository = new MockRepository(MockBehavior.Strict)
      {
        DefaultValue = DefaultValue.Mock
      };

      var fooMock = repository.Create<IFoo>();

      // mock overriding default settings
      var otherMock = repository.Create<IBaz>(MockBehavior.Loose);

      // verify all mocks
      repository.Verify();
    }

    abstract class Person
    {
      protected int SSN { get; set; }
      protected abstract void Execute(string cmd);
    }

    [Test]
    public void ExpectationsForProtectedMembers()
    {
      var mock = new Mock<Person>();
      mock.Protected().Setup<int>("SSN")
        .Returns(42);


      mock.Protected()
        .Setup<string>("Execute", ItExpr.IsAny<string>());
    }
  }
}

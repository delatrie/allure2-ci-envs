using System;
using NUnit.Framework;

namespace Allure.Examples.CiEnvs.Allure2.Tests;

class UnitTests
{
    private Random rnd;

    [SetUp]
    public void Init()
    {
        this.rnd = new Random();
    }

    [Test]
    public void AlwaysPasses()
    {
    }

    [Test]
    public void FailsWith25Percent()
    {
        Assert.That(this.rnd.NextInt64(0, 4), Is.Not.Zero);
    }

    [Test]
    public void BreaksWith25Percent()
    {
        if (this.rnd.NextInt64(0, 4) == 0)
        {
            throw new Exception("An artifitial exception");
        }
    }

    [Test]
    public void FailsBreaksOrSkipesWith30Percent()
    {
        switch (this.rnd.NextInt64(0, 10))
        {
            case 0:
                throw new Exception("An artifitial exception");
            case 1:
                Assert.Fail("An artificial failure");
                return;
            case 2:
                Assert.Ignore();
                return;
        }
    }
}

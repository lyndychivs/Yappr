namespace Yappr.Unit.Tests;

using System;

using NUnit.Framework;

[TestFixture]
public sealed class TldrOutcomeTests
{
    [Test]
    public void Success_NullResult_ThrowsArgumentNullException()
    {
        Assert.That(
            () => TldrOutcome.Success(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("result"));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Failure_NullOrWhiteSpaceError_ThrowsArgumentException(string? error)
    {
        Assert.That(
            () => TldrOutcome.Failure(error!),
            Throws.InstanceOf<ArgumentException>().With.Property(nameof(ArgumentException.ParamName)).EqualTo("error"));
    }
}

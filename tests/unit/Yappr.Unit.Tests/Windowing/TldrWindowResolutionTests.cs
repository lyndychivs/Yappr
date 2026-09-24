namespace Yappr.Unit.Tests.Windowing;

using System;

using NUnit.Framework;

using Yappr.Windowing;

[TestFixture]
public sealed class TldrWindowResolutionTests
{
    [TestCase(0)]
    [TestCase(-1)]
    public void ForMessageLimit_ZeroOrNegative_ThrowsArgumentOutOfRangeException(int messageLimit)
    {
        Assert.That(
            () => TldrWindowResolution.ForMessageLimit(messageLimit),
            Throws.InstanceOf<ArgumentOutOfRangeException>().With.Property(nameof(ArgumentException.ParamName)).EqualTo("messageLimit"));
    }

    [Test]
    public void ForMessageLimit_One_ReturnsValidMessageLimit()
    {
        TldrWindowResolution window = TldrWindowResolution.ForMessageLimit(1);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(window.IsValid, Is.True);
            Assert.That(window.MessageLimit, Is.EqualTo(1));
        }
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Invalid_NullOrWhiteSpaceValidationError_ThrowsArgumentException(string? validationError)
    {
        Assert.That(
            () => TldrWindowResolution.Invalid(validationError!),
            Throws.InstanceOf<ArgumentException>().With.Property(nameof(ArgumentException.ParamName)).EqualTo("validationError"));
    }
}

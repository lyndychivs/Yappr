namespace Yappr.Unit.Tests.Windowing;

using System;

using NUnit.Framework;

using Yappr.Models;
using Yappr.Windowing;

[TestFixture]
public sealed class TldrWindowResolverTests
{
    private readonly TldrLimitsOptions limits = new() { MaxDays = 30, MaxHours = 720, MaxMessages = 500 };

    [TestCase(0)]
    [TestCase(-1)]
    public void Resolve_NonPositiveValue_IsInvalid(int value)
    {
        TldrWindowResolution result = TldrWindowResolver.Resolve(TldrWindowKind.Days, value, this.limits);

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Resolve_DaysWithinLimit_ReturnsTimeCutoff()
    {
        DateTimeOffset before = DateTimeOffset.UtcNow.AddDays(-3);

        TldrWindowResolution result = TldrWindowResolver.Resolve(TldrWindowKind.Days, 3, this.limits);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.MessageLimit, Is.Null);
            Assert.That(result.SinceUtc, Is.Not.Null);
            Assert.That(result.SinceUtc!.Value, Is.EqualTo(before).Within(TimeSpan.FromSeconds(5)));
        });
    }

    [Test]
    public void Resolve_DaysAboveLimit_IsInvalid()
    {
        TldrWindowResolution result = TldrWindowResolver.Resolve(TldrWindowKind.Days, this.limits.MaxDays + 1, this.limits);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ValidationError, Does.Contain("days"));
        });
    }

    [Test]
    public void Resolve_HoursAboveLimit_IsInvalid()
    {
        TldrWindowResolution result = TldrWindowResolver.Resolve(TldrWindowKind.Hours, this.limits.MaxHours + 1, this.limits);

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Resolve_MessagesWithinLimit_ReturnsMessageLimit()
    {
        TldrWindowResolution result = TldrWindowResolver.Resolve(TldrWindowKind.Messages, 50, this.limits);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.MessageLimit, Is.EqualTo(50));
            Assert.That(result.SinceUtc, Is.Null);
        });
    }

    [Test]
    public void Resolve_MessagesAboveLimit_IsInvalid()
    {
        TldrWindowResolution result = TldrWindowResolver.Resolve(TldrWindowKind.Messages, this.limits.MaxMessages + 1, this.limits);

        Assert.That(result.IsValid, Is.False);
    }
}

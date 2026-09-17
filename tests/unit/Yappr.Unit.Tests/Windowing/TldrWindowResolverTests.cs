namespace Yappr.Unit.Tests.Windowing;

using System;

using NUnit.Framework;

using Yappr.Models;
using Yappr.Windowing;

[TestFixture]
public sealed class TldrWindowResolverTests
{
    private readonly TldrLimitsOptions limits = new()
    {
        MaxDays = 30,
        MaxHours = 720,
        MaxMessages = 500,
    };

    [TestCase(0)]
    [TestCase(-1)]
    public void Resolve_NonPositiveValue_IsInvalid(int value)
    {
        TldrWindowResolution result = TldrWindowResolver.Resolve(TldrWindowKind.Days, value, limits);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ValidationError, Is.EqualTo("The value must be greater than zero."));
        }
    }

    [Test]
    public void Resolve_DaysWithinLimit_ReturnsTimeCutoff()
    {
        DateTimeOffset before = DateTimeOffset.UtcNow.AddDays(-3);

        TldrWindowResolution result = TldrWindowResolver.Resolve(TldrWindowKind.Days, 3, limits);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.MessageLimit, Is.Null);
            Assert.That(result.SinceUtc, Is.Not.Null);
            Assert.That(result.SinceUtc!.Value, Is.EqualTo(before).Within(TimeSpan.FromSeconds(5)));
        }
    }

    [Test]
    public void Resolve_DaysAboveLimit_IsInvalid()
    {
        TldrWindowResolution result = TldrWindowResolver.Resolve(TldrWindowKind.Days, limits.MaxDays + 1, limits);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ValidationError, Is.EqualTo("`days` can be at most 30."));
        }
    }

    [Test]
    public void Resolve_DaysAtLimit_ReturnsTimeCutoff()
    {
        TldrWindowResolution result = TldrWindowResolver.Resolve(TldrWindowKind.Days, limits.MaxDays, limits);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Resolve_HoursAboveLimit_IsInvalid()
    {
        TldrWindowResolution result = TldrWindowResolver.Resolve(TldrWindowKind.Hours, limits.MaxHours + 1, limits);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ValidationError, Is.EqualTo("`hours` can be at most 720."));
        }
    }

    [Test]
    public void Resolve_HoursAtLimit_ReturnsTimeCutoff()
    {
        DateTimeOffset before = DateTimeOffset.UtcNow.AddHours(-limits.MaxHours);

        TldrWindowResolution result = TldrWindowResolver.Resolve(TldrWindowKind.Hours, limits.MaxHours, limits);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.SinceUtc, Is.Not.Null);
            Assert.That(result.SinceUtc!.Value, Is.EqualTo(before).Within(TimeSpan.FromSeconds(5)));
        }
    }

    [Test]
    public void Resolve_MessagesWithinLimit_ReturnsMessageLimit()
    {
        TldrWindowResolution result = TldrWindowResolver.Resolve(TldrWindowKind.Messages, 50, limits);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.MessageLimit, Is.EqualTo(50));
            Assert.That(result.SinceUtc, Is.Null);
        }
    }

    [Test]
    public void Resolve_MessagesAboveLimit_IsInvalid()
    {
        TldrWindowResolution result = TldrWindowResolver.Resolve(TldrWindowKind.Messages, limits.MaxMessages + 1, limits);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ValidationError, Is.EqualTo("`messages` can be at most 500."));
        }
    }

    [Test]
    public void Resolve_MessagesAtLimit_ReturnsMessageLimit()
    {
        TldrWindowResolution result = TldrWindowResolver.Resolve(TldrWindowKind.Messages, limits.MaxMessages, limits);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.MessageLimit, Is.EqualTo(limits.MaxMessages));
        }
    }

    [Test]
    public void Resolve_NullLimits_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => TldrWindowResolver.Resolve(TldrWindowKind.Days, 1, null!));
    }
}

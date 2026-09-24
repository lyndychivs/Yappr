namespace Yappr.Discord.Unit.Tests.Services;

using System;

using NUnit.Framework;

using Yappr.Discord.Services;

[TestFixture]
public sealed class NetCordMessageFetcherTests
{
    [Test]
    public void Constructor_NullGatewayClient_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new NetCordMessageFetcher(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("gatewayClient"));
    }
}

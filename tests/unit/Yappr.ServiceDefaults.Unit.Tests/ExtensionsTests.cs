namespace Yappr.ServiceDefaults.Unit.Tests;

using System;

using Microsoft.Extensions.Hosting;

using NUnit.Framework;

using Yappr.ServiceDefaults;

[TestFixture]
public sealed class ExtensionsTests
{
    [Test]
    public void AddServiceDefaults_NullBuilder_ThrowsArgumentNullException()
    {
        Assert.That(
            () => Extensions.AddServiceDefaults<HostApplicationBuilder>(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("builder"));
    }

    [Test]
    public void MapDefaultEndpoints_NullApp_ThrowsArgumentNullException()
    {
        Assert.That(
            () => Extensions.MapDefaultEndpoints(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("app"));
    }
}

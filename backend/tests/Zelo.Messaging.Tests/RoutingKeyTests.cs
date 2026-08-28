using Xunit;
using Zelo.Contracts;
using Zelo.Messaging.Internal;

namespace Zelo.Messaging.Tests;

public class RoutingKeyTests
{
    [Fact]
    public void For_UsesLowercasedFullTypeName()
    {
        var key = RoutingKey.For<ObligationScheduled>();

        Assert.Equal("zelo.contracts.obligationscheduled", key);
    }

    [Fact]
    public void For_DifferentEventTypes_ProduceDifferentKeys()
    {
        var scheduled = RoutingKey.For<ObligationScheduled>();
        var completed = RoutingKey.For<ObligationCompleted>();

        Assert.NotEqual(scheduled, completed);
    }

    [Fact]
    public void For_GenericAndTypeOverload_AgreeWithEachOther()
    {
        var generic = RoutingKey.For<AssetCreated>();
        var byType = RoutingKey.For(typeof(AssetCreated));

        Assert.Equal(generic, byType);
    }
}

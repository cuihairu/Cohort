using Cohort.Protocol.Models;

namespace Cohort.Engine.Tests;

public sealed class TestPlatformEventMapperTests
{
    [Fact]
    public void ServerMapper_MapsMatchAndFactionIds()
    {
        var mapper = new Cohort.Server.Ingress.TestPlatformEventMapper();
        const string raw = """
            {
              "sessionId": "s1",
              "eventId": "e1",
              "userId": "u1",
              "kind": "Gift",
              "matchId": "m1",
              "factionId": "blue"
            }
            """;

        var ev = mapper.TryMap("test", raw, ingestTimeMs: 1234);

        Assert.NotNull(ev);
        Assert.Equal("m1", ev!.MatchId);
        Assert.Equal("blue", ev.FactionId);
        Assert.Equal(AudienceEventKind.Gift, ev.Kind);
    }

    [Fact]
    public void GatewayMapper_MapsMatchAndFactionIds()
    {
        var mapper = new Cohort.Gateway.Ingress.TestPlatformEventMapper();
        const string raw = """
            {
              "sessionId": "s1",
              "eventId": "e1",
              "userId": "u1",
              "kind": "Comment",
              "matchId": "m1",
              "factionId": "blue"
            }
            """;

        var ev = mapper.TryMap("test", raw, ingestTimeMs: 1234);

        Assert.NotNull(ev);
        Assert.Equal("m1", ev!.MatchId);
        Assert.Equal("blue", ev.FactionId);
        Assert.Equal(AudienceEventKind.Comment, ev.Kind);
    }
}

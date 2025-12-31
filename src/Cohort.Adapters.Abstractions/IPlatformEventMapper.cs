using Cohort.Protocol.Models;

namespace Cohort.Adapters.Abstractions;

public interface IPlatformEventMapper
{
    AudienceEvent? TryMap(string platform, string rawBody, long ingestTimeMs);
}


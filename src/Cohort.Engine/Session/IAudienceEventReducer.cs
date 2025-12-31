using Cohort.Protocol.Models;

namespace Cohort.Engine.Session;

public interface IAudienceEventReducer
{
    IReadOnlyList<AudienceEvent> Reduce(IReadOnlyList<AudienceEvent> events, int maxEventsPerTick);
}


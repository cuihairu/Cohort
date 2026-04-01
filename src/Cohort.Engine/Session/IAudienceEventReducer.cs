using Cohort.Protocol.Models;

namespace Cohort.Engine.Session;

public sealed record AudienceEventReduceResult(
    IReadOnlyList<AudienceEvent> Events,
    int MergedInputEvents,
    int DroppedInputEvents
);

public interface IAudienceEventReducer
{
    AudienceEventReduceResult Reduce(IReadOnlyList<AudienceEvent> events, int maxEventsPerTick);
}

namespace Cohort.Engine.Session;

public sealed record SessionDiagnostics(
    string SessionId,
    long TickId,
    int TickDurationMs,
    long ServerTimeMs,
    long TotalIngestedEvents,
    long TotalAppliedEvents,
    long TotalDroppedEvents,
    long TotalSnapshotsSent,
    long TotalResyncSnapshotsSent,
    long LastTickProcessMs,
    IReadOnlyList<SessionClientDiagnostics> Clients
);

public sealed record SessionClientDiagnostics(
    string ClientId,
    long ConnectedTickId,
    long LastAckTickId,
    long LagTicks,
    long LastAckAgeMs,
    long ResyncCount,
    long LastResyncAgeMs
);


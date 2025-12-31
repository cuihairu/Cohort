using System.Text.Json;

namespace Cohort.Protocol.Messages;

public sealed record ServerSnapshot(
    string Type,
    string SessionId,
    long TickId,
    long ServerTimeMs,
    JsonElement State,
    bool? Forced = null,
    string? Reason = null,
    string? TargetClientId = null,
    long? ClientLagTicks = null,
    long? ClientLastAckTickId = null
);

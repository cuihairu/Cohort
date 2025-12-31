using System.Text.Json;
using Cohort.Adapters.Abstractions;
using Cohort.Protocol.Models;

namespace Cohort.Gateway.Ingress;

public sealed class TestPlatformEventMapper : IPlatformEventMapper
{
    public AudienceEvent? TryMap(string platform, string rawBody, long ingestTimeMs)
    {
        if (!string.Equals(platform, "test", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        using var doc = JsonDocument.Parse(rawBody);
        var root = doc.RootElement;

        if (!root.TryGetProperty("sessionId", out var sessionIdEl))
        {
            return null;
        }

        var sessionId = sessionIdEl.GetString();
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return null;
        }

        var eventId = root.TryGetProperty("eventId", out var eventIdEl) ? eventIdEl.GetString() : null;
        eventId ??= $"test:{Guid.NewGuid():N}";

        var userId = root.TryGetProperty("userId", out var userIdEl) ? userIdEl.GetString() : null;
        userId ??= "anonymous";

        AudienceEventKind kind = AudienceEventKind.Comment;
        if (root.TryGetProperty("kind", out var kindEl))
        {
            if (kindEl.ValueKind == JsonValueKind.String && Enum.TryParse(kindEl.GetString(), ignoreCase: true, out AudienceEventKind parsed))
            {
                kind = parsed;
            }
            else if (kindEl.ValueKind == JsonValueKind.Number && kindEl.TryGetInt32(out var kindNum))
            {
                kind = (AudienceEventKind)kindNum;
            }
        }

        string? text = root.TryGetProperty("text", out var textEl) ? textEl.GetString() : null;
        string? giftId = root.TryGetProperty("giftId", out var giftIdEl) ? giftIdEl.GetString() : null;
        int? giftCount = root.TryGetProperty("giftCount", out var giftCountEl) && giftCountEl.TryGetInt32(out var gc) ? gc : null;
        int? giftValue = root.TryGetProperty("giftValue", out var giftValueEl) && giftValueEl.TryGetInt32(out var gv) ? gv : null;

        return new AudienceEvent(
            EventId: eventId,
            Platform: "test",
            SessionId: sessionId,
            UserId: userId,
            Kind: kind,
            IngestTimeMs: ingestTimeMs,
            Text: text,
            GiftId: giftId,
            GiftCount: giftCount,
            GiftValue: giftValue
        );
    }
}


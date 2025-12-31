using Cohort.Protocol.Models;

namespace Cohort.Engine.Session;

public sealed class DefaultAudienceEventReducer : IAudienceEventReducer
{
    public IReadOnlyList<AudienceEvent> Reduce(IReadOnlyList<AudienceEvent> events, int maxEventsPerTick)
    {
        if (maxEventsPerTick <= 0 || events.Count <= maxEventsPerTick)
        {
            return events;
        }

        var likesByUser = new Dictionary<string, int>(StringComparer.Ordinal);
        var giftsByUserGift = new Dictionary<(string UserId, string GiftId), (int Count, int Value)>();
        var comments = new List<AudienceEvent>(capacity: Math.Min(events.Count, maxEventsPerTick));
        var passthrough = new List<AudienceEvent>();

        foreach (var e in events)
        {
            switch (e.Kind)
            {
                case AudienceEventKind.Like:
                    likesByUser[e.UserId] = likesByUser.TryGetValue(e.UserId, out var c) ? c + 1 : 1;
                    break;
                case AudienceEventKind.Gift:
                    {
                        var giftId = e.GiftId ?? "unknown";
                        var giftCount = e.GiftCount ?? 1;
                        var giftValue = e.GiftValue ?? 0;
                        var key = (e.UserId, giftId);
                        if (giftsByUserGift.TryGetValue(key, out var v))
                        {
                            giftsByUserGift[key] = (v.Count + giftCount, v.Value + giftValue * giftCount);
                        }
                        else
                        {
                            giftsByUserGift[key] = (giftCount, giftValue * giftCount);
                        }
                    }
                    break;
                case AudienceEventKind.Comment:
                    comments.Add(e);
                    break;
                default:
                    passthrough.Add(e);
                    break;
            }
        }

        var reduced = new List<AudienceEvent>(capacity: maxEventsPerTick);

        foreach (var e in passthrough)
        {
            reduced.Add(e);
            if (reduced.Count >= maxEventsPerTick)
            {
                return reduced;
            }
        }

        foreach (var kv in giftsByUserGift)
        {
            reduced.Add(new AudienceEvent(
                EventId: $"merged:gift:{kv.Key.UserId}:{kv.Key.GiftId}:{Guid.NewGuid():N}",
                Platform: "merged",
                SessionId: events[0].SessionId,
                UserId: kv.Key.UserId,
                Kind: AudienceEventKind.Gift,
                IngestTimeMs: events[0].IngestTimeMs,
                GiftId: kv.Key.GiftId,
                GiftCount: kv.Value.Count,
                GiftValue: kv.Value.Value
            ));
            if (reduced.Count >= maxEventsPerTick)
            {
                return reduced;
            }
        }

        foreach (var kv in likesByUser)
        {
            reduced.Add(new AudienceEvent(
                EventId: $"merged:like:{kv.Key}:{Guid.NewGuid():N}",
                Platform: "merged",
                SessionId: events[0].SessionId,
                UserId: kv.Key,
                Kind: AudienceEventKind.Like,
                IngestTimeMs: events[0].IngestTimeMs,
                Text: kv.Value.ToString()
            ));
            if (reduced.Count >= maxEventsPerTick)
            {
                return reduced;
            }
        }

        foreach (var e in comments)
        {
            reduced.Add(e);
            if (reduced.Count >= maxEventsPerTick)
            {
                return reduced;
            }
        }

        return reduced;
    }
}


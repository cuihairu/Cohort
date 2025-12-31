using Cohort.Engine.Abstractions;
using Cohort.Protocol.Models;

namespace Cohort.SampleGame;

public sealed class SampleGameModule : IGameModule
{
    private readonly string _sessionId;
    private long _tickId;

    private readonly Dictionary<string, long> _likesByUser = new(StringComparer.Ordinal);
    private readonly Dictionary<string, long> _giftsByUser = new(StringComparer.Ordinal);
    private readonly Dictionary<string, long> _commentsByUser = new(StringComparer.Ordinal);

    public SampleGameModule(string sessionId)
    {
        _sessionId = sessionId;
    }

    public string Name => "sample";

    public void ApplyEvents(long tickId, IReadOnlyList<AudienceEvent> events)
    {
        _tickId = tickId;
        foreach (var e in events)
        {
            switch (e.Kind)
            {
                case AudienceEventKind.Like:
                    {
                        var inc = 1;
                        if (!string.IsNullOrWhiteSpace(e.Text) && long.TryParse(e.Text, out var parsed))
                        {
                            inc = (int)parsed;
                        }
                        _likesByUser[e.UserId] = _likesByUser.TryGetValue(e.UserId, out var likeTotal) ? likeTotal + inc : inc;
                    }
                    break;
                case AudienceEventKind.Gift:
                    {
                        var inc = e.GiftCount ?? 1;
                        _giftsByUser[e.UserId] = _giftsByUser.TryGetValue(e.UserId, out var giftTotal) ? giftTotal + inc : inc;
                    }
                    break;
                case AudienceEventKind.Comment:
                    _commentsByUser[e.UserId] = _commentsByUser.TryGetValue(e.UserId, out var commentTotal) ? commentTotal + 1 : 1;
                    break;
            }
        }
    }

    public object GetStateSnapshot()
    {
        return new
        {
            game = Name,
            sessionId = _sessionId,
            tickId = _tickId,
            totals = new
            {
                users = _likesByUser.Keys.Union(_giftsByUser.Keys).Union(_commentsByUser.Keys).Distinct().Count(),
                likes = _likesByUser.Values.Sum(),
                gifts = _giftsByUser.Values.Sum(),
                comments = _commentsByUser.Values.Sum(),
            },
            top = new
            {
                likes = _likesByUser
                    .OrderByDescending(kv => kv.Value)
                    .Take(10)
                    .Select(kv => new { userId = kv.Key, value = kv.Value })
                    .ToArray(),
                gifts = _giftsByUser
                    .OrderByDescending(kv => kv.Value)
                    .Take(10)
                    .Select(kv => new { userId = kv.Key, value = kv.Value })
                    .ToArray(),
            }
        };
    }

    public void Dispose()
    {
    }
}

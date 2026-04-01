using Cohort.Protocol.Models;

namespace Cohort.Adapters.Abstractions;

public interface IAudienceFactionRule
{
    AudienceEvent Apply(AudienceEvent e);
}

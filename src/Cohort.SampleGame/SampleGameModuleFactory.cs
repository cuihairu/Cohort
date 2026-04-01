using Cohort.Engine.Abstractions;

namespace Cohort.SampleGame;

public sealed class SampleGameModuleFactory : IGameModuleFactory
{
    private readonly MatchStateStore _matchStore = new();

    public IGameModule Create(string sessionId) => new SampleGameModule(sessionId, _matchStore);
}

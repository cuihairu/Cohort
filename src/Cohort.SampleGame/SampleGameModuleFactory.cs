using Cohort.Engine.Abstractions;

namespace Cohort.SampleGame;

public sealed class SampleGameModuleFactory : IGameModuleFactory
{
    public IGameModule Create(string sessionId) => new SampleGameModule(sessionId);
}


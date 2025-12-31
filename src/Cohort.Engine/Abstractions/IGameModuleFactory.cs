namespace Cohort.Engine.Abstractions;

public interface IGameModuleFactory
{
    IGameModule Create(string sessionId);
}


namespace Cohort.Engine.Session;

public interface ISessionClient
{
    string ClientId { get; }
    ValueTask SendSnapshotAsync(SessionSnapshot snapshot, CancellationToken cancellationToken);
}


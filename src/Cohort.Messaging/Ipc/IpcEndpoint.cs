namespace Cohort.Messaging.Ipc;

public sealed record IpcEndpoint(
    string UnixSocketPath,
    string NamedPipeName
);


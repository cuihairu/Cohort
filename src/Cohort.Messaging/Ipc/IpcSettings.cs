namespace Cohort.Messaging.Ipc;

public sealed record IpcSettings(
    IpcTransport Transport = IpcTransport.Auto,
    string UnixSocketDir = "/tmp/cohort",
    string NamedPipePrefix = "cohort"
);


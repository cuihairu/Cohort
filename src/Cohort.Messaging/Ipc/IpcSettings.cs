namespace Cohort.Messaging.Ipc;

public sealed record IpcSettings(
    IpcTransport Transport = IpcTransport.Auto,
    string UnixSocketDir = "/tmp/cohort",
    string NamedPipePrefix = "cohort",
    string TcpHost = "127.0.0.1",
    int TcpGatewayToEnginePort = 27500,
    int TcpEngineToGatewayPort = 27501
);

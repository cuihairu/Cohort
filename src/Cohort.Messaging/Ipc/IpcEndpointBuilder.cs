namespace Cohort.Messaging.Ipc;

public static class IpcEndpointBuilder
{
    public static IpcDuplexEndpoints Build(IpcSettings settings)
    {
        var dir = string.IsNullOrWhiteSpace(settings.UnixSocketDir) ? "/tmp/cohort" : settings.UnixSocketDir;
        var prefix = string.IsNullOrWhiteSpace(settings.NamedPipePrefix) ? "cohort" : settings.NamedPipePrefix;

        var gwToEng = new IpcEndpoint(
            UnixSocketPath: Path.Combine(dir, $"{prefix}_gw_to_eng.sock"),
            NamedPipeName: $"{prefix}.gw_to_eng"
        );

        var engToGw = new IpcEndpoint(
            UnixSocketPath: Path.Combine(dir, $"{prefix}_eng_to_gw.sock"),
            NamedPipeName: $"{prefix}.eng_to_gw"
        );

        return new IpcDuplexEndpoints(gwToEng, engToGw);
    }
}


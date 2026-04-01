namespace Cohort.Messaging.Ipc;

public static class IpcEndpointBuilder
{
    public static IpcDuplexEndpoints Build(IpcSettings settings)
    {
        var dir = string.IsNullOrWhiteSpace(settings.UnixSocketDir) ? "/tmp/cohort" : settings.UnixSocketDir;
        var prefix = string.IsNullOrWhiteSpace(settings.NamedPipePrefix) ? "cohort" : settings.NamedPipePrefix;
        var tcpHost = string.IsNullOrWhiteSpace(settings.TcpHost) ? "127.0.0.1" : settings.TcpHost;

        var gwToEng = new IpcEndpoint(
            UnixSocketPath: Path.Combine(dir, $"{prefix}_gw_to_eng.sock"),
            NamedPipeName: $"{prefix}.gw_to_eng",
            TcpHost: tcpHost,
            TcpPort: settings.TcpGatewayToEnginePort,
            HttpUrl: NormalizeHttpUrl(settings.HttpGatewayToEngineUrl, "http://127.0.0.1:27600/gw-to-eng/")
        );

        var engToGw = new IpcEndpoint(
            UnixSocketPath: Path.Combine(dir, $"{prefix}_eng_to_gw.sock"),
            NamedPipeName: $"{prefix}.eng_to_gw",
            TcpHost: tcpHost,
            TcpPort: settings.TcpEngineToGatewayPort,
            HttpUrl: NormalizeHttpUrl(settings.HttpEngineToGatewayUrl, "http://127.0.0.1:27601/eng-to-gw/")
        );

        return new IpcDuplexEndpoints(gwToEng, engToGw);
    }

    private static string NormalizeHttpUrl(string? url, string fallback)
    {
        url = string.IsNullOrWhiteSpace(url) ? fallback : url.Trim();
        return url.EndsWith('/') ? url : $"{url}/";
    }
}

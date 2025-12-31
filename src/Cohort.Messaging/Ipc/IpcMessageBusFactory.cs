using System.Net.Sockets;
using Cohort.Messaging.Transports;

namespace Cohort.Messaging.Ipc;

public static class IpcMessageBusFactory
{
    public static IMessageBus CreateServer(IpcTransport transport, IpcEndpoint endpoint)
    {
        var auto = transport == IpcTransport.Auto;
        if (auto)
        {
            transport = Socket.OSSupportsUnixDomainSockets ? IpcTransport.UnixDomainSocket : IpcTransport.NamedPipe;
        }

        switch (transport)
        {
            case IpcTransport.UnixDomainSocket:
                try
                {
                    var bus = new UnixDomainSocketMessageBus(endpoint.UnixSocketPath);
                    bus.StartServer();
                    return bus;
                }
                catch
                {
                    if (!auto)
                    {
                        throw;
                    }
                    goto case IpcTransport.NamedPipe;
                }

            case IpcTransport.NamedPipe:
                {
                    var bus = new NamedPipeMessageBus(endpoint.NamedPipeName);
                    bus.StartServer();
                    return bus;
                }

            case IpcTransport.Tcp:
                {
                    EnsureTcpConfigured(endpoint);
                    var bus = new TcpMessageBus(endpoint.TcpHost, endpoint.TcpPort);
                    bus.StartServer();
                    return bus;
                }

            default:
                throw new ArgumentOutOfRangeException(nameof(transport), transport, null);
        }
    }

    public static IMessageBus CreateClient(IpcTransport transport, IpcEndpoint endpoint)
    {
        if (transport == IpcTransport.Auto)
        {
            transport = Socket.OSSupportsUnixDomainSockets ? IpcTransport.UnixDomainSocket : IpcTransport.NamedPipe;
        }

        return transport switch
        {
            IpcTransport.UnixDomainSocket => new UnixDomainSocketMessageBus(endpoint.UnixSocketPath),
            IpcTransport.NamedPipe => new NamedPipeMessageBus(endpoint.NamedPipeName),
            IpcTransport.Tcp => new TcpMessageBus(endpoint.TcpHost, EnsureTcpConfigured(endpoint).TcpPort),
            _ => throw new ArgumentOutOfRangeException(nameof(transport), transport, null),
        };
    }

    private static IpcEndpoint EnsureTcpConfigured(IpcEndpoint endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint.TcpHost))
        {
            throw new InvalidOperationException("IPC transport Tcp requires a non-empty TcpHost.");
        }
        if (endpoint.TcpPort is <= 0 or > 65535)
        {
            throw new InvalidOperationException("IPC transport Tcp requires a fixed TcpPort in range 1..65535.");
        }
        return endpoint;
    }
}

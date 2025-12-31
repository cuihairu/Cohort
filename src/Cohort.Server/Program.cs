using System.Net.WebSockets;
using System.Text.Json;
using Cohort.Engine.Abstractions;
using Cohort.Engine.Session;
using Cohort.Protocol;
using Cohort.Protocol.Messages;
using Cohort.Protocol.Models;
using Cohort.Server;
using Cohort.SampleGame;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IGameModuleFactory, SampleGameModuleFactory>();
builder.Services.AddSingleton(new SessionConfig());
builder.Services.AddSingleton<SessionManager>();

var app = builder.Build();
app.UseWebSockets();

app.MapGet("/", () => new { name = "Cohort", ok = true });
app.MapGet("/health", () => Results.Ok());
app.MapGet("/sessions", (SessionManager sessions) => sessions.GetDiagnostics());

app.MapPost("/ingress/test", async (IngressTestRequest req, SessionManager sessions) =>
{
    var sessionId = string.IsNullOrWhiteSpace(req.SessionId) ? sessions.CreateSessionId() : req.SessionId;
    var session = sessions.GetOrCreate(sessionId);

    var ingestTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    var ev = new AudienceEvent(
        EventId: req.EventId ?? $"test:{Guid.NewGuid():N}",
        Platform: req.Platform ?? "test",
        SessionId: sessionId,
        UserId: req.UserId ?? "anonymous",
        Kind: req.Kind ?? AudienceEventKind.Comment,
        IngestTimeMs: ingestTimeMs,
        Text: req.Text,
        GiftId: req.GiftId,
        GiftCount: req.GiftCount,
        GiftValue: req.GiftValue
    );

    await session.IngestAudienceEventAsync(ev);
    return Results.Ok(new { sessionId, eventId = ev.EventId, tick = session.TickId });
});

app.Map("/ws", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Expected WebSocket request.");
        return;
    }

    var sessions = context.RequestServices.GetRequiredService<SessionManager>();
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Cohort.WS");

    using var ws = await context.WebSockets.AcceptWebSocketAsync();

    ClientHello? hello;
    try
    {
        var helloText = await WebSocketHelpers.ReceiveTextAsync(ws, context.RequestAborted);
        hello = JsonSerializer.Deserialize<ClientHello>(helloText, ProtocolJson.SerializerOptions);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Failed to parse hello");
        await WebSocketHelpers.SendJsonAsync(ws, new ServerError(ProtocolTypes.Error, "Invalid hello"), context.RequestAborted);
        return;
    }

    if (hello == null || !string.Equals(hello.Type, ProtocolTypes.Hello, StringComparison.Ordinal))
    {
        await WebSocketHelpers.SendJsonAsync(ws, new ServerError(ProtocolTypes.Error, "First message must be hello"), context.RequestAborted);
        return;
    }

    var sessionId = string.IsNullOrWhiteSpace(hello.SessionId) ? sessions.CreateSessionId() : hello.SessionId!;
    var clientId = string.IsNullOrWhiteSpace(hello.ClientId) ? $"c_{Guid.NewGuid():N}" : hello.ClientId!;
    var session = sessions.GetOrCreate(sessionId);

    var client = new WsSessionClient(clientId, sessionId, ws, logger);
    await session.AddClientAsync(client);

    await WebSocketHelpers.SendJsonAsync(ws, new ServerWelcome(
        Type: ProtocolTypes.Welcome,
        SessionId: sessionId,
        ClientId: clientId,
        TickDurationMs: sessions.Config.TickDurationMs,
        InputDelayTicks: sessions.Config.InputDelayTicks,
        SnapshotEveryTicks: sessions.Config.SnapshotEveryTicks,
        ServerTimeMs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    ), context.RequestAborted);

    try
    {
        while (ws.State == WebSocketState.Open && !context.RequestAborted.IsCancellationRequested)
        {
            var text = await WebSocketHelpers.ReceiveTextAsync(ws, context.RequestAborted);
            using var doc = JsonDocument.Parse(text);
            var type = doc.RootElement.TryGetProperty("type", out var t) ? t.GetString() : null;

            if (string.Equals(type, ProtocolTypes.Ack, StringComparison.Ordinal))
            {
                var ack = doc.RootElement.Deserialize<ClientAck>(ProtocolJson.SerializerOptions);
                if (ack != null)
                {
                    await session.AckAsync(clientId, ack.LastAppliedTickId, ack.ClientTimeMs ?? 0);
                }
            }
            else if (string.Equals(type, ProtocolTypes.Ping, StringComparison.Ordinal))
            {
                await WebSocketHelpers.SendJsonAsync(ws, new ServerPong(ProtocolTypes.Pong, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()), context.RequestAborted);
            }
        }
    }
    catch (OperationCanceledException)
    {
    }
    catch (WebSocketException)
    {
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "WebSocket loop error");
    }
    finally
    {
        await session.RemoveClientAsync(clientId);
        try { await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None); } catch { }
    }
});

app.Run();

internal sealed record IngressTestRequest(
    string? SessionId,
    string? Platform,
    string? EventId,
    string? UserId,
    AudienceEventKind? Kind,
    string? Text,
    string? GiftId,
    int? GiftCount,
    int? GiftValue
);

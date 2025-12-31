using Cohort.Engine.Abstractions;
using Cohort.Engine.Session;
using Cohort.EngineHost;
using Cohort.SampleGame;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IGameModuleFactory, SampleGameModuleFactory>();
builder.Services.AddSingleton(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    return new SessionConfig(
        TickDurationMs: cfg.GetValue("Session:TickDurationMs", 100),
        InputDelayTicks: cfg.GetValue("Session:InputDelayTicks", 2),
        SnapshotEveryTicks: cfg.GetValue("Session:SnapshotEveryTicks", 1),
        MaxEventsPerTick: cfg.GetValue("Session:MaxEventsPerTick", 200),
        MaxLagTicks: cfg.GetValue("Session:MaxLagTicks", 50),
        ResyncCooldownMs: cfg.GetValue("Session:ResyncCooldownMs", 2000)
    );
});
builder.Services.AddHostedService<EngineIpcService>();

var app = builder.Build();

app.MapGet("/", () => new { name = "Cohort.EngineHost", ok = true });
app.MapGet("/health", () => Results.Ok());

app.Run();

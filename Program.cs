using WorkServiceFile;
using WsAppFile;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddActivatedSingleton<IWsConnections, WsConnections>();
builder.Services.AddTransient<WsProcessorScopedSrv>();
builder.Services.AddScoped<IWsProcessorScopedSrv, WsProcessorScopedSrv>();
builder.Services.AddHostedService<BgWebsocketProcessorService>();

var app = builder.Build();

var wsApp = new WsApp(app.Services.GetRequiredService<IWsConnections>());
app.UseWebSockets();

app.MapGet("/", () => "Hello World!");

app.MapGet("/ws", wsApp.Main);

app.Run();

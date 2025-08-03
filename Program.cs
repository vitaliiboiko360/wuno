using WorkServiceFile;
using WsAppFile;

var builder = WebApplication.CreateBuilder(args);


IHost host = Host.CreateDefaultBuilder(args).ConfigureServices(services =>
{
  services.AddTransient<WsProcessorScopedSrv>();
  services.AddScoped<IWsProcessorScopedSrv, WsProcessorScopedSrv>();
  services.AddHostedService<BgWebsocketProcessorService>();
}).Build();

await host.RunAsync();

var app = builder.Build();
var wsApp = new WsApp();

app.UseWebSockets();

app.MapGet("/", () => "Hello World!");

app.MapGet("/ws", wsApp.Main);

app.Run();

using WorkServiceFile;
using WsAppFile;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddActivatedSingleton<IWsConnections, WsConnections>();
builder.Services.AddTransient<WsProcessorScopedSrv>();
builder.Services.AddScoped<IWsProcessorScopedSrv, WsProcessorScopedSrv>();
builder.Services.AddHostedService<BgWebsocketProcessorService>();

// builder.WebHost.ConfigureKestrel(serverOptions =>
// {
//     serverOptions.ListenAnyIP(7192, listenOptions =>
//     {
//         listenOptions.UseHttps();
//     });
// });

builder.Logging.AddSimpleConsole(options =>
{
  options.IncludeScopes = false;
  options.SingleLine = true;
});

builder.Services.AddLogging(logging =>
{
  logging.ClearProviders();
  logging.AddConsole();
});

var app = builder.Build();

var wsApp = new WsApp(
  app.Services.GetRequiredService<ILogger<WsAppFile.WsApp>>(),
  app.Services.GetRequiredService<IWsConnections>()
);
var webSocketOptions = new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(5) };
app.UseWebSockets(webSocketOptions);

app.MapGet("/", () => "Hello World!");

app.Use(
  async (context, next) =>
  {
    if (context.Request.Path != "/ws")
    {
      await next(context);
      return;
    }
    if (context.WebSockets.IsWebSocketRequest)
    {
      var tcs = new TaskCompletionSource<object>();
      wsApp.Main(context, tcs);
      await tcs.Task;
    }
    else
    {
      context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
  }
);

app.Run();

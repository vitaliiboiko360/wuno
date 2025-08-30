using TableGameManagerFile;
using TableStateFile;
using WebsocketProcessorFile;
using WorkServiceFile;
using WsAppFile;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddActivatedSingleton<IWsConnections, WsConnections>();
builder.Services.AddTransient<WebsocketProcessor>();
builder.Services.AddScoped<IWebsocketProcessor, WebsocketProcessor>();
builder.Services.AddHostedService<BgWebsocketProcessorService>();
builder.Services.AddScoped<ITableGameManager, TableGameManager>();
builder.Services.AddScoped<ITableState, TableState>();

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
    // Console.WriteLine($"Processing Request with URL {context.Request.Path}");
    if (context.Request.Path != "/ws")
    {
      await next(context);
      return;
    }
    // string dictionaryString =
    //   "{"
    //   + String.Join(",", context.Request.Headers.Select(kv => $"\"{kv.Key}\":\"{kv.Value}\""))
    //   + "}";
    // Console.WriteLine($"Processing Request with URL {dictionaryString}");
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

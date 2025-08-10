using System;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorkServiceFile;

internal interface IWsProcessorScopedSrv
{
  Task DoWork(CancellationToken stoppingToken);
}

internal class WsProcessorScopedSrv : IWsProcessorScopedSrv
{
  private int stateCounter = 0;
  private readonly ILogger _logger;
  private IWsConnections _wsConnections;

  public WsProcessorScopedSrv(ILogger<WsProcessorScopedSrv> logger, IWsConnections wsConnections)
  {
    _logger = logger;
    _wsConnections = wsConnections;
  }

  private void onMessageRecive(
    System.Threading.Tasks.Task<System.Net.WebSockets.WebSocketReceiveResult> wsResult,
    int wsId
  )
  {
    try
    {
      if (wsResult.Result.MessageType == WebSocketMessageType.Close)
      {
        Console.WriteLine($"for ws ID= {wsId} detect connection is closed\n");
      }
      Console.WriteLine($"ws w ID=${wsId} recieved: {wsResult.Result.ToString()}");
    }
    catch
    {
      Console.WriteLine("Exception!!!! from onMessageRecive handler");
    }
  }

  public async Task DoWork(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      stateCounter++;
      // Console.WriteLine(
      //   "DO WORK FROM SCOPED SERVICE; CHANGED STATE BEFORE DELAY = {}",
      //   stateCounter
      // );
      // Console.WriteLine("WS CONNECTIONS ==== {}", _wsConnections.Size());
      for (var i = 0; i < _wsConnections.Size(); ++i)
      {
        var ws = _wsConnections.GetConnections()[i];
        // Console.WriteLine($"{i} ws CloseStatus = {ws.CloseStatusDescription}");
        // Console.WriteLine($"{i} ws CloseStatus = {ws.State}");
        if (ws.State == WebSocketState.Open)
        {
          var buffer = new byte[256];
          var task = ws.ReceiveAsync(buffer, CancellationToken.None);
          task.ContinueWith((wsRes) => onMessageRecive(wsRes, i));
          Console.WriteLine($"sending to {i}");
          await ws.SendAsync(
            Encoding.ASCII.GetBytes($"{i}_${stateCounter}"),
            WebSocketMessageType.Text,
            true,
            stoppingToken
          );
        }
      }
      await Task.Delay(5000, stoppingToken);
    }
  }
}

public class BgWebsocketProcessorService : BackgroundService
{
  private readonly ILogger<BgWebsocketProcessorService> _logger;

  public BgWebsocketProcessorService(
    IServiceProvider services,
    ILogger<BgWebsocketProcessorService> logger
  )
  {
    Services = services;
    _logger = logger;
  }

  public IServiceProvider Services { get; }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    Console.WriteLine("BgWebsocketProcessorService is running. ExecuteAsync");

    await DoWork(stoppingToken);
  }

  private async Task DoWork(CancellationToken stoppingToken)
  {
    Console.WriteLine("BgWebsocketProcessorService is working. DoWork");

    using (var scope = Services.CreateScope())
    {
      var scopedProcessingService =
        scope.ServiceProvider.GetRequiredService<WsProcessorScopedSrv>();

      await scopedProcessingService.DoWork(stoppingToken);
    }
  }

  public override async Task StopAsync(CancellationToken stoppingToken)
  {
    Console.WriteLine("BgWebsocketProcessorService is stopping. StopAsync");

    await base.StopAsync(stoppingToken);
  }
}

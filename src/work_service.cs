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

  public async Task DoWork(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      stateCounter++;
      _logger.LogInformation(
        "DO WORK FROM SCOPED SERVICE; CHANGED STATE BEFORE DELAY = {}",
        stateCounter
      );
      _logger.LogInformation("WS CONNECTIONS ==== {}", _wsConnections.Size());
      for (var i = 0; i < _wsConnections.Size(); ++i)
      {
        var ws = _wsConnections.GetConnections()[i];
        if (ws.State == WebSocketState.Open)
        {
          _logger.LogInformation($"sending to {i}");
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
    _logger.LogInformation("BgWebsocketProcessorService is running. ExecuteAsync");

    await DoWork(stoppingToken);
  }

  private async Task DoWork(CancellationToken stoppingToken)
  {
    _logger.LogInformation("BgWebsocketProcessorService is working. DoWork");

    using (var scope = Services.CreateScope())
    {
      var scopedProcessingService =
        scope.ServiceProvider.GetRequiredService<WsProcessorScopedSrv>();

      await scopedProcessingService.DoWork(stoppingToken);
    }
  }

  public override async Task StopAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("BgWebsocketProcessorService is stopping. StopAsync");

    await base.StopAsync(stoppingToken);
  }
}

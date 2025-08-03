using System;
using System.Net.WebSockets;
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

  public WsProcessorScopedSrv(ILogger<WsProcessorScopedSrv> logger)
  {
    _logger = logger;
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

  private TaskCompletionSource<object> _socketFinishedTcs;

  public void AddSocket(WebSocket webSocket, TaskCompletionSource<object> socketFinishedTcs)
  {
    _socketFinishedTcs = socketFinishedTcs;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Consume Scoped Service Hosted Service running.");

    await DoWork(stoppingToken);
  }

  private async Task DoWork(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Consume Scoped Service Hosted Service is working.");

    using (var scope = Services.CreateScope())
    {
      var scopedProcessingService =
        scope.ServiceProvider.GetRequiredService<WsProcessorScopedSrv>();

      await scopedProcessingService.DoWork(stoppingToken);
    }
  }

  public override async Task StopAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Consume Scoped Service Hosted Service is stopping.");

    await base.StopAsync(stoppingToken);
  }
}

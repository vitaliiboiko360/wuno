using System;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TableGameManagerFile;

namespace WorkServiceFile;

internal interface IWsProcessorScopedSrv
{
  Task DoWork(CancellationToken stoppingToken);
}

public struct WsMessage
{
  public WsMessage()
  {
    buffer = new byte[32];
  }

  public byte[] buffer;
}

internal class WsProcessorScopedSrv : IWsProcessorScopedSrv
{
  private int stateCounter = 0;
  private readonly ILogger _logger;
  private IWsConnections _wsConnections;
  private ITableGameManager _tableGameManager;
  private List<WsMessage> _wsMessages = new List<WsMessage>();

  public WsProcessorScopedSrv(
    ILogger<WsProcessorScopedSrv> logger,
    IWsConnections wsConnections,
    ITableGameManager tableGameManager
  )
  {
    _logger = logger;
    _wsConnections = wsConnections;
    _tableGameManager = tableGameManager;
  }

  private void onMessageRecieve(
    System.Threading.Tasks.Task<System.Net.WebSockets.WebSocketReceiveResult> wsResult,
    int wsId
  )
  {
    Console.WriteLine($"onMessageRecieve for ID ==: {wsId}");
    Console.WriteLine($"wsResult.IsCompleted ==: {wsResult.IsCompleted}");
    Console.WriteLine($"wsResult.IsCanceled ==: {wsResult.IsCanceled}");
    if (wsResult.IsCanceled)
    {
      return;
    }
    try
    {
      if (wsResult.Result.MessageType == WebSocketMessageType.Close)
      {
        Console.WriteLine($"For ws ID= {wsId} , MESSAGE TYPE is CLOSED\n");
      }
      if (wsResult.Result.MessageType == WebSocketMessageType.Text)
      {
        Console.WriteLine($"For ws ID= {wsId} , MESSAGE TYPE is TEXT\n");
      }
      if (wsResult.Result.MessageType == WebSocketMessageType.Binary)
      {
        Console.WriteLine($"For ws ID= {wsId} , MESSAGE TYPE is BINARY\n");
      }
      try
      {
        string msgRecived = Encoding.ASCII.GetString(_wsMessages[wsId].buffer);
        Console.WriteLine($"WS ID={wsId} recieved: {msgRecived}");
      }
      catch (Exception e)
      {
        Console.WriteLine($"Encoding.ASCII.GetString Exception For wsId={wsId} === {e.Message}");
      }
      _tableGameManager.ProcessMessage(_wsMessages[wsId]);
    }
    catch (Exception e)
    {
      Console.WriteLine($"WebSocketReceiveResult.Result Exception For wsId={wsId} === {e.Message}");
    }
  }

  public async Task DoWork(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      stateCounter++;
      for (var i = 0; i < _wsConnections.Size(); ++i)
      {
        var ws = _wsConnections.GetConnections()[i];
        if (ws.State == WebSocketState.Open)
        {
          if (i >= _wsMessages.Count)
          {
            Console.WriteLine($"adding message buffer for i= {i}");
            _wsMessages.Add(new WsMessage());
          }

          var task = ws.ReceiveAsync(_wsMessages[i].buffer, CancellationToken.None);
          int copyIndex = i;
          task.ContinueWith((wsRes) => onMessageRecieve(wsRes, copyIndex));
          Console.WriteLine($"sending to {i}");
          var prev = Encoding.ASCII.GetBytes($"{i}_${stateCounter}");
          var bufferToSend = new byte[8];
          Random rand = new Random();
          bufferToSend[0] = (byte)rand.NextInt64(255);
          bufferToSend[1] = (byte)rand.NextInt64(255);
          bufferToSend[2] = (byte)rand.NextInt64(255);
          bufferToSend[3] = (byte)rand.NextInt64(255);
          bufferToSend[4] = (byte)rand.NextInt64(255);
          bufferToSend[5] = (byte)rand.NextInt64(255);
          ws.SendAsync(bufferToSend, WebSocketMessageType.Binary, true, stoppingToken);
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

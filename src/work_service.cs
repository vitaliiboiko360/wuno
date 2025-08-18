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

  private List<WsMessage> _wsMessages = new List<WsMessage>();

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
    Console.WriteLine($"now the wsId is == ${wsId}");
    try
    {
      if (wsResult.Result.MessageType == WebSocketMessageType.Close)
      {
        Console.WriteLine($"for ws ID= {wsId} detect connection is closed\n");
        return;
      }
      string msgRecived = Encoding.ASCII.GetString(_wsMessages[wsId].buffer);
      Console.WriteLine($"ws w ID={wsId} recieved: {msgRecived}");
    }
    catch (Exception e)
    {
      Console.WriteLine($"Exception!!!! wsId={wsId} from onMessageRecive handler {e.Message}");
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
        // Console.WriteLine($"i==={i}");
        var ws = _wsConnections.GetConnections()[i];
        // Console.WriteLine($"{i} ws CloseStatus = {ws.CloseStatusDescription}");
        // Console.WriteLine($"{i} ws CloseStatus = {ws.State}");
        if (ws.State == WebSocketState.Open)
        {
          if (i >= _wsMessages.Count)
          {
            Console.WriteLine($"adding message buffer for i= {i}");
            _wsMessages.Add(new WsMessage());
          }

          var task = ws.ReceiveAsync(_wsMessages[i].buffer, CancellationToken.None);
          int copyIndex = i;
          task.ContinueWith((wsRes) => onMessageRecive(wsRes, copyIndex));
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
          ws.SendAsync(
            bufferToSend,
            WebSocketMessageType.Binary,
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

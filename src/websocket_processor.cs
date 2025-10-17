using System.ComponentModel;
using System.Net.WebSockets;
using System.Text;
using TableGameManagerFile;
using TableStateFile;

namespace WebsocketProcessorFile;

internal interface IWebsocketProcessor
{
  Task DoWork(CancellationToken stoppingToken);
}

public struct WsMessage
{
  public WebSocket ws;

  public WsMessage(WebSocket associatedWsConnection)
  {
    ws = associatedWsConnection;
    buffer = new byte[32];
  }

  public byte[] buffer;
}

internal class WebsocketProcessor : IWebsocketProcessor
{
  private readonly ILogger _logger;
  private IWsConnections _wsConnections;
  private ITableGameManager _tableGameManager;
  private List<WsMessage> _wsMessages = new List<WsMessage>();

  private Dictionary<uint, Task> _tasks = [];

  private ITableState _tableState;

  public WebsocketProcessor(
    ILogger<WebsocketProcessor> logger,
    IWsConnections wsConnections,
    ITableGameManager tableGameManager,
    ITableState tableState
  )
  {
    _logger = logger;
    _wsConnections = wsConnections;
    _tableGameManager = tableGameManager;
    _tableState = tableState;

    tableState.PropertyChanged += OnTableStateChanged;
  }

  private void onMessageRecieve(
    System.Threading.Tasks.Task<System.Net.WebSockets.WebSocketReceiveResult> wsResult,
    IWsConnection wsConnection
  )
  {
    uint wsId = wsConnection.ID;
    if (false)
    {
      Console.WriteLine($"onMessageRecieve for ID ==: {wsId}");
      Console.WriteLine($"wsResult.IsCompleted ==: {wsResult.IsCompleted}");
      Console.WriteLine($"wsResult.IsCanceled ==: {wsResult.IsCanceled}");
    }
    if (wsResult.IsCanceled)
    {
      return;
    }
    try
    {
      if (false)
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
      }
      try
      {
        string msgRecived = Encoding.ASCII.GetString(wsConnection.MessageBuffer);

        if (false)
        {
          Console.WriteLine($"WS ID={wsId} recieved: {msgRecived}");
        }
      }
      catch (Exception e)
      {
        Console.WriteLine($"Encoding.ASCII.GetString Exception For wsId={wsId} === {e.Message}");
      }
      _tableGameManager.ProcessConnection(wsConnection);
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
      byte[] allTableState = _tableState.getAllTableState();

      for (var i = 0; i < _wsConnections.Connections.Count; ++i)
      {
        var ws = _wsConnections.Connections[i];
        if (ws.WebSocket.State == WebSocketState.Open)
        {
          if (false)
          {
            CancellationTokenSource cts = new CancellationTokenSource();
            Task tsk;
            var isTaskExists = _tasks.TryGetValue(ws.ID, out tsk!);
            if (isTaskExists)
            {
              cts.Cancel();
              _tasks.Remove(ws.ID);
              cts = new CancellationTokenSource();
            }
            CancellationToken ct = new CancellationToken();
          }

          if (_tasks.ContainsKey(ws.ID))
          {
            Task currentTasks = _tasks[ws.ID];
            if (
              currentTasks.Status != TaskStatus.Running
              || currentTasks.Status != TaskStatus.RanToCompletion
              || currentTasks.Status != TaskStatus.WaitingForChildrenToComplete
            )
            {
              ws.WebSocket.SendAsync(
                _tableState.getAllTableState(),
                WebSocketMessageType.Binary,
                true,
                stoppingToken
              );
              continue;
            }
          }

          Task<WebSocketReceiveResult> task = ws.WebSocket.ReceiveAsync(
            ws.MessageBuffer,
            stoppingToken
          );
          task.ContinueWith(
            (wsRes) =>
            {
              onMessageRecieve(wsRes, ws);
              _tasks.Remove(ws.ID);
            },
            stoppingToken
          );

          _tasks.Add(ws.ID, task);

          ws.WebSocket.SendAsync(
            _tableState.getAllTableState(),
            WebSocketMessageType.Binary,
            true,
            stoppingToken
          );
        }
      }
      await Task.Delay(5000, stoppingToken);
    }
  }

  void OnTableStateChanged(object sender, PropertyChangedEventArgs e)
  {
    Console.WriteLine($"TABLE STATE PROPERTY CHANGED +++ {e.PropertyName}");
  }
}

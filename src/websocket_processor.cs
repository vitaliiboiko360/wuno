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
  private int stateCounter = 0;
  private readonly ILogger _logger;
  private IWsConnections _wsConnections;
  private ITableGameManager _tableGameManager;
  private List<WsMessage> _wsMessages = new List<WsMessage>();
  private byte[] _allTableCachedMessage = new byte[TableStateConstant.allStateMessageSize];
  private List<int> _cachedConnectionIds = new List<int>();

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
      byte[] allTableState = _tableState.getAllTableState();
      if (!_allTableCachedMessage.SequenceEqual(allTableState))
      {
        _allTableCachedMessage = allTableState;
        _cachedConnectionIds.Clear();
      }
      stateCounter++;
      for (var i = 0; i < _wsConnections.Size(); ++i)
      {
        var ws = _wsConnections.GetConnections()[i];
        if (ws.State == WebSocketState.Open)
        {
          if (i >= _wsMessages.Count)
          {
            Console.WriteLine($"adding message buffer for i= {i}");
            _wsMessages.Add(new WsMessage(ws));
          }

          var task = ws.ReceiveAsync(_wsMessages[i].buffer, CancellationToken.None);
          int copyIndex = i;
          task.ContinueWith((wsRes) => onMessageRecieve(wsRes, copyIndex));
          var prev = Encoding.ASCII.GetBytes($"{i}_${stateCounter}");

          if (_cachedConnectionIds.BinarySearch(i) < 0)
          {
            _cachedConnectionIds.Add(i);
          }
          ws.SendAsync(
            _tableState.getAllTableState(),
            WebSocketMessageType.Binary,
            true,
            stoppingToken
          );

          if (false)
          {
            Console.WriteLine($"sending to {i}");
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
      }
      await Task.Delay(5000, stoppingToken);
    }
  }

  void OnTableStateChanged(object sender, PropertyChangedEventArgs e)
  {
    Console.WriteLine($"TABLE STATE PROPERTY CHANGED +++ {e.PropertyName}");
    // for (var i = 0; i < _wsConnections.Size(); ++i)
    // {
    //   var ws = _wsConnections.GetConnections()[i];
    //   if (ws.State == WebSocketState.Open)
    //   {
    //     var bufferToSend = new byte[8];
    //     bufferToSend[0] = 1;
    //     if (e.PropertyName == "topSeat") { }
    //     ws.SendAsync(bufferToSend, WebSocketMessageType.Binary, true, CancellationToken.None);
    //   }
    // }
  }
}

using System.Net.WebSockets;
using Connections = System.Collections.Generic.List<IWsConnection>;

public interface IWsConnections
{
  public void AddSocket(WebSocket webSocket, TaskCompletionSource<object> tcs);
  public int Size();

  public List<WebSocket> GetConnections();

  public List<WebSocket> GetPlayerConnections();

  public void MoveToPlayersConnections(WebSocket wsPlayerConnection);

  public Connections Connections { get; set; }
}

public class WsConnections : IWsConnections
{
  List<WebSocket> _activeWebSockets = new List<WebSocket>();
  List<WebSocket> _playersConnections = new List<WebSocket>();

  List<IWsConnection> _connections = new List<IWsConnection>();
  List<TaskCompletionSource<object>> _tcsList = new List<TaskCompletionSource<object>>();

  public void AddSocket(WebSocket webSocket, TaskCompletionSource<object> tcs)
  {
    _activeWebSockets.Add(webSocket);
    _tcsList.Add(tcs);
    Connections.Add(new WsConnection(webSocket));
  }

  public WsConnections()
  {
    Connections = new Connections();
  }

  public int Size()
  {
    return _activeWebSockets.Count;
  }

  public Connections Connections { get; set; }

  public List<WebSocket> GetConnections()
  {
    return _activeWebSockets;
  }

  public List<WebSocket> GetPlayerConnections()
  {
    return _playersConnections;
  }

  public void MoveToPlayersConnections(WebSocket wsPlayerConnection)
  {
    _activeWebSockets.Remove(wsPlayerConnection);
    _playersConnections.Add(wsPlayerConnection);
  }
}

public interface IWsConnection
{
  static uint MessageBufferLength = 32;
  public WebSocket WebSocket { get; set; }
  public byte[] MessageBuffer { get; set; }
  public Guid Guid { get; set; }
  public uint ID { get; set; }
}

public class WsConnection : IWsConnection
{
  static uint nextIdCounter = 0;

  public WsConnection(WebSocket webSocket)
  {
    WebSocket = webSocket;
    MessageBuffer = new byte[IWsConnection.MessageBufferLength];
    ID = nextIdCounter++;
  }

  public WebSocket WebSocket { get; set; }
  public byte[] MessageBuffer { get; set; }
  public Guid Guid { get; set; }
  public uint ID { get; set; }
}

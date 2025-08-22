using System.Net.WebSockets;

public interface IWsConnections
{
  public void AddSocket(WebSocket webSocket, TaskCompletionSource<object> tcs);
  public int Size();

  public List<WebSocket> GetConnections();

  public List<WebSocket> GetPlayerConnections();

  public void MoveToPlayersConnections(WebSocket wsPlayerConnection);
}

public class WsConnections : IWsConnections
{
  List<WebSocket> _activeWebSockets = new List<WebSocket>();
  List<WebSocket> _playersConnections = new List<WebSocket>();
  List<TaskCompletionSource<object>> _tcsList = new List<TaskCompletionSource<object>>();

  public void AddSocket(WebSocket webSocket, TaskCompletionSource<object> tcs)
  {
    _activeWebSockets.Add(webSocket);
    _tcsList.Add(tcs);
  }

  public int Size()
  {
    return _activeWebSockets.Count;
  }

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

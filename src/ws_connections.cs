using System.Net.WebSockets;

public interface IWsConnections
{
  public void AddSocket(WebSocket webSocket, TaskCompletionSource<object> tcs);
  public int Size();

  public List<WebSocket> GetConnections();
}

public class WsConnections : IWsConnections
{
  List<WebSocket> _activeWebSockets = new List<WebSocket>();

  public void AddSocket(WebSocket webSocket, TaskCompletionSource<object> tcs)
  {
    _activeWebSockets.Add(webSocket);
  }

  public int Size()
  {
    return _activeWebSockets.Count;
  }

  public List<WebSocket> GetConnections()
  {
    return _activeWebSockets;
  }
}

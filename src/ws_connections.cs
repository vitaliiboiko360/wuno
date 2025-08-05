using System.Net.WebSockets;

public interface IWsConnections
{
  public void AddSocket(WebSocket webSocket);
}

public class WsConnections : IWsConnections
{
  List<WebSocket> _activeWebSockets = new List<WebSocket>();

  public void AddSocket(WebSocket webSocket)
  {
    _activeWebSockets.Add(webSocket);
  }

  public int Size()
  {
    return _activeWebSockets.Count;
  }
}

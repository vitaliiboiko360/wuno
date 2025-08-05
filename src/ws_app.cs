using System.Threading.Tasks;
using System.Web;
using WorkServiceFile;

namespace WsAppFile;

class WsApp
{
  readonly IWsConnections _wsConnections;

  public WsApp(IWsConnections wsConnections)
  {
    _wsConnections = wsConnections;
  }

  public async void Main(HttpContext context)
  {
    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
    var socketFinishedTcs = new TaskCompletionSource<object>();

    if (_wsConnections != null)
    {
      _wsConnections.AddSocket(webSocket);
    }
    await socketFinishedTcs.Task;
  }
}

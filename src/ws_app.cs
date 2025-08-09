using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WorkServiceFile;

namespace WsAppFile;

class WsApp
{
  readonly IWsConnections _wsConnections;
  private readonly ILogger _logger;

  public WsApp(ILogger<WsApp> logger, IWsConnections wsConnections)
  {
    _wsConnections = wsConnections;
    _logger = logger;
  }

  public async void Main(HttpContext context, TaskCompletionSource<object> tcs)
  {
    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
    _logger.LogInformation("we have ready websocket");
    if (_wsConnections != null)
    {
      _wsConnections.AddSocket(webSocket, tcs);
    }
    _logger.LogInformation("request is done, websocket might be gone already");
  }
}

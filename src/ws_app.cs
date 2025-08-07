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

  public async void Main(HttpContext context)
  {
    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
    var socketFinishedTcs = new TaskCompletionSource<object>();

    if (_wsConnections != null)
    {
      _wsConnections.AddSocket(webSocket);
    }

    
    _logger.LogInformation("before awaiting task for req");

    await socketFinishedTcs.Task;

    _logger.LogInformation("request is done, websocket might be gone already");
  }
}

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
    Console.WriteLine("we have ready websocket");

    _wsConnections.AddSocket(webSocket, tcs);

    Console.WriteLine("request is done, task is awaited, websocket might be passed already");
  }
}

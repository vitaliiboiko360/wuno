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
    // var socketFinishedTcs = new TaskCompletionSource<object>();

_logger.LogInformation("we have ready websocket");
    var buffer = new byte[1024 * 4];
    var receiveResult = await webSocket.ReceiveAsync(
        new ArraySegment<byte>(buffer), CancellationToken.None);
        
        _logger.LogInformation("before while loop");

    while (!receiveResult.CloseStatus.HasValue)
    {
      await webSocket.SendAsync(
          new ArraySegment<byte>(buffer, 0, receiveResult.Count),
          receiveResult.MessageType,
          receiveResult.EndOfMessage,
          CancellationToken.None);

      receiveResult = await webSocket.ReceiveAsync(
          new ArraySegment<byte>(buffer), CancellationToken.None);
    }
    return;

    if (_wsConnections != null)
    {
      _wsConnections.AddSocket(webSocket);
    }

    _logger.LogInformation("before awaiting task for req");
    // await socketFinishedTcs.Task;

    _logger.LogInformation("request is done, websocket might be gone already");
  }
}

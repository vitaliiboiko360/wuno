using System.Threading.Tasks;
using System.Web;

namespace WsAppFile;

class WsApp
{
  public async void Main(HttpContext context)
  {
    using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
    var socketFinishedTcs = new TaskCompletionSource<object>();

    BackgroundSocketProcessor.AddSocket(webSocket, socketFinishedTcs);

    await socketFinishedTcs.Task;
  }
}

using WorkServiceFile;

namespace TableGameManagerFile;

public interface ITableGameManager
{
  public void ProcessMessage(WsMessage wsMessage);
}

public class TableGameManager : ITableGameManager
{
  public void ProcessMessage(WsMessage wsMessage)
  {
    if (wsMessage.buffer[0] % 2 == 0)
    {
      Console.WriteLine($"WS Message Starts EVEN");
    }
    if (wsMessage.buffer[0] % 2 != 0)
    {
      Console.WriteLine($"WS Message Starts ODD");
    }
  }
}

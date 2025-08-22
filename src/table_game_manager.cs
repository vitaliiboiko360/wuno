using ManagerCommandsFile;
using SeatsFile;
using TableStateFile;
using WebsocketProcessorFile;

namespace TableGameManagerFile;

public interface ITableGameManager
{
  public void ProcessMessage(WsMessage wsMessage);
}

public class TableGameManager : ITableGameManager
{
  ITableState _tableState;

  TableGameManager(ITableState tableState)
  {
    _tableState = tableState;
  }

  public void ProcessMessage(WsMessage wsMessage)
  {
    if (wsMessage.buffer[0] % 2 == 0)
    {
      Console.WriteLine($"WS Message Starts EVEN == ${wsMessage.buffer[0]}");
    }
    if (wsMessage.buffer[0] % 2 != 0)
    {
      Console.WriteLine($"WS Message Starts ODD == ${wsMessage.buffer[0]}");
    }

    uint cmdByte = wsMessage.buffer[0];

    if (cmdByte == (uint)ManagerCommands.Table)
    {
      ProcessTableMessage(wsMessage);
    }
    if (cmdByte == (uint)ManagerCommands.Game)
    {
      ProcessGameMessage(wsMessage);
    }
  }

  void ProcessTableMessage(WsMessage wsMessage)
  {
    uint actByte = wsMessage.buffer[1];
    if (actByte == (uint)wsMessage.buffer[1])
    {
      uint place = wsMessage.buffer[2];
      if (place == (uint)Seat.Left)
      {
        _tableState.allocateSeat(Seat.Left);
      }
      if (place == (uint)Seat.Right)
      {
        _tableState.allocateSeat(Seat.Right);
      }
      if (place == (uint)Seat.Top)
      {
        _tableState.allocateSeat(Seat.Top);
      }
      if (place == (uint)Seat.Bottom)
      {
        _tableState.allocateSeat(Seat.Bottom);
      }
    }
  }

  void ProcessGameMessage(WsMessage wsMessage) { }
}

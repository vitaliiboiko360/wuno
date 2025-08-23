using System.Net.WebSockets;
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
  IWsConnections _wsConnections;

  public TableGameManager(ITableState tableState, IWsConnections wsConnections)
  {
    _tableState = tableState;
    _wsConnections = wsConnections;
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
    if (actByte == (uint)TableActionsIncoming.RequestSeat)
    {
      uint place = wsMessage.buffer[2];
      if (place == (uint)Seat.Left)
      {
        if (_tableState.allocateSeat(Seat.Left))
        {
          approveSeatRequest(wsMessage.ws, place);
        }
      }
      if (place == (uint)Seat.Right)
      {
        if (_tableState.allocateSeat(Seat.Right))
        {
          approveSeatRequest(wsMessage.ws, place);
        }
      }
      if (place == (uint)Seat.Top)
      {
        if (_tableState.allocateSeat(Seat.Top))
        {
          approveSeatRequest(wsMessage.ws, place);
        }
      }
      if (place == (uint)Seat.Bottom)
      {
        if (_tableState.allocateSeat(Seat.Bottom))
        {
          approveSeatRequest(wsMessage.ws, place);
        }
      }
    }
  }

  void ProcessGameMessage(WsMessage wsMessage) { }

  void approveSeatRequest(WebSocket webSocket, uint seat)
  {
    var arrayToSend = new byte[8];
    arrayToSend[0] = (byte)ManagerCommands.Table;
    arrayToSend[1] = (byte)TableActionsOutcoming.GrantSeat;
    webSocket.SendAsync(arrayToSend, WebSocketMessageType.Binary, true, CancellationToken.None);
    Array.Clear(arrayToSend, 0, arrayToSend.Length);
    arrayToSend[0] = (byte)ManagerCommands.Table;
    arrayToSend[1] = (byte)TableActionsOutcoming.TakeSeat;
    arrayToSend[2] = (byte)seat;
    for (var i = 0; i < _wsConnections.GetConnections().Count; i++)
    {
      var ws = _wsConnections.GetConnections()[i];
      if (ws == webSocket)
      {
        continue;
      }
      ws.SendAsync(arrayToSend, WebSocketMessageType.Binary, true, CancellationToken.None);
    }
  }
}

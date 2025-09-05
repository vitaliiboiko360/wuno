using System.Diagnostics;
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
    if (cmdByte == (uint)ManagerCommands.ClientID)
    {
      ProcessClientIDMessage(wsMessage);
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
    arrayToSend[2] = (byte)seat;
    var playerSeatInfo = _tableState.getPlayerSeatInfo(seat);
    arrayToSend[3] = playerSeatInfo.colorIndex;
    arrayToSend[4] = playerSeatInfo.avatarIndex;
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
      ws.SendAsync(
        _tableState.getAllTableState(),
        WebSocketMessageType.Binary,
        true,
        CancellationToken.None
      );
    }
  }

  void ProcessClientIDMessage(WsMessage wsMessage)
  {
    uint actByte = wsMessage.buffer[1];
    if (actByte == (uint)TableActionsIncoming.GetNewClientGuid)
    {
      Guid newGuid = Guid.NewGuid();
      byte[] clientIDMessage = new byte[18];
      clientIDMessage[0] = (byte)ManagerCommands.ClientID;
      Array.Copy(newGuid.ToByteArray(), 0, clientIDMessage, 1, 16);
      wsMessage.ws.SendAsync(
        clientIDMessage,
        WebSocketMessageType.Binary,
        true,
        CancellationToken.None
      );
      Console.WriteLine($"Just sent new client ID == {newGuid}");
    }
    if (actByte == (uint)TableActionsIncoming.SetOldClientGuid)
    {
      const int guidLength = 16;
      byte[] clientID = new byte[guidLength];
      Array.Copy(wsMessage.buffer, 2, clientID, 0, guidLength);
      Guid guid = new Guid(clientID);
      Console.WriteLine($"Connection is already have ID == {guid}");
    }
  }
}

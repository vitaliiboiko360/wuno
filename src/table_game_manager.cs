using System.Diagnostics;
using System.Net.WebSockets;
using ManagerCommandsFile;
using SeatsFile;
using TableStateFile;
using WebsocketProcessorFile;

namespace TableGameManagerFile;

public interface ITableGameManager
{
  public void ProcessConnection(IWsConnection wsConnection);
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

  public void ProcessConnection(IWsConnection wsConnection)
  {
    if (wsConnection.MessageBuffer[0] % 2 == 0)
    {
      Console.WriteLine($"WS Message Starts EVEN == ${wsConnection.MessageBuffer[0]}");
    }
    if (wsConnection.MessageBuffer[0] % 2 != 0)
    {
      Console.WriteLine($"WS Message Starts ODD == ${wsConnection.MessageBuffer[0]}");
    }

    uint cmdByte = wsConnection.MessageBuffer[0];

    if (cmdByte == (uint)ManagerCommands.Table)
    {
      ProcessTableMessage(wsConnection);
    }
    if (cmdByte == (uint)ManagerCommands.Game)
    {
      ProcessGameMessage(wsConnection);
    }
    if (cmdByte == (uint)ManagerCommands.ClientID)
    {
      ProcessClientIDMessage(wsConnection);
    }
  }

  void ProcessTableMessage(IWsConnection wsConnection)
  {
    uint actByte = wsConnection.MessageBuffer[1];
    if (actByte == (uint)TableActionsIncoming.RequestSeat)
    {
      uint place = wsConnection.MessageBuffer[2];
      if (place == (uint)Seat.Left)
      {
        if (_tableState.allocateSeat(Seat.Left))
        {
          approveSeatRequest(wsConnection.WebSocket, place, wsConnection);
        }
      }
      if (place == (uint)Seat.Right)
      {
        if (_tableState.allocateSeat(Seat.Right))
        {
          approveSeatRequest(wsConnection.WebSocket, place, wsConnection);
        }
      }
      if (place == (uint)Seat.Top)
      {
        if (_tableState.allocateSeat(Seat.Top))
        {
          approveSeatRequest(wsConnection.WebSocket, place, wsConnection);
        }
      }
      if (place == (uint)Seat.Bottom)
      {
        if (_tableState.allocateSeat(Seat.Bottom))
        {
          approveSeatRequest(wsConnection.WebSocket, place, wsConnection);
        }
      }
    }
  }

  void ProcessGameMessage(IWsConnection wsConnection) { }

  void approveSeatRequest(WebSocket webSocket, uint seat, IWsConnection wsConnection)
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

    if (wsConnection.Guid != Guid.Empty)
    {
      _tableState.playerConnections.Add(wsConnection.Guid, (Seat)seat);
    }
  }

  void sendPlayerSeatIfPlayer(IWsConnection wsConnection)
  {
    if (_tableState.playerConnections.ContainsKey(wsConnection.Guid))
    {
      var seat = _tableState.playerConnections[wsConnection.Guid];
      byte[] arrayToSend = new byte[8];
      arrayToSend[0] = (byte)ManagerCommands.Table;
      arrayToSend[1] = (byte)TableActionsOutcoming.GrantSeat;
      arrayToSend[2] = (byte)seat;
      wsConnection.WebSocket.SendAsync(arrayToSend, WebSocketMessageType.Binary, true, CancellationToken.None);
    }
  }

  void ProcessClientIDMessage(IWsConnection wsConnection)
  {
    uint actByte = wsConnection.MessageBuffer[1];
    if (actByte == (uint)TableActionsIncoming.GetNewClientGuid)
    {
      Guid newGuid = Guid.NewGuid();
      byte[] clientIDMessage = new byte[18];
      clientIDMessage[0] = (byte)ManagerCommands.ClientID;
      Array.Copy(newGuid.ToByteArray(), 0, clientIDMessage, 1, 16);
      wsConnection.WebSocket.SendAsync(
        clientIDMessage,
        WebSocketMessageType.Binary,
        true,
        CancellationToken.None
      );
      wsConnection.Guid = newGuid;
      Console.WriteLine($"Just sent new client ID == {newGuid}");
    }
    if (actByte == (uint)TableActionsIncoming.SetOldClientGuid)
    {
      const int guidLength = 16;
      byte[] clientID = new byte[guidLength];
      Array.Copy(wsConnection.MessageBuffer, 2, clientID, 0, guidLength);
      Guid guid = new Guid(clientID);
      wsConnection.Guid = guid;
      Console.WriteLine($"Connection is already have ID == {guid}");
    }
  }
}

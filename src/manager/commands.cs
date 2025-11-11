namespace ManagerCommandsFile;

enum ManagerCommands
{
  NotAssigned,
  Table,
  Game,
  ClientID,
}

enum TableActionsIncoming
{
  None,
  RequestSeat,
  FreeSeat,
  GetNewClientGuid,
  SetOldClientGuid,
  CheckPlayerSeat,
  RequestInitTable = 6,
}

enum TableActionsOutcoming
{
  None,
  GrantSeat,
  FreeSeat,
  TakeSeat,
  AllTableState,
  RequestInitTable = 6,
}

public enum Seat
{
  Unassigned,
  Bottom,
  Left,
  Top,
  Right,
}

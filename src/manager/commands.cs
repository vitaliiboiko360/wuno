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
}

enum TableActionsOutcoming
{
  None,
  GrantSeat,
  FreeSeat,
  GetNewClientGuid,
  SetOldClientGuid,
  TakeSeat,
  AllTableState,
}

public enum Seat
{
  Unassigned,
  Bottom,
  Left,
  Top,
  Right,
}

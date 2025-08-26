namespace ManagerCommandsFile;

enum ManagerCommands
{
  NotAssigned,
  Table,
  Game,
}

enum TableActionsIncoming
{
  None,
  RequestSeat,
  FreeSeat,
}

enum TableActionsOutcoming
{
  None,
  GrantSeat,
  FreeSeat,
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

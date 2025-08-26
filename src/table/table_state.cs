using System.ComponentModel;
using System.Runtime.CompilerServices;
using ManagerCommandsFile;

namespace TableStateFile;

public class TableStateConstant
{
  public const int allStateMessageSize = 16;
}

public interface ITableState
{
  public bool allocateSeat(Seat seat);
  public void freeSeat(Seat seat);

  public event PropertyChangedEventHandler PropertyChanged;
  public byte[] getAllTableState();

  public PlayerSeatInfo getPlayerSeatInfo(uint seat);
}

public class TableState : ITableState, INotifyPropertyChanged
{
  bool bottomSeat = false;
  bool leftSeat = false;
  bool topSeat = false;
  bool rightSeat = false;

  PlayerSeatInfo[] playerInfos = new PlayerSeatInfo[4];

  public event PropertyChangedEventHandler PropertyChanged;

  protected virtual void OnPropertyChanged(string propertyName)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }

  public bool allocateSeat(Seat seat)
  {
    if (seat == Seat.Bottom && !bottomSeat)
    {
      bottomSeat = true;
      playerInfos[0].assignRandomName();
      OnPropertyChanged(nameof(bottomSeat));
      return true;
    }
    if (seat == Seat.Left && !leftSeat)
    {
      leftSeat = true;
      playerInfos[1].assignRandomName();
      OnPropertyChanged(nameof(leftSeat));
      return true;
    }
    if (seat == Seat.Top && !topSeat)
    {
      topSeat = true;
      playerInfos[2].assignRandomName();
      OnPropertyChanged(nameof(topSeat));
      return true;
    }
    if (seat == Seat.Right && !rightSeat)
    {
      rightSeat = true;
      playerInfos[3].assignRandomName();
      OnPropertyChanged(nameof(rightSeat));
      return true;
    }
    return false;
  }

  public void freeSeat(Seat seat)
  {
    if (seat == Seat.Bottom)
    {
      bottomSeat = false;
      OnPropertyChanged(nameof(bottomSeat));
    }
    if (seat == Seat.Left)
    {
      leftSeat = false;
      OnPropertyChanged(nameof(leftSeat));
    }
    if (seat == Seat.Top)
    {
      topSeat = false;
      OnPropertyChanged(nameof(topSeat));
    }
    if (seat == Seat.Right)
    {
      rightSeat = false;
      OnPropertyChanged(nameof(rightSeat));
    }
  }

  public byte[] getAllTableState()
  {
    var ret = new byte[TableStateConstant.allStateMessageSize];
    ret[0] = (byte)ManagerCommands.Table;
    ret[1] = (byte)TableActionsOutcoming.AllTableState;
    ret[2] = (byte)Seat.Bottom;
    ret[3] = playerInfos[0].colorIndex;
    ret[4] = playerInfos[0].avatarIndex;
    ret[5] = (byte)Seat.Left;
    ret[6] = playerInfos[1].colorIndex;
    ret[7] = playerInfos[1].avatarIndex;
    ret[8] = (byte)Seat.Top;
    ret[9] = playerInfos[2].colorIndex;
    ret[10] = playerInfos[2].avatarIndex;
    ret[11] = (byte)Seat.Right;
    ret[12] = playerInfos[3].colorIndex;
    ret[13] = playerInfos[3].avatarIndex;
    return ret;
  }

  public PlayerSeatInfo getPlayerSeatInfo(uint seat)
  {
    if (seat >= (uint)Seat.Bottom && seat <= (uint)Seat.Right)
    {
      return playerInfos[(int)seat-1];
    }
    return new PlayerSeatInfo();
  }
}

public struct PlayerSeatInfo
{
  public byte colorIndex;
  public byte avatarIndex;
  public String displayName;
  public bool isAssigned = false;

  public PlayerSeatInfo()
  {
    clearInfo();
  }

  public void assignRandomName()
  {
    isAssigned = true;
    colorIndex = (byte)((byte)new Random().NextInt64(9) + 1);
    avatarIndex = (byte)((byte)new Random().NextInt64(9) + 1);
    displayName = displayName + new Random().NextInt64(10).ToString();
  }

  public void clearInfo()
  {
    isAssigned = false;
    colorIndex = 0;
    avatarIndex = 0;
    displayName = "player";
  }
}

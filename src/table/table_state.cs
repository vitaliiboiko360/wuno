using System.ComponentModel;
using System.Runtime.CompilerServices;
using SeatsFile;

namespace TableStateFile;

public interface ITableState
{
  public bool allocateSeat(Seat seat);
  public void freeSeat(Seat seat);

  public event PropertyChangedEventHandler PropertyChanged;
}

public class TableState : ITableState, INotifyPropertyChanged
{
  bool topSeat = false;
  bool rightSeat = false;
  bool bottomSeat = false;
  bool leftSeat = false;

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
    colorIndex = (byte)new Random().NextInt64(10);
    avatarIndex = (byte)new Random().NextInt64(10);
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
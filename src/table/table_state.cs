using System.ComponentModel;
using SeatsFile;

namespace TableStateFile;

interface ITableState
{
  public bool allocateSeat(Seat seat);
  public void freeSeat(Seat seat);

  public event PropertyChangedEventHandler PropertyChanged;
}

class TableState : ITableState, INotifyPropertyChanged
{
  bool topSeat = false;
  bool rightSeat = false;
  bool bottomSeat = false;
  bool leftSeat = false;

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
      OnPropertyChanged(nameof(bottomSeat));
      return true;
    }
    if (seat == Seat.Left && !leftSeat)
    {
      leftSeat = true;
      OnPropertyChanged(nameof(leftSeat));
      return true;
    }
    if (seat == Seat.Top && !topSeat)
    {
      topSeat = true;
      OnPropertyChanged(nameof(topSeat));
      return true;
    }
    if (seat == Seat.Right && !rightSeat)
    {
      rightSeat = true;
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

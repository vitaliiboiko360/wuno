using SeatsFile;

namespace TableStateFile;

interface ITableState
{
  public void allocateSeat(Seat seat);
  public void freeSeat(Seat seat);
}

class TableState : ITableState
{
  bool topSeat = false;
  bool rightSeat = false;
  bool bottomSeat = false;
  bool leftSeat = false;

  public void allocateSeat(Seat seat)
  {
    if (seat == Seat.Bottom)
    {
      bottomSeat = true;
    }
    if (seat == Seat.Left)
    {
      leftSeat = true;
    }
    if (seat == Seat.Top)
    {
      topSeat = true;
    }
    if (seat == Seat.Right)
    {
      rightSeat = true;
    }
  }

  public void freeSeat(Seat seat)
  {
    if (seat == Seat.Bottom)
    {
      bottomSeat = false;
    }
    if (seat == Seat.Left)
    {
      leftSeat = true;
    }
    if (seat == Seat.Top)
    {
      topSeat = true;
    }
    if (seat == Seat.Right)
    {
      rightSeat = true;
    }
  }
}

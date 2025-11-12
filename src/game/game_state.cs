public class UnoConstants
{
  public const int handInitCardNumber = 7;
  public const int playersNumber = 4;
}

public interface IGameState
{
  public void registerPlayerMove(uint seat, byte[] card);
  public byte[] getPlayerInitHand(uint seat);
  public byte getNextCard();
  public byte[] getNextTwoCards();
  public byte[] getNextFourCards();
  public byte[] getPlayerCards(uint seat);
}

public class GameState : IGameState
{
  PlayerHand[] playerCards = new PlayerHand[UnoConstants.playersNumber];

  public void registerPlayerMove(uint seat, byte[] card) { }

  public byte[] getPlayerInitHand(uint seat)
  {
    var ret = new byte[UnoConstants.handInitCardNumber];
    return ret;
  }

  public byte getNextCard()
  {
    byte ret = 0;
    return ret;
  }

  public byte[] getNextTwoCards()
  {
    byte[] ret = new byte[2];
    return ret;
  }

  public byte[] getNextFourCards()
  {
    byte[] ret = new byte[4];
    return ret;
  }

  public byte[] getPlayerCards(uint seat)
  {
    return playerCards[seat].cards;
  }
}

public class PlayerHand
{
  public byte[] cards;
  public int length;

  public void addCards(byte[] cards, int length)
  {
    Buffer.BlockCopy(cards, 0, this.cards, 0, length);
    this.length = length;
  }
}

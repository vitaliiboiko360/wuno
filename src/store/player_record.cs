using System.Reflection.Metadata;
using SQLite;

public class Player
{
  [PrimaryKey, AutoIncrement]
  public int Id { get; set; }
  public int AvatarId { get; set; }
  public int ColorId { get; set; }
  public Blob Guid { get; set; }
  public string Name { get; set; }
}

public class Valuation
{
  [PrimaryKey, AutoIncrement]
  public int Id { get; set; }

  [Indexed]
  public int StockId { get; set; }
  public DateTime Time { get; set; }
  public decimal Price { get; set; }

  [Ignore]
  public string IgnoreField { get; set; }
}

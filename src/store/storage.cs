using SQLite;

class Storage
{
  // Get an absolute path to the database file
  String databasePath = Path.Combine(Environment.CurrentDirectory, "data.db");
  SQLiteAsyncConnection db;

  Storage()
  {
    db = new SQLiteAsyncConnection(databasePath);
    db.CreateTableAsync<Player>();
    Console.WriteLine("Table created!");
  }
}

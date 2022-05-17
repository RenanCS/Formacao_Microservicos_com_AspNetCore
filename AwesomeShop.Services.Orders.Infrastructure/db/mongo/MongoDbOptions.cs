namespace AwesomeShop.Services.Orders.Infrastructure.db.mongo
{
    public class MongoDbOptions
    {
        public MongoDbOptions(string database, string connectionString)
        {
            Database = database;
            ConnectionString = connectionString;
        }

        public string Database { get; private set; }
        public string ConnectionString { get; private set; }

    }
}

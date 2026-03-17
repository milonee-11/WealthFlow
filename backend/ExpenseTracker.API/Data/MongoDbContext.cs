using MongoDB.Driver;
using ExpenseTracker.API.Models;

namespace ExpenseTracker.API.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    public IMongoCollection<User> Users { get; }
    public IMongoCollection<Expense> Expenses { get; }
    public IMongoCollection<Alert> Alerts { get; }

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetValue<string>("MongoDB:ConnectionString");
        var databaseName = configuration.GetValue<string>("MongoDB:DatabaseName");
        
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
        
        Users = _database.GetCollection<User>("Users");
        Expenses = _database.GetCollection<Expense>("Expenses");
        Alerts = _database.GetCollection<Alert>("Alerts");
    }
}
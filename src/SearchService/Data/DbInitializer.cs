using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Data;

public class DbInitializer
{

    public static async Task InitDb(WebApplication app)
    {
        await DB.InitAsync("SearchDb",
     MongoClientSettings.FromConnectionString(
        app.Configuration.GetConnectionString("MongoDBConnection")
        ));

        await DB.Index<Item>()
            .Key(i => i.Make, KeyType.Text)
            .Key(i => i.Model, KeyType.Text)
            .Key(i => i.Color, KeyType.Text)
            .CreateAsync();

        var count = await DB.CountAsync<Item>();
        if (count == 0)
        {
            Console.WriteLine("Seeding initial data...");
            var itemData = File.ReadAllText("Data/auction.json");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var items = JsonSerializer.Deserialize<List<Item>>(itemData, options);
            await DB.SaveAsync(items!);
        }
    }
}

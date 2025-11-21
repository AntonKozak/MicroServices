using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services;

public class AuctionServiceHttpClient
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    public AuctionServiceHttpClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<List<Item>> GetItemsForSearchDB(string searchTerm)
    {
        var lastUpdatedAuctions = await DB.Find<Item, string>()
            .Sort(X => X.Descending(i => i.UpdatedAt))
            .Project(i => i.UpdatedAt.ToString())
            .ExecuteAsync();
        return await _httpClient.GetFromJsonAsync<List<Item>>(
            $"{_configuration["AuctionServiceUrl"]}/api/auctions?date={lastUpdatedAuctions.FirstOrDefault()}"
        ) ?? new List<Item>();
    }
}

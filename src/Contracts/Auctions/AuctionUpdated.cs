
namespace Contracts.Auctions;

public class AuctionUpdated
{

    public string Id { get; set; } = null!;

    public string Make { get; set; } = null!;

    public string Model { get; set; } = null!;

    public string Color { get; set; } = null!;

    public int Mileage { get; set; }

    public int Year { get; set; }
}

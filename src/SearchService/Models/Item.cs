using MongoDB.Entities;

namespace SearchService.Models;

public class Item : Entity
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime AuctionEnd { get; set; }

    public string Seller { get; set; } = null!;
    public string Winner { get; set; } = null!;

    public string Make { get; set; } = null!;
    public string Model { get; set; } = null!;

    public int Year { get; set; }
    public string Color { get; set; } = null!;
    public int Mileage { get; set; }

    public string ImageUrl { get; set; } = null!;
    public string Status { get; set; } = null!;

    public int ReservePrice { get; set; }

    public int? SoldAmount { get; set; }
    public int? CurrentHighBid { get; set; }
}

namespace AuctionService.Entities;

public class Auction
{

    public Guid Id { get; set; }

    public int ReservePrice { get; set; } = 0;

    // username from JWT claim
    public string Seller { get; set; } = null!;

    // username of winner
    public string? Winner { get; set; }

    public int? SoldAmount { get; set; }

    public int? CurrentHighBid { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime AuctionEnd { get; set; }

    public Status Status { get; set; } = Status.Live;

    // Navigation
    public Item Item { get; set; } = null!;

}

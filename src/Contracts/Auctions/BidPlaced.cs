namespace Contracts.Auctions;

public class BidPlaced
{
    public string Id { get; set; } = null!;
    public string AuctionId { get; set; } = null!;
    public string Bidder { get; set; } = null!;
    public int Amount { get; set; }
    public DateTime BidTime { get; set; }
    public string BidStatus { get; set; } = null!;
    public List<string> PreviousBidders { get; set; } = new();
}

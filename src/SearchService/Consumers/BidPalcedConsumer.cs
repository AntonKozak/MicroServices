
using Contracts.Auctions;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class BidPalcedConsumer : IConsumer<BidPlaced>
{
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("BidPlaced event  - - - - - > > > > >received in SearchService");
        var auction = await DB.Find<Item>().OneAsync(context.Message.AuctionId);

        if (auction == null)
        {
            Console.WriteLine("Auction not found");
            return;
        }

        if (context.Message.BidStatus.Contains("Accepted")
        && context.Message.Amount > auction.CurrentHighBid)
        {
            Console.WriteLine($"Auction {auction.ID} updated with new high bid: {auction.CurrentHighBid}");
            auction.CurrentHighBid = context.Message.Amount;
            await auction.SaveAsync();
        }
    }
}

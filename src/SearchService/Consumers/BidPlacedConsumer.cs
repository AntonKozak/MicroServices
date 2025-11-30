using Contracts.Auctions;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("🎧 Create bid!");
        Console.WriteLine($"   AuctionId: {context.Message.AuctionId}");
        Console.WriteLine($"   Amount: {context.Message.Amount}");

        var auction = await DB.Find<Item>().OneAsync(context.Message.AuctionId);

        if (context.Message.Amount > auction.CurrentHighBid)
        {
            auction.CurrentHighBid = context.Message.Amount;
            await auction.SaveAsync();
            Console.WriteLine("✅ Update bid!");
        }
    }
}

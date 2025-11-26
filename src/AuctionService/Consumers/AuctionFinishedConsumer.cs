using AuctionService.Data;
using AuctionService.Entities;
using Contracts.Auctions;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
    private readonly AuctionDbContext _dbContext;
    public AuctionFinishedConsumer(AuctionDbContext dbContext)
    {
        _dbContext = dbContext;

    }

    public async Task Consume(ConsumeContext<AuctionFinished> auctionContext)
    {
        Console.WriteLine("AuctionFinishedConsumer reached - - - - - > > > > AuctionFinishedConsumer");
        var auction = await _dbContext.Auctions.FindAsync(auctionContext.Message.AuctionId);

        if (auctionContext.Message.ItemSold)
        {
            auction!.Winner = auctionContext.Message.Winner;
            auction.SoldAmount = auctionContext.Message.Amount;
        }

        auction!.Status = auction.SoldAmount > auction.ReservePrice ? Status.Finished : Status.ReserveNotMet;

        await _dbContext.SaveChangesAsync();
    }
}

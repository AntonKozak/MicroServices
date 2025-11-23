using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionDeletedConsumer : IConsumer<Contracts.AuctionDeleted>
{
    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {
        Console.WriteLine("- - - - - > > > Consuming AuctionDeleted", context.Message.Id);

        var result = await DB.DeleteAsync<Item>(context.Message.Id);

        if (!result.IsAcknowledged)
        {
            Console.WriteLine("----->>> Deletion not acknowledged in AuctionDeletedConsumer");
            throw new Exception("Deletion not acknowledged in AuctionDeletedConsumer");
        }
    }
}

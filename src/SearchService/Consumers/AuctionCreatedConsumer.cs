using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly IMapper _mapper;
    public AuctionCreatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine("- - - - - > > > Consuming AuctionCreated", context.Message.Id);
        var item = _mapper.Map<Item>(context.Message);

        // Try to fake Consuming fault queues and retries
        if (item.Model == "Foo")
        {
            Console.WriteLine("----->>> Simulating exception in AuctionCreatedConsumer");
            throw new ArgumentException("Simulated exception in AuctionCreatedConsumer");
        }

        await item.SaveAsync();
    }
}

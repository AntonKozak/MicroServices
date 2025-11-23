using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionCreatedFaultConsumer : IConsumer<Fault<AuctionCreated>>
{
    public async Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
    {
        var exception = context.Message.Exceptions.First();

        if (exception.ExceptionType == "System.ArgumentException")
        {
            Console.WriteLine("----->>> Handling ArgumentException specifically FooExceptionInAuctionCreatedFaultConsumer.");
            context.Message.Message.Model = "FooExceptionInAuctionCreatedFaultConsumer";
            await context.Publish(context.Message.Message);
        }
        else
        {
            Console.WriteLine("----->>> Handling general exception in AuctionCreatedFaultConsumer.");
        }

    }
}

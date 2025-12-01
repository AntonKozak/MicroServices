using Contracts.Auctions;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    public AuctionFinishedConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task Consume(ConsumeContext<AuctionFinished> context)
    {

        Console.WriteLine("AuctionFinished event received in NotificationService");

        return _hubContext.Clients.All.SendAsync("AuctionFinished", context.Message);
    }
}

using Contracts.Auctions;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<AuctionCreatedConsumer> _logger;

    public AuctionCreatedConsumer(IHubContext<NotificationHub> hubContext, ILogger<AuctionCreatedConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        _logger.LogInformation("📧 Auction Created Notification!");
        _logger.LogInformation("   ID: {Id}", context.Message.Id);
        _logger.LogInformation("   Seller: {Seller}", context.Message.Seller);

        await _hubContext.Clients.All.SendAsync("AuctionCreated", context.Message);
    }
}

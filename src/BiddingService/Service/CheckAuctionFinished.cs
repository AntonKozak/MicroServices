
using BiddingService.Models;
using Contracts.Auctions;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService.Service;

public class CheckAuctionFinished : BackgroundService
{
    private readonly ILogger<CheckAuctionFinished> _logger;
    private readonly IServiceProvider _serviceProvider;
    public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Start checking for finished auctions.");

        stoppingToken.Register(() =>
        {
            _logger.LogInformation("Stopping checking for finished auctions.");
        });

        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckAuctions(stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Check every minute
        }
    }

    private async Task CheckAuctions(CancellationToken stoppingToken)
    {
        var finishedAuctions = await DB.Find<Auction>()
            .Match(a => a.AuctionEnd <= DateTime.UtcNow)
            .Match(x => !x.Finished)
            .ExecuteAsync(stoppingToken);

        if (finishedAuctions.Count == 0)
        {
            _logger.LogInformation("No finished auctions found at {Time}", DateTime.UtcNow);
            return;
        }

        _logger.LogInformation("Found {Count} finished auctions at {Time}", finishedAuctions.Count, DateTime.UtcNow);

        using var scope = _serviceProvider.CreateScope();

        var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        foreach (var auction in finishedAuctions)
        {
            auction.Finished = true;
            await auction.SaveAsync(null, stoppingToken);

            var winningBid = await DB.Find<Bid>()
                .Match(b => b.AuctionId == auction.ID)
                .Match(b => b.BidStatus == BidStatus.Accepted)
                .Sort(x => x.Descending(b => b.Amount))
                .ExecuteFirstAsync(stoppingToken);

            await endpoint.Publish(new AuctionFinished
            {
                ItemSold = winningBid != null,
                AuctionId = auction.ID!,
                Winner = winningBid?.Bidder!,
                Amount = winningBid?.Amount ?? 0,
                Seller = auction.Seller
            }, stoppingToken);
        }
    }
}

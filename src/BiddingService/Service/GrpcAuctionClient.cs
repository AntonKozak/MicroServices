using AuctionService.Protos;
using BiddingService.Models;
using Grpc.Net.Client;

namespace BiddingService.Service;

public class GrpcAuctionClient
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GrpcAuctionClient> _logger;
    public GrpcAuctionClient(ILogger<GrpcAuctionClient> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public Auction GetAuction(string id)
    {
        _logger.LogInformation("Creating gRPC channel to AuctionService");
        var channel = GrpcChannel.ForAddress(_configuration["GrpcAuction:AuctionServiceUrl"]!);
        var client = new GrpcAuction.GrpcAuctionClient(channel);
        var request = new GetAuctionRequest { Id = id };

        try
        {
            var reply = client.GetAuction(request);
            var auction = new Auction
            {
                ID = reply.Id,
                AuctionEnd = DateTime.Parse(reply.AuctionEnd),
                Seller = reply.Seller,
                ReservePrice = reply.ReservePrice,
            };

            return auction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling gRPC service for Auction ID: {AuctionId}", id);
            return null!;
        }
    }
}

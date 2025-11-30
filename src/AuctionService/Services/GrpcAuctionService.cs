using AuctionService.Data;
using AuctionService.Protos;
using Grpc.Core;

namespace AuctionService.Services;

public class GrpcAuctionService : GrpcAuction.GrpcAuctionBase
{
    private readonly AuctionDbContext _context;
    public GrpcAuctionService(AuctionDbContext context)
    {
        _context = context;
    }

    public override async Task<GrpcAuctionModel> GetAuction(GetAuctionRequest request,
    ServerCallContext context)
    {
        Console.WriteLine("Received gRPC request for Auction ID: " + request.Id);

        var auction = await _context.Auctions.FindAsync(Guid.Parse(request.Id))
        ?? throw new RpcException(new Status(StatusCode.NotFound, "Auction not found"));

        var response = new GrpcAuctionModel
        {
            AuctionEnd = auction.AuctionEnd.ToString(),
            Id = auction.Id.ToString(),
            ReservePrice = auction.ReservePrice,
            Seller = auction.Seller,
        };

        return response;

    }
}

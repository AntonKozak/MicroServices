using AuctionService.DTOs;
using AuctionService.Entities;

namespace AuctionService.Interfaces;

public interface IAuctionRepository
{
    Task<List<AuctionDto>> GetAuctionsAsync(string date);
    Task<AuctionDto> GetAuctionById(Guid id);
    Task<Auction> GetAuctionEntityById(Guid id);
    void AddAuction(Auction auction);
    void RemoveAuction(Auction auction);
}

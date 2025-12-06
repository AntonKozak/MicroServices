using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Repository;

public class AuctionRepository : IAuctionRepository
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;
    public AuctionRepository(AuctionDbContext context, IMapper mapper)
    {
        _mapper = mapper;
        _context = context;
    }

    public void AddAuction(Auction auction)
    {
        _context
            .Auctions
            .Add(auction);
    }

    public async Task<AuctionDto> GetAuctionById(Guid id)
    {
        try
        {
            var auction = await _context.Auctions
            .ProjectTo<AuctionDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(a => a.Id == id);

            return auction!;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw new Exception("Error retrieving auction by ID", ex);
        }

    }

    public async Task<Auction> GetAuctionEntityById(Guid id)
    {
        var auction = await _context.Auctions
            .Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == id);

        return auction!;
    }

    public async Task<List<AuctionDto>> GetAuctionsAsync(string date)
    {
        var query = _context.Auctions
            .OrderBy(a => a.Item.Make)
            .AsQueryable();

        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(a => a.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }

        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    public void RemoveAuction(Auction auction)
    {
        _context.Auctions.Remove(auction);
    }
}

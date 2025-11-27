using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    public AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
        _mapper = mapper;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string? date)
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

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
        .Include(a => a.Item)
        .FirstOrDefaultAsync(a => a.Id == id);

        if (auction == null)
            return NotFound();

        var auctionDto = _mapper.Map<AuctionDto>(auction);
        return Ok(auctionDto);
    }


    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
    {
        var auction = _mapper.Map<Auction>(createAuctionDto);

        // Get seller from JWT claims
        auction.Seller = User.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated");

        _context.Auctions.Add(auction);


        // Publish event to event bus
        var newAuction = _mapper.Map<AuctionDto>(auction);
        await _publishEndpoint.Publish(_mapper.Map<Contracts.AuctionCreated>(newAuction));


        var result = await _context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Could not create auction");

        return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, newAuction);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _context.Auctions
            .Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (auction == null)
            return NotFound();

        // Verify user is the seller
        if (auction.Seller != User.Identity?.Name)
            return Forbid();

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage != 0 ? updateAuctionDto.Mileage : auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year != 0 ? updateAuctionDto.Year : auction.Item.Year;
        auction.UpdatedAt = DateTime.UtcNow;

        await _publishEndpoint.Publish(_mapper.Map<Contracts.AuctionUpdated>(auction));

        var result = await _context.SaveChangesAsync() > 0;

        if (!result)
            return BadRequest("Could not update auction");

        return NoContent();
    }


    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id);

        if (auction == null)
            return NotFound();

        // Verify user is the seller
        if (auction.Seller != User.Identity?.Name)
            return Forbid();

        _context.Auctions.Remove(auction);

        await _publishEndpoint.Publish<Contracts.AuctionDeleted>(new { Id = auction.Id.ToString() });

        var result = await _context.SaveChangesAsync() > 0;

        if (!result)
            return BadRequest("Could not delete auction");

        return Ok();
    }

}

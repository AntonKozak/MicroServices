using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;
    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {
        _mapper = mapper;
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<AuctionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<AuctionDto>>> GetAuctions()
    {
        var auctions = await _context.Auctions
            .Include(a => a.Item)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        var auctionsDto = _mapper.Map<List<AuctionDto>>(auctions);
        return Ok(auctionsDto);
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


    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
    {
        var auction = _mapper.Map<Auction>(createAuctionDto);

        auction.Seller = "Test user";


        _context.Auctions.Add(auction);
        var result = await _context.SaveChangesAsync() > 0;

        if (!result)
            return BadRequest("Could not create auction");

        var auctionDto = _mapper.Map<AuctionDto>(auction);

        return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, auctionDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _context.Auctions
            .Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (auction == null)
            return NotFound();

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage != 0 ? updateAuctionDto.Mileage : auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year != 0 ? updateAuctionDto.Year : auction.Item.Year;
        auction.UpdatedAt = DateTime.UtcNow;

        var result = await _context.SaveChangesAsync() > 0;

        if (!result)
            return BadRequest("Could not update auction");

        return NoContent();
    }


    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id);

        if (auction == null)
            return NotFound();

        _context.Auctions.Remove(auction);
        var result = await _context.SaveChangesAsync() > 0;

        if (!result)
            return BadRequest("Could not delete auction");

        return Ok();
    }

}

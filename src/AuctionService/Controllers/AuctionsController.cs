using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.Interfaces;
using AutoMapper;
using Contracts.Auctions;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuctionsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IUnitOfWork _unitOfWork;
    public AuctionsController(IUnitOfWork unitOfWork, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string? date)
    {
        var auctions = await _unitOfWork.AuctionRepository.GetAuctionsAsync(date ?? string.Empty);

        return Ok(auctions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _unitOfWork.AuctionRepository.GetAuctionById(id);

        if (auction == null)
            return NotFound();

        return Ok(auction);
    }


    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
    {
        var auction = _mapper.Map<Auction>(createAuctionDto);

        // Get seller from JWT claims
        auction.Seller = User.Identity?.Name ?? throw new UnauthorizedAccessException("User not authenticated");

        _unitOfWork.AuctionRepository.AddAuction(auction);


        // Publish event to event bus
        var newAuction = _mapper.Map<AuctionDto>(auction);
        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));


        var result = await _unitOfWork.CompleteAsync();

        if (!result) return BadRequest("Could not create auction");

        return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, newAuction);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _unitOfWork.AuctionRepository.GetAuctionEntityById(id);

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

        await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

        var result = await _unitOfWork.CompleteAsync();

        if (!result)
            return BadRequest("Could not update auction");

        return NoContent();
    }


    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _unitOfWork.AuctionRepository.GetAuctionEntityById(id);

        if (auction == null)
            return NotFound();

        // Verify user is the seller
        if (auction.Seller != User.Identity?.Name)
            return Forbid();

        _unitOfWork.AuctionRepository.RemoveAuction(auction);

        await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

        var result = await _unitOfWork.CompleteAsync();

        if (!result)
            return BadRequest("Could not delete auction");

        return Ok();
    }

}

using AutoMapper;
using BiddingService.DTOs;
using BiddingService.Models;
using Contracts.Auctions;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;

namespace BiddingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BidsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    public BidsController(IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
        _mapper = mapper;
    }

    [Authorize]
    [HttpPost("{auctionId}")]
    public async Task<ActionResult<BidDto>> PlaceBid(string auctionId, int amount)
    {
        var auction = await DB.Find<Auction>().OneAsync(auctionId);
        if (auction == null)
        {
            //check with auction service if auction exists
            return NotFound("Auction not found.");
        }

        if (auction.Seller == User.Identity?.Name)
        {
            return BadRequest("Sellers cannot bid on their own auctions.");
        }

        // Check if auction has ended
        if (auction.AuctionEnd < DateTime.UtcNow)
        {
            return BadRequest("Auction has ended.");
        }

        // Get highest bid
        var highestBid = await DB.Find<Bid>()
            .Match(b => b.AuctionId == auctionId)
            .Sort(b => b.Descending(x => x.Amount))
            .ExecuteFirstAsync();

        // Bid must be higher than current highest bid
        if (highestBid != null && amount <= highestBid.Amount)
        {
            return BadRequest($"Bid must be higher than current highest bid of {highestBid.Amount}");
        }

        // Create bid
        var bid = new Bid
        {
            AuctionId = auctionId,
            Bidder = User.Identity?.Name ?? "Anonymous",
            Amount = amount,
        };

        // Determine bid status
        if (amount >= auction.ReservePrice)
        {
            bid.BidStatus = BidStatus.Accepted;
        }
        else
        {
            bid.BidStatus = BidStatus.AcceptedBelowReserve;
        }

        await bid.SaveAsync();
        _publishEndpoint.Publish(_mapper.Map<BidPlaced>(bid));

        return Ok(_mapper.Map<BidDto>(bid));
    }

    [HttpGet("{auctionId}")]

    public async Task<ActionResult<List<BidDto>>> GetBidsForAuction(string auctionId)
    {
        var bids = await DB.Find<Bid>()
            .Match(b => b.AuctionId == auctionId)
            .Sort(b => b.Descending(x => x.BidTime))
            .ExecuteAsync();
        return bids.Select(_mapper.Map<BidDto>).ToList();
    }
}

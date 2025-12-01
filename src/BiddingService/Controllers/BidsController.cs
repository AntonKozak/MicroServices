using AutoMapper;
using BiddingService.DTOs;
using BiddingService.Models;
using BiddingService.Service;
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
    private readonly GrpcAuctionClient _grpcAuctionClient;
    public BidsController(IMapper mapper, IPublishEndpoint publishEndpoint, GrpcAuctionClient grpcAuctionClient)
    {
        _grpcAuctionClient = grpcAuctionClient;
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
            auction = _grpcAuctionClient.GetAuction(auctionId);

            if (auction == null)
            {
                return NotFound("Auction not found.");
            }
            await auction.SaveAsync();
        }

        if (auction.Seller == User.Identity?.Name)
        {
            return BadRequest("Sellers cannot bid on their own auctions.");
        }

        if (auction.AuctionEnd < DateTime.UtcNow)
        {
            return BadRequest("Auction has ended.");
        }

        // Отримуємо список попередніх учасників
        var previousBidders = await DB.Find<Bid>()
            .Match(b => b.AuctionId == auctionId)
            .ExecuteAsync();

        var uniquePreviousBidders = previousBidders
            .Select(b => b.Bidder)
            .Distinct()
            .Where(b => b != User.Identity?.Name)
            .ToList();

        var highestBid = await DB.Find<Bid>()
            .Match(b => b.AuctionId == auctionId)
            .Sort(b => b.Descending(x => x.Amount))
            .ExecuteFirstAsync();

        if (highestBid != null && amount <= highestBid.Amount)
        {
            return BadRequest($"Bid must be higher than current highest bid of {highestBid.Amount}");
        }

        var bid = new Bid
        {
            AuctionId = auctionId,
            Bidder = User.Identity?.Name ?? "Anonymous",
            Amount = amount,
        };

        if (amount >= auction.ReservePrice)
        {
            bid.BidStatus = BidStatus.Accepted;
        }
        else
        {
            bid.BidStatus = BidStatus.AcceptedBelowReserve;
        }

        await bid.SaveAsync();

        var bidPlacedEvent = _mapper.Map<BidPlaced>(bid);
        bidPlacedEvent.PreviousBidders = uniquePreviousBidders;

        await _publishEndpoint.Publish(bidPlacedEvent);

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

    [HttpGet("{auctionId}/bidders")]
    public async Task<ActionResult<List<string>>> GetBiddersForAuction(string auctionId)
    {
        var bids = await DB.Find<Bid>()
            .Match(b => b.AuctionId == auctionId)
            .ExecuteAsync();

        var uniqueBidders = bids
            .Select(b => b.Bidder)
            .Distinct()
            .ToList();

        return uniqueBidders;
    }
}

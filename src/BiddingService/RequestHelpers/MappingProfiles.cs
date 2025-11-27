using AutoMapper;
using BiddingService.DTOs;
using BiddingService.Models;
using Contracts.Auctions;

namespace BiddingService.RequestHelpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Bid, BidDto>();
        CreateMap<Bid, BidPlaced>();
    }

}

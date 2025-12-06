using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Controllers;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.Interfaces;
using AuctionService.RequestHelpers;
using AuctionService.UnitTests.Helpers;
using AutoFixture;
using AutoMapper;
using Contracts.Auctions;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuctionService.UnitTests;

public class AuctionControllerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Fixture _fixture;
    private readonly AuctionsController _auctionsController;
    private readonly IMapper _mapper;

    public AuctionControllerTests()
    {
        _fixture = new Fixture();
        _fixture.Customize<CreateAuctionDto>(c => c.With(x => x.Status, "Live"));
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();

        var mockMapper = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(MappingProfiles).Assembly);
        }).CreateMapper().ConfigurationProvider;

        _mapper = new Mapper(mockMapper);

        _auctionsController = new AuctionsController(_unitOfWorkMock.Object, _mapper, _publishEndpointMock.Object)
        {

            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = TestHelper.GetClaimsPrincipal() }
            }
        };

    }


    [Fact]
    public async Task GetAllAuctions_WithNoParams_Returns10Auctions()
    {
        // Arrange
        var auctions = _fixture.CreateMany<AuctionDto>(10).ToList(); // Create a list of 10 AuctionDto objects
        _unitOfWorkMock.Setup(u => u.AuctionRepository.GetAuctionsAsync(string.Empty))
            .ReturnsAsync(auctions); // Mock the repository method

        // Act
        var result = await _auctionsController.GetAllAuctions(null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<List<AuctionDto>>(okResult.Value);
        Assert.Equal(10, returnValue.Count);
    }

    [Fact]
    public async Task GetAuctionById_WithValidGuid_ReturnsAuction()
    {
        // Arrange
        var auction = _fixture.Create<AuctionDto>();

        _unitOfWorkMock.Setup(u => u.AuctionRepository.GetAuctionById(auction.Id))
            .ReturnsAsync(auction);

        // Act
        var result = await _auctionsController.GetAuctionById(auction.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<AuctionDto>(okResult.Value);
        Assert.Equal(auction.Make, returnValue.Make);
    }

    [Fact]
    public async Task GetAuctionById_WithInvalidGuid_ReturnsNotFound()
    {
        // Arrange
        var invalidGuid = Guid.NewGuid();

        _unitOfWorkMock.Setup(u => u.AuctionRepository.GetAuctionById(invalidGuid))
            .ReturnsAsync((AuctionDto?)null);

        // Act
        var result = await _auctionsController.GetAuctionById(invalidGuid);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateAuction_WithValidData_ReturnsCreatedAuction()
    {
        // Arrange
        var createAuctionDto = _fixture.Create<CreateAuctionDto>();

        _unitOfWorkMock.Setup(u => u.AuctionRepository.AddAuction(It.IsAny<Auction>()));
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(true);
        _publishEndpointMock.Setup(p => p.Publish(It.IsAny<AuctionCreated>(), default)).Returns(Task.CompletedTask);

        // Act
        var result = await _auctionsController.CreateAuction(createAuctionDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnValue = Assert.IsType<AuctionDto>(createdAtActionResult.Value);
        Assert.Equal(createAuctionDto.Make, returnValue.Make);
        Assert.Equal("testuser", returnValue.Seller);
        Assert.Equal("GetAuctionById", createdAtActionResult.ActionName);
    }

    [Fact]
    public async Task UpdateAuction_WithNotValidSellerName_ReturnsForbid()
    {
        // Arrange
        var auctionId = Guid.NewGuid();
        var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();
        var auctionEntity = _fixture.Build<Auction>()
            .With(a => a.Seller, "differentuser")
            .Create();

        _unitOfWorkMock.Setup(u => u.AuctionRepository.GetAuctionEntityById(auctionId))
            .ReturnsAsync(auctionEntity);

        // Act
        var result = await _auctionsController.UpdateAuction(auctionId, updateAuctionDto);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithNotValidGuidId_ReturnsNotFound()
    {
        // Arrange
        var invalidGuid = Guid.NewGuid();
        var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();

        _unitOfWorkMock.Setup(u => u.AuctionRepository.GetAuctionEntityById(invalidGuid))
            .ReturnsAsync((Auction?)null);

        // Act
        var result = await _auctionsController.UpdateAuction(invalidGuid, updateAuctionDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}

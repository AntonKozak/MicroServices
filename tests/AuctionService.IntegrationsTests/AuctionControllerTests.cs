using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationsTests.Fixtures;
using AuctionService.IntegrationsTests.Util;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationsTests;

[Collection("Shared collection")]
public class AuctionControllerTests : IAsyncLifetime
{
    private readonly CustomWebAppFactory _customWebAppFactory;
    private readonly HttpClient _httpClient;
    private const string GT_ID = "afbee524-5972-4075-8800-7d1f9d7b0a0c";

    public AuctionControllerTests(CustomWebAppFactory customWebAppFactory)
    {
        _customWebAppFactory = customWebAppFactory;
        _httpClient = _customWebAppFactory.CreateClient();
    }

    [Fact]
    public async Task GetAuctions_ShouldReturn3Auctions()
    {
        // Arrange
        var request = "/api/auctions";

        // Act
        var response = await _httpClient.GetFromJsonAsync<List<AuctionDto>>(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(3, response.Count);
    }

    [Fact]
    public async Task GetAuctionById_WithValidId_ShouldReturnAuction()
    {
        // Arrange
        var request = $"/api/auctions/{GT_ID}";

        // Act
        var response = await _httpClient.GetFromJsonAsync<AuctionDto>(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(GT_ID, response.Id.ToString());
    }

    [Fact]
    public async Task GetAuctionById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var request = $"/api/auctions/{invalidId}";

        // Act
        var response = await _httpClient.GetAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateAuction_WithValidData_ShouldReturn201()
    {
        // Arrange
        var request = "/api/auctions";
        var createAuctionDto = GetAuctionForCreate();
        _httpClient.SetFakeBearerToken(AuthHelper.GetBearerForUser("bob"));

        // Act
        var response = await _httpClient.PostAsJsonAsync(request, createAuctionDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var createdAuction = await response.Content.ReadFromJsonAsync<AuctionDto>();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(createdAuction);
        Assert.Equal("TestMake", createdAuction.Make);
        Assert.Equal("TestModel", createdAuction.Model);
        Assert.Equal("bob", createdAuction.Seller);
    }
    [Fact]
    public async Task CreateAuction_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = "/api/auctions";
        var createAuctionDto = new
        {
            Item = new
            {
                Make = "TestMake",
                Model = "TestModel",
                Year = 2020,
                Description = "Test Description"
            },
            StartingPrice = 1000.00m,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7)
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync(request, createAuctionDto);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync()
    {
        using var scope = _customWebAppFactory.Services.CreateScope();
        var bd = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        DbHelper.ReinitDbForTest(bd);
        return Task.CompletedTask;
    }

    private CreateAuctionDto GetAuctionForCreate()
    {
        return new CreateAuctionDto
        {

            Make = "TestMake",
            Model = "TestModel",
            Color = "Red",
            ImageUrl = "http://example.com/image.jpg",
            Mileage = 10000,
            Year = 2020,
            ReservePrice = 500,

        };
    }

}

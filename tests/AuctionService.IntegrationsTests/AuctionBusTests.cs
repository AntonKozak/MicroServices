using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationsTests.Fixtures;
using AuctionService.IntegrationsTests.Util;
using Contracts.Auctions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationsTests;

[Collection("Shared collection")]
public class AuctionBusTests : IAsyncLifetime
{
    private readonly CustomWebAppFactory _customWebAppFactory;
    private readonly HttpClient _httpClient;
    private ITestHarness _testHarness;
    private const string GT_ID = "afbee524-5972-4075-8800-7d1f9d7b0a0c";

    public AuctionBusTests(CustomWebAppFactory customWebAppFactory)
    {
        _customWebAppFactory = customWebAppFactory;
        _httpClient = _customWebAppFactory.CreateClient();
        _testHarness = _customWebAppFactory.Services.GetTestHarness();
    }

    [Fact]
    public async Task CreateAuction_WithValidPbject_ShouldPublishAuctionCreated()
    {
        // Arrange
        var request = "/api/auctions";
        var auctionToCreate = GetAuctionForCreate();
        _httpClient.SetFakeBearerToken(AuthHelper.GetBearerForUser("bob"));
        // Act
        var response = await _httpClient.PostAsJsonAsync(request, auctionToCreate);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(await _testHarness.Published.Any<AuctionCreated>());

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

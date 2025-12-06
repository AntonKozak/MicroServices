using System.Security.Claims;

namespace AuctionService.UnitTests.Helpers;

public static class TestHelper
{
    public static ClaimsPrincipal GetClaimsPrincipal(string username = "testuser")
    {
        var claims = new List<Claim>
        {
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", username),
            new Claim(ClaimTypes.Name, username)
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        return new ClaimsPrincipal(identity);
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using InvoiceKeeper.Domain.Entities;
using InvoiceKeeper.Domain.Enums;
using InvoiceKeeper.Infrastructure.Configuration;
using InvoiceKeeper.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace InvoiceKeeper.Tests.Services;

public class JwtTokenServiceTests
{
    private static readonly JwtOptions Options = new()
    {
        Secret = "unit-test-secret-key-with-enough-length-1234",
        Issuer = "InvoiceKeeperBackend",
        Audience = "InvoiceKeeperClients",
        ExpiryInHours = 2
    };

    [Fact]
    public void CreateToken_ContainsUserIdentityAndRole()
    {
        var user = new User { Username = "tester", Role = UserRole.Admin };
        var token = new JwtTokenService(
            Microsoft.Extensions.Options.Options.Create(Options)).CreateToken(user);

        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal(Options.Issuer, parsed.Issuer);
        Assert.Contains(parsed.Audiences, audience => audience == Options.Audience);
        Assert.Equal(user.Id.ToString(), parsed.Subject);
        Assert.Contains(parsed.Claims, c => c.Type == ClaimTypes.Name && c.Value == "tester");
        Assert.Contains(parsed.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }
}


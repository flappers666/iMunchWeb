using System.Security.Claims;
using iMunchWeb.Data.Entities;

namespace iMunchWeb.Feature.Auth.Services;

public interface ITokenService
{
    public string GenerateJwtToken(ApplicationUser user);
    public RefreshToken GenerateRefreshToken(string userId);
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
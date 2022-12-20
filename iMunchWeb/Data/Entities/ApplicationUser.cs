using Microsoft.AspNetCore.Identity;

namespace iMunchWeb.Data.Entities;

public class ApplicationUser: IdentityUser
{
    [PersonalData]
    public string FirstName { get; set; }

    [PersonalData]
    public string LastName { get; set; }

    public List<RefreshToken> RefreshTokens { get; set; }
}
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Duende.IdentityServer.EntityFramework.Options;
using iMunchWeb.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace iMunchWeb.Data;

public class ApplicationDbContext: IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
        
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
}


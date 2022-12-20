using iMunchWeb.Data;
using iMunchWeb.Data.Entities;
using iMunchWeb.Feature.Auth.Services;
using iMunchWeb.Feature.Project.Const;
using iMunchWeb.Feature.Project.Helpers;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace iMunchWeb.Feature.Auth.Commands;

public class Refresh
{
    public class UserRefreshCommand : IRequest<UserRefreshResult>
    {
        public string Jwt { get; set; }
        public string RefreshToken { get; set; }
    }

    public class UserRefreshResult : Login.UserLoginResult
    {}

    public class Handler : IRequestHandler<UserRefreshCommand, UserRefreshResult>
    {
        private readonly ApplicationDbContext _db;
        private readonly ITokenService _tokenService;
        private ILogger _logger;

        public Handler(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, IOptions<AppSettings> appSettings, ITokenService tokenService,
            ILoggerFactory loggerFactory)
        {
            _db = dbContext;
            _tokenService = tokenService;
            _logger = loggerFactory.CreateLogger(LoggerCategory.Auth);
        }

        public async Task<UserRefreshResult> Handle(UserRefreshCommand command, CancellationToken cancellationToken)
        {
            var result = new UserRefreshResult()
            {
                Success = false
            };

            var principle = _tokenService.GetPrincipalFromExpiredToken(command.Jwt);
            if (principle != null && principle.Identity.IsAuthenticated)
            {
                var id = principle.Identity.Name;
                var user = _db.Users
                    .Include(r => r.RefreshTokens)
                    .FirstOrDefault(x => x.Id == id);

                if (user != null)
                {
                    var currentToken = user.RefreshTokens.Single(x => x.Token == command.RefreshToken);
                    var isValid = currentToken.Expires > DateTime.UtcNow && !currentToken.Revoked;

                    if (isValid)
                    {
                        var newToken = _tokenService.GenerateRefreshToken(user.Id);
                        currentToken.Revoked = true;
                        user.RefreshTokens.Add(newToken);

                        var jsonToken = _tokenService.GenerateJwtToken(user);

                        result.Token = jsonToken;
                        result.RefreshToken = newToken.Token;
                        result.Success = true;

                        await _db.SaveChangesAsync(cancellationToken);
                    }
                }
            }

            return result;
        }
    }
}
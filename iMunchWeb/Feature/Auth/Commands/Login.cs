using iMunchWeb.Data;
using iMunchWeb.Data.Entities;
using iMunchWeb.Feature.Auth.Services;
using iMunchWeb.Feature.Project.Commands;
using iMunchWeb.Feature.Project.Const;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace iMunchWeb.Feature.Auth.Commands;


public class Login
{
    public class UserLoginCommand : IRequest<UserLoginResult>
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }

    public class UserLoginResult : BaseResult
    {
        public String Token { get; set; }

        public String RefreshToken { get; set; }
    }

    public class Handler : IRequestHandler<UserLoginCommand, UserLoginResult>
    {
        private readonly ApplicationDbContext _db;
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private ILogger _logger;

        public Handler(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, ITokenService tokenService,
                ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _db = dbContext;
            _tokenService = tokenService;
            _logger = loggerFactory.CreateLogger(LoggerCategory.Auth);
        }

        public async Task<UserLoginResult> Handle(UserLoginCommand command, CancellationToken cancellationToken)
        {
            var result = new UserLoginResult
            {
                Success = false
            };

            if (string.IsNullOrEmpty(command.UserName) || string.IsNullOrEmpty(command.Password))
                return result;

            var user = _db.Users
                .Include(r=> r.RefreshTokens)
                .FirstOrDefault(x => x.UserName == command.UserName);

            if (user != null)
                result.Success = await _userManager.CheckPasswordAsync(user, command.Password);


            if (result.Success && user != null)
            {
                //Add jwt
                result.Token = _tokenService.GenerateJwtToken(user);
                
                //Add refresh token
                var refreshToken = _tokenService.GenerateRefreshToken(user.Id);
                user.RefreshTokens.Add(refreshToken);
                
                await _db.SaveChangesAsync(cancellationToken);
                
                result.RefreshToken = refreshToken.Token;
            }
            
            return result;
        }
    }
}

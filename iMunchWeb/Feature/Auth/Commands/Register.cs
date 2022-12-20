using iMunchWeb.Data;
using iMunchWeb.Data.Entities;
using iMunchWeb.Feature.Project.Commands;
using iMunchWeb.Feature.Project.Const;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace iMunchWeb.Feature.Auth.Commands;

//TODO: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/?view=aspnetcore-5.0&tabs=visual-studio
    public class Register
    {
        public class UserRegisterCommand : IRequest<UserRegisterResult>
        {
            public string Email { get; set; }
            
            public string FirstName { get; set; }

            public string LastName { get; set; }
            
            public string Password { get; set; }
            
        }

        public class UserRegisterResult : BaseResult
        {
            
        }
        
        public class Handler : IRequestHandler<UserRegisterCommand, UserRegisterResult>
        {
            private readonly ApplicationDbContext _db;
            private readonly ILogger _logger;
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly RoleManager<IdentityRole> _roleManager;

            public Handler(ApplicationDbContext dbContext,
                UserManager<ApplicationUser> userManager,
                RoleManager<IdentityRole> roleManager,
                ILoggerFactory loggerFactory)
            {
                _userManager = userManager;
                _roleManager = roleManager;
                _db = dbContext;
                _logger = loggerFactory.CreateLogger(LoggerCategory.Auth);
            }

            public async Task<UserRegisterResult> Handle(UserRegisterCommand command, CancellationToken cancellationToken)
            {
                var result = new UserRegisterResult
                {
                    Success = false,
                    Errors = new List<String>()
                };

                try
                {
                    var userIdentity = new ApplicationUser()
                    {
                        UserName = command.Email,
                        Email = command.Email,
                        FirstName = command.FirstName,
                        LastName = command.LastName
                    };
                    
                    var user = await _userManager.CreateAsync(userIdentity, command.Password);
                    IdentityResult? role = null;
                    if (user.Succeeded)
                    {
                        role = await _userManager.AddToRoleAsync(userIdentity, Roles.Customer);
                        result.Success = role.Succeeded;
                    }

                    if (!result.Success)
                    {
                        if (user.Errors.Any())
                        {
                            result.Errors.AddRange(user.Errors.Select(e => e.Description).ToList());
                        }

                        if (role !=null && role.Errors.Any()){
                            result.Errors.AddRange(role.Errors.Select(e => e.Description).ToList());
                        }
                        
                        _logger.LogWarning($"User not registered");
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error registering user");
                    result.Errors.Add(ex.Message);
                    return result;
                }
            }
            
            
        }
    }
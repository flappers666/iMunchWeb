using iMunchWeb.Feature.Auth.Commands;
using iMunchWeb.Feature.Project.Const;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iMunchWeb.Feature.Auth.Controllers;

public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private ILogger _logger;

    public AuthController(ILoggerFactory loggerFactory, IMediator mediator)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger(LoggerCategory.Api);
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] Register.UserRegisterCommand command)
    {
        var result = await _mediator.Send(command);
            
        if (result.Success)
            return Ok(result);

        return BadRequest(result);
            
    }
        
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] Login.UserLoginCommand command)
    {
        var result = await _mediator.Send(command);
            
        if (result.Success)
            return Ok(result);

        return Unauthorized();
            
    }
        
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Refresh([FromBody] Refresh.UserRefreshCommand command)
    {
        var result = await _mediator.Send(command);
            
        if (result.Success)
            return Ok(result);

        return BadRequest();
            
    }
}
namespace FiberEvolutionScraper.Api.Controllers;

using FiberEvolutionScraper.Api.Domain.User.Create;
using FiberEvolutionScraper.Api.Models.User;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class AuthController
{
    private readonly ISender mediator;
    private readonly ILogger<AuthController> logger;

    public AuthController(ISender sender, ILogger<AuthController> logger)
    {
        mediator = sender;
        this.logger = logger;
    }

    [HttpPost("createOrUpdateUser")]
    public async Task<IActionResult> CreateAccount(User user)
    {
        logger.LogInformation("Create new User");
        var command = new CreateUserCommand(user);
        var result = await mediator.Send(command);
        if (result.IsError)
        {
            return new BadRequestObjectResult(result);
        }
        return new ObjectResult(result);
    }
}

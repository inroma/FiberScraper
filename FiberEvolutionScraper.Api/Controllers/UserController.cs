namespace FiberEvolutionScraper.Api.Controllers;

using FiberEvolutionScraper.Api.Domain.User.Create;
using FiberEvolutionScraper.Api.Models.User;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly ISender mediator;
    private readonly ILogger<UserController> logger;

    public UserController(ISender sender, ILogger<UserController> logger)
    {
        mediator = sender;
        this.logger = logger;
    }

    [HttpPost("syncUser")]
    public async Task<ActionResult<UserModel>> CreateAccount()
    {
        logger.LogInformation("Create or Update User");
        var command = new CreateOrUpdateUserCommand(User);
        var result = await mediator.Send(command);
        if (result.IsError)
        {
            return new BadRequestObjectResult(result.Errors);
        }
        return result.Value;
    }
}

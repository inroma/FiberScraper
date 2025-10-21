namespace FiberEvolutionScraper.Api.Domain.User.Create;

using ErrorOr;
using FiberEvolutionScraper.Api.Models.User;
using MediatR;
using System.Security.Claims;

public class CreateOrUpdateUserCommand(ClaimsPrincipal claims) : IRequest<ErrorOr<UserModel>>
{
    public ClaimsPrincipal Claims { get; set; } = claims;
}
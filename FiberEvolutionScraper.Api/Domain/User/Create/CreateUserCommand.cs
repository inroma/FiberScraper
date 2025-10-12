namespace FiberEvolutionScraper.Api.Domain.User.Create;

using ErrorOr;
using FiberEvolutionScraper.Api.Models.User;
using MediatR;

public class CreateUserCommand(User user) : IRequest<ErrorOr<User>>
{
    public User User { get; set; } = user;
}
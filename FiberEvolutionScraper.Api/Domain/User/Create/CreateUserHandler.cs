namespace FiberEvolutionScraper.Api.Domain.User.Create;

using ErrorOr;
using FiberEvolutionScraper.Api.Models.User;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, ErrorOr<User>>
{

    public Task<ErrorOr<User>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

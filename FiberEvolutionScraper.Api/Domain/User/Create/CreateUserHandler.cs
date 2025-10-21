namespace FiberEvolutionScraper.Api.Domain.User.Create;

using ErrorOr;
using FiberEvolutionScraper.Api.Data;
using FiberEvolutionScraper.Api.Models.User;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class CreateOrUpdateUserHandler : IRequestHandler<CreateOrUpdateUserCommand, ErrorOr<UserModel>>
{
    #region Private Properties

    private readonly UnitOfWork unitOfWork;

    #endregion Private Properties

    #region Public Constructor

    public CreateOrUpdateUserHandler(UnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }

    #endregion Public Constructor

    public Task<ErrorOr<UserModel>> Handle(CreateOrUpdateUserCommand request, CancellationToken cancellationToken)
    {
        var uid = request.Claims.FindFirst(c => c.Type == "uid")?.Value;
        UserModel user = unitOfWork.UserRepository.Get(a => a.UId == uid).FirstOrDefault();

        if (user == null)
        {
            user = new UserModel()
            {
                UId = uid,
                UserName = request.Claims.FindFirst("name")?.Value
            };
            unitOfWork.UserRepository.Update(user);
        }
        else
        {
            user.UserName = request.Claims.FindFirst("name")?.Value;
            unitOfWork.UserRepository.Insert(user);
        }

        unitOfWork.Save();

        return Task.FromResult(user.ToErrorOr());
    }
}

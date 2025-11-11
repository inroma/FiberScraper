namespace FiberEvolutionScraper.Api.Domain.User.Create;

using ErrorOr;
using FiberEvolutionScraper.Api.Data;
using FiberEvolutionScraper.Api.Data.Interfaces;
using FiberEvolutionScraper.Api.Models.User;
using MediatR;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

public class CreateOrUpdateUserHandler(IUnitOfWork<ApplicationDbContext> unitOfWork)
    : IRequestHandler<CreateOrUpdateUserCommand, ErrorOr<UserModel>>
{
    #region Private Properties

    private readonly IUnitOfWork<ApplicationDbContext> unitOfWork = unitOfWork;

    #endregion Private Properties

    #region Public Constructor

    #endregion Public Constructor

    public Task<ErrorOr<UserModel>> Handle(CreateOrUpdateUserCommand request, CancellationToken cancellationToken)
    {
        var uid = request.Claims.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        UserModel user = unitOfWork.GetRepository<UserModel>().Get(a => a.UId == uid).FirstOrDefault();

        if (user == null)
        {
            user = new()
            {
                UId = uid,
                UserName = request.Claims.FindFirst("name")?.Value
            };
            unitOfWork.GetRepository<UserModel>().Insert(user);
        }
        else
        {
            user.UserName = request.Claims.FindFirst("name")?.Value;
            unitOfWork.GetRepository<UserModel>().Update(user);
        }

        unitOfWork.Save();

        return Task.FromResult(user.ToErrorOr());
    }
}

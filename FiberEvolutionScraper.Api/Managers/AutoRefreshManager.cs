using ErrorOr;
using FiberEvolutionScraper.Api.Data;
using FiberEvolutionScraper.Api.Data.Interfaces;
using FiberEvolutionScraper.Api.Models;
using FiberEvolutionScraper.Api.Models.User;

namespace FiberEvolutionScraper.Api.Managers;

public class AutoRefreshManager
{
    private readonly IUnitOfWork<ApplicationDbContext> unitOfWork;
    private readonly ILogger<AutoRefreshManager> logger;
    private readonly FiberManager fiberManager;

    public AutoRefreshManager(IUnitOfWork<ApplicationDbContext> unitOfWork, ILogger<AutoRefreshManager> logger, FiberManager fiberManager)
    {
        this.unitOfWork = unitOfWork;
        this.logger = logger;
        this.fiberManager = fiberManager;
    }

    public IEnumerable<AutoRefreshInput> GetAll(UserModel user)
    {
        var list = unitOfWork.GetRepository<AutoRefreshInput>().Get(a => a.UserId == user.Id).ToList();
        return list;
    }

    public int Add(AutoRefreshInput input, UserModel user)
    {
        input.UserId = user.Id;
        unitOfWork.GetRepository<AutoRefreshInput>().Insert(input);
        return unitOfWork.Save();
    }

    public ErrorOr<int> Delete(int inputId, UserModel user)
    {
        var item = unitOfWork.GetRepository<AutoRefreshInput>().Get(x => x.Id == inputId).FirstOrDefault();
        if (item != null)
        {
            if (item.UserId != user.Id)
            {
                return Error.Validation(description: "User is not owner of the entity");
            }
            unitOfWork.GetRepository<AutoRefreshInput>().Delete(item);
            return unitOfWork.Save();
        }
        return Error.NotFound(description: $"Aucun enregistrement en BDD avec cet Id: {inputId}");
    }

    public ErrorOr<int> Update(AutoRefreshInput input, UserModel user)
    {
        try
        {
            if (input.UserId != user.Id)
            {
                return Error.Validation(description: "User is not owner of the entity");
            }
            unitOfWork.GetRepository<AutoRefreshInput>().Update(input);
            return unitOfWork.Save();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur pendant l'update d'une zone");
            return Error.Unexpected();
        }
    }

    public async Task RefreshAll(UserModel user = null)
    {
        logger.LogInformation("Refreshing all areas");
        var areas = unitOfWork.GetRepository<AutoRefreshInput>().Get(a => a.Enabled && a.LastRun < DateTime.UtcNow.AddHours(-1));
        if (user != null)
        {
            areas = areas.Where(a => a.UserId == user.Id);
        }
        foreach (var area in areas)
        {
            try
            {
                await fiberManager.UpdateDbFibers(area.CoordX, area.CoordY, area.AreaSize);
                area.LastRun = DateTime.UtcNow;

                unitOfWork.Save();
                // On pause 20 sec pour pas se faire ban par l'API Orange
                Thread.Sleep(TimeSpan.FromSeconds(20));
            }
            catch (Exception ex)
            {
                logger.LogError("Erreur pendant un refresh automatique: {message}", ex.InnerException.Message ?? ex.Message);
                continue;
            }
        }
        await Task.FromResult(true);
    }
}
using ErrorOr;
using FiberEvolutionScraper.Api.Data;
using FiberEvolutionScraper.Api.Models;
using FiberEvolutionScraper.Api.Models.User;
using Microsoft.EntityFrameworkCore;

namespace FiberEvolutionScraper.Api.Managers;

public class AutoRefreshManager
{
    private readonly ApplicationDbContext context;
    private readonly ILogger<AutoRefreshManager> logger;
    private readonly FiberManager fiberManager;

    public AutoRefreshManager(ApplicationDbContext context, ILogger<AutoRefreshManager> logger, FiberManager fiberManager)
    {
        this.context = context;
        this.logger = logger;
        this.fiberManager = fiberManager;
    }

    public async Task<IEnumerable<AutoRefreshInput>> GetAll(UserModel user)
    {
        var list = await context.AutoRefreshInputs.Where(a => a.UserId == user.Id).ToListAsync();
        return list;
    }

    public async Task<int> Add(AutoRefreshInput input, UserModel user)
    {
        input.UserId = user.Id;
        await context.AutoRefreshInputs.AddAsync(input);
        return await context.SaveChangesAsync();
    }

    public async Task<int> AddRange(IEnumerable<AutoRefreshInput> list, UserModel user)
    {
        foreach (var input in list)
        {
            input.UserId = user.Id;
        }
        await context.AutoRefreshInputs.AddRangeAsync(list);
        return await context.SaveChangesAsync();
    }

    public async Task<ErrorOr<int>> Delete(int inputId, UserModel user)
    {
        var item = await context.AutoRefreshInputs.FirstOrDefaultAsync(x => x.Id == inputId);
        if (item != null)
        {
            if (item.UserId != user.Id)
            {
                return Error.Validation(description: "User is not owner of the entity");
            }
            context.AutoRefreshInputs.Remove(item);
            return await context.SaveChangesAsync();
        }
        return Error.NotFound(description: $"Aucun enregistrement en BDD avec cet Id: {inputId}");
    }

    public async Task<ErrorOr<int>> Update(AutoRefreshInput input, UserModel user)
    {
        try
        {
            if (input.UserId != user.Id)
            {
                return Error.Validation(description: "User is not owner of the entity");
            }
            context.AutoRefreshInputs.Update(input);
            return await context.SaveChangesAsync();
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
        var areas = context.AutoRefreshInputs.Where(a => a.Enabled);
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

                await context.SaveChangesAsync();
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
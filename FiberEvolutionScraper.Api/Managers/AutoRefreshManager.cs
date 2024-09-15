using FiberEvolutionScraper.Api.Data;
using FiberEvolutionScraper.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FiberEvolutionScraper.Api.Services;

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

    public async Task<IEnumerable<AutoRefreshInput>> GetAll()
    {
        var list = await context.AutoRefreshInputs.ToListAsync();
        return list;
    }

    public async Task<int> Add(AutoRefreshInput input)
    {
        await context.AutoRefreshInputs.AddAsync(input);
        return await context.SaveChangesAsync();
    }

    public async Task<int> AddRange(IEnumerable<AutoRefreshInput> list)
    {
        await context.AutoRefreshInputs.AddRangeAsync(list);
        return await context.SaveChangesAsync();
    }

    public async Task<int> Delete(int inputId)
    {
        var item = await context.AutoRefreshInputs.FirstOrDefaultAsync(x => x.Id == inputId);
        if (item != null)
        {
            context.AutoRefreshInputs.Remove(item);
            return await context.SaveChangesAsync();
        }
        throw new($"Aucun enregistrement en BDD avec cet Id: {inputId}");
    }

    public async Task<int> Delete(AutoRefreshInput input)
    {
        context.AutoRefreshInputs.Remove(input);
        return await context.SaveChangesAsync();
    }

    public async Task<int> Update(AutoRefreshInput input)
    {
        try
        {
            context.AutoRefreshInputs.Update(input);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex.InnerException.Message ?? ex.Message);
            return -1;
        }
    }

    public async Task RefreshAll()
    {
        logger.LogInformation("Refreshing all areas");
        var areas = context.AutoRefreshInputs.Where(a => a.Enabled).ToList();
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
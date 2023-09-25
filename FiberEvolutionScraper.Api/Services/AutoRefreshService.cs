using FiberEvolutionScraper.Api.Data;
using Quartz;

namespace FiberEvolutionScraper.Api.Services;

public class AutoRefreshService : IJob
{
    #region Private Fields

    private readonly ApplicationDbContext Context;
    private readonly FiberManager FiberManager;
    private readonly ILogger<AutoRefreshService> logger;

    #endregion Private Fields

    public AutoRefreshService(ApplicationDbContext context, FiberManager fiberManager, ILogger<AutoRefreshService> logger)
    {
        Context = context;
        FiberManager = fiberManager;
        this.logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var areas = Context.AutoRefreshInputs.Where(a => a.Enabled).ToList();
        foreach (var area in areas)
        {
            try
            {
                await FiberManager.UpdateDbFibers(area.CoordX, area.CoordY, area.AreaSize);
                area.LastRun = DateTime.UtcNow;

                await Context.SaveChangesAsync();
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
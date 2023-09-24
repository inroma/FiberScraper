using FiberEvolutionScraper.Api.Data;
using Quartz;

namespace FiberEvolutionScraper.Api.Services;

public class AutoRefreshService : IJob
{
    #region Private Fields

    private readonly ApplicationDbContext Context;
    private readonly FiberManager FiberManager;

    #endregion Private Fields

    public AutoRefreshService(ApplicationDbContext context, FiberManager fiberManager)
    {
        Context = context;
        FiberManager = fiberManager;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var areas = Context.AutoRefreshInputs.Where(a => a.Enabled);
        foreach (var area in areas)
        {
            try
            {
                var updatedFibersCount = FiberManager.UpdateDbFibers(area.CoordX, area.CoordY, area.AreaSize);
                area.LastRun = DateTime.Now;

                await Context.SaveChangesAsync();
                // On pause 20 sec pour pas se faire ban par l'API Orange
                Thread.Sleep(TimeSpan.FromSeconds(20));
            }
            catch
            {
                continue;
            }
        }
        await Task.FromResult(true);
    }
}
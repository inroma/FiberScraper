using FiberEvolutionScraper.Api.Data;
using Quartz;

namespace FiberEvolutionScraper.Api.Services;

public class AutoRefreshService : IJob
{
    #region Private Fields

    private readonly AutoRefreshManager AutoRefreshManager;
    private readonly ILogger<AutoRefreshService> logger;

    #endregion Private Fields

    public AutoRefreshService(ILogger<AutoRefreshService> logger, AutoRefreshManager autoRefreshManager)
    {
        this.logger = logger;
        AutoRefreshManager = autoRefreshManager;
    }

    public async Task Execute(IJobExecutionContext context) => await AutoRefreshManager.RefreshAll();
}
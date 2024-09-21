using Quartz;

namespace FiberEvolutionScraper.Api.Services;

public class AutoRefreshService : BackgroundService, IJob
{
    #region Private Fields

    private readonly ILogger<AutoRefreshService> Logger;
    private readonly IServiceProvider ServiceProvider;

    #endregion Private Fields

    public AutoRefreshService(IServiceProvider services, ILogger<AutoRefreshService> logger)
    {
        ServiceProvider = services;
        Logger = logger;
    }

    public async Task Execute(IJobExecutionContext context) => await ExecuteAsync();

    public async Task StartRefresh() => await ExecuteAsync();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken = default)
    {
        Logger.LogInformation("Executing Refresh Service");
        using var scope = ServiceProvider.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<AutoRefreshManager>();
        await manager.RefreshAll();
    }
}
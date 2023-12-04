using FiberEvolutionScraper.Api.Models;
using FiberEvolutionScraper.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Quartz;

namespace FiberEvolutionScraper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AutoRefreshController
{
    #region Private Fields

    private readonly AutoRefreshManager autoRefreshManager;
    private readonly ILogger<AutoRefreshController> logger;
    private readonly ISchedulerFactory schedulerFactory;

    #endregion Private Fields

    public AutoRefreshController(AutoRefreshManager autoRefreshManager, ILogger<AutoRefreshController> logger, ISchedulerFactory factory)
    {
        this.autoRefreshManager = autoRefreshManager;
        this.logger = logger;
        schedulerFactory = factory;
    }

    [HttpGet("GetAll")]
    public async Task<IEnumerable<AutoRefreshInput>> GetAll()
    {
        logger.LogDebug("Get All Auto Refresh Inputs in Db");
        var list = await autoRefreshManager.GetAll();

        return list;
    }

    [HttpPost("Add")]
    public async Task<int> AddAreaEntity(AutoRefreshInput refreshInput)
    {
        logger.LogDebug("Add Auto Refresh Input in Db (x: {x}, y: {y})", refreshInput.CoordX, refreshInput.CoordY);
        var result = await autoRefreshManager.Add(refreshInput);

        return result;
    }

    [HttpPatch("Update")]
    public async Task<int> UpdateAreaEntity(AutoRefreshInput refreshInput)
    {
        logger.LogDebug("Update Auto Refresh Input in Db (id: {id})", refreshInput.Id);
        var result = await autoRefreshManager.Update(refreshInput);

        return result;
    }

    [HttpDelete("Delete")]
    public async Task<int> DeleteAreaEntity(int inputId)
    {
        logger.LogDebug("Delete Auto Refresh Input in Db (id: {id})", inputId);
        var result = await autoRefreshManager.Delete(inputId);

        return result;
    }

    [HttpPost("RunAll")]
    public async Task RunAll()
    {
        logger.LogDebug("Run All Auto Refresh");
        IScheduler scheduler = await schedulerFactory.GetScheduler();
        var trigger = await scheduler.GetTrigger(new("AutoRefreshJob-trigger"));
        if (trigger != null)
        {
            await scheduler.TriggerJob(new JobKey("AutoRefreshJob"));
        }
    }
}

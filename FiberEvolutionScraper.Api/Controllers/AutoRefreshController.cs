using FiberEvolutionScraper.Api.Models;
using FiberEvolutionScraper.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FiberEvolutionScraper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AutoRefreshController
{
    #region Private Fields

    private readonly AutoRefreshManager autoRefreshManager;
    private readonly ILogger<AutoRefreshController> logger;

    #endregion Private Fields

    public AutoRefreshController(AutoRefreshManager autoRefreshManager, ILogger<AutoRefreshController> logger)
    {
        this.autoRefreshManager = autoRefreshManager;
        this.logger = logger;
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
}

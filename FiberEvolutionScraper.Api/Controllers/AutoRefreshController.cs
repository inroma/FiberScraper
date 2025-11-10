using FiberEvolutionScraper.Api.Extensions;
using FiberEvolutionScraper.Api.Managers;
using FiberEvolutionScraper.Api.Models;
using FiberEvolutionScraper.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiberEvolutionScraper.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class AutoRefreshController : ControllerBase
{
    #region Private Fields

    private readonly AutoRefreshManager autoRefreshManager;
    private readonly AutoRefreshService autoRefreshService;
    private readonly ILogger<AutoRefreshController> logger;

    #endregion Private Fields

    public AutoRefreshController(AutoRefreshManager autoRefreshManager, ILogger<AutoRefreshController> logger, AutoRefreshService autoRefreshService)
    {
        this.autoRefreshManager = autoRefreshManager;
        this.logger = logger;
        this.autoRefreshService = autoRefreshService;
    }

    [HttpGet("GetAll")]
    public async Task<IEnumerable<AutoRefreshInput>> GetAll()
    {
        logger.LogDebug("Get All Auto Refresh Inputs in Db");
        var list = await autoRefreshManager.GetAll(HttpContext.GetUser());

        return list;
    }

    [HttpPost("Add")]
    public async Task<int> AddAreaEntity(AutoRefreshInput refreshInput)
    {
        logger.LogDebug("Add Auto Refresh Input in Db (x: {x}, y: {y})", refreshInput.CoordX, refreshInput.CoordY);
        var result = await autoRefreshManager.Add(refreshInput, HttpContext.GetUser());

        return result;
    }

    [HttpPatch("Update")]
    public async Task<ActionResult<int>> UpdateAreaEntity(AutoRefreshInput refreshInput)
    {
        logger.LogDebug("Update Auto Refresh Input in Db (id: {id})", refreshInput.Id);
        var result = await autoRefreshManager.Update(refreshInput, HttpContext.GetUser());

        if (result.IsError)
        {
            return new BadRequestObjectResult(result.Errors);
        }
        return result.Value;
    }

    [HttpDelete("Delete")]
    public async Task<ActionResult<int>> DeleteAreaEntity(int inputId)
    {
        logger.LogDebug("Delete Auto Refresh Input in Db (id: {id})", inputId);
        var result = await autoRefreshManager.Delete(inputId, HttpContext.GetUser());

        if (result.IsError)
        {
            return new BadRequestObjectResult(result.Errors);
        }
        return result.Value;
    }

    [HttpPost("RunAll")]
    public ActionResult<bool> RunAll()
    {
        logger.LogDebug("Run All Auto Refresh");
        _ = Task.Run(autoRefreshService.StartRefresh);
        return true;
    }
}

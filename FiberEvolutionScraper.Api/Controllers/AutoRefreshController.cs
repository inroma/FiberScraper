using FiberEvolutionScraper.Api.Extensions;
using FiberEvolutionScraper.Api.Managers;
using FiberEvolutionScraper.Api.Models;
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
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<AutoRefreshController> logger;

    #endregion Private Fields

    public AutoRefreshController(AutoRefreshManager autoRefreshManager, ILogger<AutoRefreshController> logger, IServiceProvider serviceProvider)
    {
        this.autoRefreshManager = autoRefreshManager;
        this.logger = logger;
        this.serviceProvider = serviceProvider;
    }

    [HttpGet("GetAll")]
    public IEnumerable<AutoRefreshInput> GetAll()
    {
        logger.LogDebug("Get All Auto Refresh Inputs in Db");
        var list = autoRefreshManager.GetAll(HttpContext.GetUser());

        return list;
    }

    [HttpPost("Add")]
    public int AddAreaEntity(AutoRefreshInput refreshInput)
    {
        logger.LogDebug("Add Auto Refresh Input in Db (x: {x}, y: {y})", refreshInput.CoordX, refreshInput.CoordY);
        var result = autoRefreshManager.Add(refreshInput, HttpContext.GetUser());

        return result;
    }

    [HttpPatch("Update")]
    public ActionResult<int> UpdateAreaEntity(AutoRefreshInput refreshInput)
    {
        logger.LogDebug("Update Auto Refresh Input in Db (id: {id})", refreshInput.Id);
        var result = autoRefreshManager.Update(refreshInput, HttpContext.GetUser());

        if (result.IsError)
        {
            return new BadRequestObjectResult(result.Errors);
        }
        return result.Value;
    }

    [HttpDelete("Delete")]
    public ActionResult<int> DeleteAreaEntity(int inputId)
    {
        logger.LogDebug("Delete Auto Refresh Input in Db (id: {id})", inputId);
        var result = autoRefreshManager.Delete(inputId, HttpContext.GetUser());

        if (result.IsError)
        {
            return new BadRequestObjectResult(result.Errors);
        }
        return result.Value;
    }

    [HttpPost("RunAll")]
    public ActionResult<bool> RunAll()
    {
        try
        {
            logger.LogDebug("Run All Auto Refresh");
            var scope = serviceProvider.CreateAsyncScope();
            var autoRefreshManager = scope.ServiceProvider.GetService<AutoRefreshManager>();
            Task.Run(async () => await autoRefreshManager.RefreshAll(HttpContext.GetUser()));
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Erreur pendant le lancement des refreshs d'un User");
            return false;
        }
    }
}

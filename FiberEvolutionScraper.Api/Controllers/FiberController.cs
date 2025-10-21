using FiberEvolutionScraper.Api.Managers;
using FiberEvolutionScraper.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiberEvolutionScraper.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class FiberController : ControllerBase
{
    private readonly FiberManager FiberManager;

    public FiberController(FiberManager fiberManager)
    {
        FiberManager = fiberManager;
    }

    [HttpGet()]
    [AllowAnonymous]
    public IList<FiberPoint> GetFibers([FromQuery] FibersGetModel parameters)
    {
        var fibers = FiberManager.GetDbFibersForLoc(parameters.CoordX, parameters.CoordY);
        return fibers;
    }

    [HttpGet()]
    public async Task<IList<FiberPoint>> GetWideArea([FromQuery] FibersGetModel parameters)
    {
        var fibers = await FiberManager.GetFibersForLocAsync(parameters.CoordX, parameters.CoordY);
        await FiberManager.SaveToDB([.. fibers]);

        return fibers;
    }

    [HttpGet()]
    public async Task<IList<FiberPoint>> GetCloseArea([FromQuery] FibersGetModel parameters)
    {
        var fibers = await FiberManager.GetFibersForLocAsync(parameters.CoordX, parameters.CoordY, 1);
        await FiberManager.SaveToDB([.. fibers]);

        return fibers;
    }

    [HttpGet()]
    [AllowAnonymous]
    public IList<FiberPoint> GetNewestPoints([FromQuery] string data)
    {
        var fibers = FiberManager.GetNewestPoints(data);
        return fibers;
    }

    [HttpGet()]
    [AllowAnonymous]
    public FiberPoint GetSameSignaturePoints([FromQuery] string signature)
    {
        var fibers = FiberManager.GetSameSignaturePoints(signature);
        return fibers;
    }

    [HttpGet]
    public int UpdateWideArea([FromQuery] FibersGetModel parameters)
    {
        var result = FiberManager.UpdateDbFibers(parameters.CoordX, parameters.CoordY);

        return result.Result;
    }
}

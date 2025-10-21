using FiberEvolutionScraper.Api.Models;
using FiberEvolutionScraper.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FiberEvolutionScraper.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class FiberController
{
    private readonly FiberManager FiberManager;

    public FiberController(FiberManager fiberManager)
    {
        FiberManager = fiberManager;
    }

    [HttpGet()]
    public IList<FiberPointDTO> GetFibers([FromQuery] FibersGetModel parameters)
    {
        var fibers = FiberManager.GetDbFibersForLoc(parameters.CoordX, parameters.CoordY);
        return fibers;
    }

    [HttpGet()]
    public async Task<IList<FiberPointDTO>> GetWideArea([FromQuery] FibersGetModel parameters)
    {
        var fibers = await FiberManager.GetFibersForLocAsync(parameters.CoordX, parameters.CoordY);
        await FiberManager.SaveToDB([.. fibers]);

        return fibers;
    }

    [HttpGet()]
    public async Task<IList<FiberPointDTO>> GetCloseArea([FromQuery] FibersGetModel parameters)
    {
        var fibers = await FiberManager.GetFibersForLocAsync(parameters.CoordX, parameters.CoordY, 1);
        await FiberManager.SaveToDB([.. fibers]);

        return fibers;
    }

    [HttpGet()]
    public IList<FiberPointDTO> GetNewestPoints([FromQuery] string data)
    {
        var fibers = FiberManager.GetNewestPoints(data);
        return fibers;
    }

    [HttpGet()]
    public FiberPointDTO GetSameSignaturePoints([FromQuery] string signature)
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

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
        var fibers = FiberManager.GetDbFibersForLoc(parameters.CoordY, parameters.CoordX);
        return fibers;
    }

    [HttpGet()]
    public async Task<IList<FiberPointDTO>> GetWideArea([FromQuery] FibersGetModel parameters)
    {
        var fibers = FiberManager.GetFibersForLoc(parameters.CoordY, parameters.CoordX);
        await FiberManager.SaveToDB(fibers.ToList());

        return fibers;
    }

    [HttpGet()]
    public async Task<IList<FiberPointDTO>> GetCloseArea([FromQuery] FibersGetModel parameters)
    {
        var fibers = FiberManager.GetFibersForLoc(parameters.CoordY, parameters.CoordX, 1);
        await FiberManager.SaveToDB(fibers.ToList());

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
        var result = FiberManager.UpdateDbFibers(parameters.CoordY, parameters.CoordX);

        return result.Result;
    }
}

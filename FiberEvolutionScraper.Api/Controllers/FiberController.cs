using FiberEvolutionScraper.Api.Models;
using FiberEvolutionScraper.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiberEvolutionScraper.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class FiberController
{
    private FiberService fiberService;

    public FiberController(IServiceProvider serviceProvider)
    {
        fiberService = serviceProvider.GetService<FiberService>();
    }

    [HttpGet()]
    public IList<FiberPointDTO> GetFibers([FromQuery] FibersGetModel parameters)
    {
        var fibers = fiberService.GetDbFibersForLoc(parameters.CoordY, parameters.CoordX);
        return fibers;
    }

    [HttpGet()]
    public IList<FiberPointDTO> GetWideArea([FromQuery] FibersGetModel parameters)
    {
        var fibers = fiberService.GetFibersForLoc(parameters.CoordY, parameters.CoordX);
        return fibers;
    }

    [HttpGet()]
    public IList<FiberPointDTO> GetCloseArea([FromQuery] FibersGetModel parameters)
    {
        var fibers = fiberService.GetFibersForLoc(parameters.CoordY, parameters.CoordX, 1);
        return fibers;
    }

    [HttpGet()]
    public IList<FiberPointDTO> GetNewestPoints([FromQuery] string data)
    {
        var fibers = fiberService.GetNewestPoints(data);
        return fibers;
    }

    [HttpGet()]
    public FiberPointDTO GetSameSignaturePoints([FromQuery] string signature)
    {
        var fibers = fiberService.GetSameSignaturePoints(signature);
        return fibers;
    }

    [HttpGet]
    public int UpdateWideArea([FromQuery] FibersGetModel parameters)
    {
        var result = fiberService.UpdateDbFibers(parameters.CoordY, parameters.CoordX);
        return result;
    }
}

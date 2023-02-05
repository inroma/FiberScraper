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
    [AllowAnonymous]
    public IList<FiberPointDTO> GetFibers([FromQuery] FibersGetModel parameters)
    {
        var fibers = fiberService.GetDbFibersForLoc(parameters.CoordY, parameters.CoordX);
        return fibers;
    }

    [HttpGet()]
    [AllowAnonymous]
    public IList<FiberPointDTO> GetWideArea([FromQuery] FibersGetModel parameters)
    {
        var fibers = fiberService.GetFibersForLoc(parameters.CoordY, parameters.CoordX);
        return fibers;
    }

    [HttpGet()]
    [AllowAnonymous]
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

    [HttpPost()]
    [AllowAnonymous]
    public IList<FiberPointDTO> GetSameSignaturePoints([FromBody] FiberPointDTO fiber)
    {
        if(fiber.EtapeFtth != EtapeFtth.DEBUG)
        {
            var fibers = fiberService.GetSameSignaturePoints(fiber);
            return fibers;
        }
        return new List<FiberPointDTO>();
    }
}

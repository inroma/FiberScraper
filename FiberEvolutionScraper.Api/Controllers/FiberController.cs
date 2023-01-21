using FiberEvolutionScraper.Api.Models;
using FiberEvolutionScraper.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiberEvolutionScraper.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class FiberController
{
    private FiberApi fiberApi;

    public FiberController(IServiceProvider serviceProvider)
    {
        fiberApi = serviceProvider.GetService<FiberApi>();
    }

    [HttpGet()]
    [AllowAnonymous]
    public FiberResponseModel GetFibers([FromQuery] FibersGetModel parameters)
    {
        var fibers = fiberApi.GetFibersForLoc(parameters.CoordX, parameters.CoordY);
        return fibers;
    }

    [HttpGet()]
    [AllowAnonymous]
    public FiberResponseModel RefreshFibers([FromQuery] FibersGetModel parameters)
    {
        var fibers = fiberApi.GetFibersForLoc(parameters.CoordX, parameters.CoordY);
        return fibers;
    }

    [HttpGet()]
    [AllowAnonymous]
    public FiberResponseModel GetLiveDataFibers([FromQuery] FibersGetModel parameters)
    {
        var fibers = fiberApi.GetFibersForLoc(parameters.CoordX, parameters.CoordY);
        return fibers;
    }
}

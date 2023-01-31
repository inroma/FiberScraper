using AutoMapper;
using FiberEvolutionScraper.Api.Api;
using FiberEvolutionScraper.Api.Data;
using FiberEvolutionScraper.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FiberEvolutionScraper.Api.Services;

public class FiberService
{
    ApplicationDbContext context;
    FiberApi fiberApi;
    IMapper mapper;

    public FiberService(IServiceProvider serviceProvider)
    {
        context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        fiberApi = serviceProvider.GetRequiredService<FiberApi>();
        mapper = serviceProvider.GetRequiredService<IMapper>();
    }

    internal IList<FiberPointDTO> GetFibersForLoc(double coordX, double coordY, int squareSize = 5, bool canIterate = true)
    {
        var fibers = fiberApi.GetFibersForLoc(coordX, coordY, squareSize, canIterate);

        var mapped = mapper.Map<IList<FiberPointDTO>>(fibers.Results).Where(x => !x.EtapeFtth.StartsWith("DEBUG_"));

        try
        {
            context.FiberPoints.AddRange(mapped.Where(m => !context.FiberPoints.Contains(m)));
            context.FiberPoints.UpdateRange(mapped.Where(m => context.FiberPoints.Contains(m)));
            context.SaveChanges();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                if (entry.Entity is FiberPointDTO)
                {
                    if (entry.GetDatabaseValues() == null)
                    {
                        context.FiberPoints.Add(entry.Entity as FiberPointDTO);
                    }
                }
            }
            context.SaveChanges();
        }

        return mapper.Map<IList<FiberPointDTO>>(fibers.Results);
    }

    internal IList<FiberPointDTO> GetDbFibersForLoc(double coordX, double coordY)
    {
        var result = context.FiberPoints.ToList().GroupBy(x => Math.Pow((coordY - x.X), 2) + Math.Pow((coordX - x.Y), 2))
            .OrderBy(x => x.Key).Take(1700).SelectMany(a => a.Select(b => b)).ToList();

        return result.ToList();
    }
}
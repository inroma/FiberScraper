using AutoMapper;
using FiberEvolutionScraper.Api.Api;
using FiberEvolutionScraper.Api.Data;
using FiberEvolutionScraper.Api.Models;
using System.Globalization;

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
        var mapped = mapper.Map<IList<FiberPointDTO>>(fibers.Results).Where(x => x.EtapeFtth != EtapeFtth.DEBUG).ToList();
        List<FiberPointDTO> resultFibers = new();

        var count = fibers.Results.Count;
        mapped = mapped.GroupBy(f => new { f.Signature, f.EtapeFtth }).Select(g => g.MinBy(d => d.EtapeFtth)).ToList();
        Console.WriteLine("Delta: {0}", count - mapped.Count);

        try
        {
            var minX = mapped.Min(x => x.X - 1);
            var maxX = mapped.Max(x => x.X + 1);
            var minY = mapped.Min(x => x.Y - 1);
            var maxY = mapped.Max(x => x.Y + 1);
            var dbFibers = context.FiberPoints.Where(ctx => ctx.X >= minX && ctx.X <= maxX && ctx.Y >= minY && ctx.Y <= maxY).ToList();

            mapped.ForEach(fiber => {
                var dbFiber = dbFibers.FirstOrDefault(f => f.Signature == fiber.Signature && f.EtapeFtth == fiber.EtapeFtth);
                if (dbFiber == null)
                {
                    fiber.Created = fiber.LastUpdated = DateTime.UtcNow;
                    context.FiberPoints.Add(fiber);
                    resultFibers.Add(fiber);
                }
                else
                {
                    dbFiber.LastUpdated = DateTime.UtcNow;
                    context.FiberPoints.Update(dbFiber);
                    resultFibers.Add(dbFiber);
                }
            });
            context.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
        }

        return resultFibers;
    }

    internal IList<FiberPointDTO> GetDbFibersForLoc(double coordX, double coordY)
    {
        var result = context.FiberPoints.OrderByDescending(x => x.LastUpdated).GroupBy(x => x.Signature).Select(a => a.First()).ToList()
            .GroupBy(x => Math.Pow((coordX - x.X), 2) + Math.Pow((coordY - x.Y), 2))
            .OrderBy(x => x.Key).SelectMany(a => a.Select(b => b)).ToList().Take(900).ToList();

        return result.ToList();
    }

    internal IList<FiberPointDTO> GetSameSignaturePoints(FiberPointDTO fiber)
    {
        var result = context.FiberPoints.Where(s => s.Signature == fiber.Signature && s.EtapeFtth != fiber.EtapeFtth).ToList();
        return result;
    }

    internal IList<FiberPointDTO> GetNewestPoints(string parameters)
    {
        var latlng = parameters.Split(",").Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToList();

        var result = context.FiberPoints
            .Where(f => f.LastUpdated >= DateTime.UtcNow.AddDays(-7) && f.X >= latlng[0] && f.Y >= latlng[1] && f.X <= latlng[2] && f.Y <= latlng[3])
            .ToList();

        return result
            .GroupBy(x => x.Signature).Select(a => a.OrderByDescending(a => a.LastUpdated).First()).Take(900).ToList();
    }
}
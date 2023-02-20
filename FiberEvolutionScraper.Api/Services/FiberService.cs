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
    IServiceProvider Services;

    public FiberService(IServiceProvider serviceProvider)
    {
        context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        fiberApi = serviceProvider.GetRequiredService<FiberApi>();
        mapper = serviceProvider.GetRequiredService<IMapper>();
        Services = serviceProvider;
    }

    internal IList<FiberPointDTO> GetFibersForLoc(double coordX, double coordY, int squareSize = 5, bool canIterate = true)
    {
        var fibers = fiberApi.GetFibersForLoc(coordX, coordY, squareSize, canIterate);
        var mapped = mapper.Map<IList<FiberPointDTO>>(fibers.Results).ToList();

        var count = fibers.Results.Count;
        mapped = mapped.DistinctBy(m => m.Signature).ToList();
        Console.WriteLine("Delta: {0}", count - mapped.Count);

        Task.Run(() => SaveToDB(mapped.Where(x => x.EligibilitesFtth.All(e => e.EtapeFtth != EtapeFtth.DEBUG)).ToList(), Services));

        return mapped;
    }

    internal IList<FiberPointDTO> GetDbFibersForLoc(double coordX, double coordY)
    {
        var result = context.FiberPoints.OrderByDescending(x => x.LastUpdated).GroupBy(x => x.Signature).Select(a => a.First()).ToList()
            .GroupBy(x => Math.Pow((coordX - x.X), 2) + Math.Pow((coordY - x.Y), 2))
            .OrderBy(x => x.Key).SelectMany(a => a.Select(b => b)).ToList().Take(1300).ToList();

        return result.ToList();
    }

    internal FiberPointDTO GetSameSignaturePoints(string signature)
    {
        var result = context.FiberPoints.First(s => s.Signature == signature);
        return result;
    }

    internal IList<FiberPointDTO> GetNewestPoints(string parameters)
    {
        var latlng = parameters.Split(",").Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToList();

        var result = context.FiberPoints
            .Where(f => f.EligibilitesFtth.Any(e => e.Created >= DateTime.UtcNow.AddDays(-7)) && f.X >= latlng[0] && f.Y >= latlng[1] && f.X <= latlng[2] && f.Y <= latlng[3])
            .ToList();

        return result
            .GroupBy(x => x.Signature).Select(a => a.OrderByDescending(a => a.LastUpdated).First()).Take(1300).ToList();
    }

    internal int UpdateDbFibers(double coordX, double coordY)
    {
        var fibers = fiberApi.GetFibersForLoc(coordX, coordY, 5, true);
        var mapped = mapper.Map<IList<FiberPointDTO>>(fibers.Results).ToList();
        mapped = mapped.DistinctBy(m => m.Signature).ToList();
        var entitySaved = SaveToDB(mapped, Services);

        return entitySaved;
    }

    private static int SaveToDB(List<FiberPointDTO> fiberPoints, IServiceProvider serviceProvider)
    {
        try
        {
            var scopedDbContext = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var minX = fiberPoints.Min(x => x.X - 1);
            var maxX = fiberPoints.Max(x => x.X + 1);
            var minY = fiberPoints.Min(x => x.Y - 1);
            var maxY = fiberPoints.Max(x => x.Y + 1);
            var dbFibers = scopedDbContext.FiberPoints.Where(ctx => ctx.X >= minX && ctx.X <= maxX && ctx.Y >= minY && ctx.Y <= maxY).ToList();

            fiberPoints.ForEach(fiber =>
            {
                var dbFiber = dbFibers.FirstOrDefault(f => f.Signature == fiber.Signature);
                if (dbFiber == null)
                {
                    fiber.Created = fiber.LastUpdated = DateTime.UtcNow;
                    scopedDbContext.FiberPoints.Add(fiber);
                }
                else
                {
                    dbFiber.LastUpdated = DateTime.UtcNow;
                    scopedDbContext.FiberPoints.Update(dbFiber);
                }
            });
            var result = scopedDbContext.SaveChanges();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
            return -1;
        }
    }
}
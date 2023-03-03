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
            .Where(f => f.X >= latlng[0] && f.Y >= latlng[1] && f.X <= latlng[2] && f.Y <= latlng[3])
            .Where(f => (f.EligibilitesFtth.Any(e => e.Created >= DateTime.UtcNow.AddDays(-5) || e.LastUpdated >= DateTime.UtcNow.AddDays(-1))
                    || (!f.EligibilitesFtth.Any() && f.Created >= DateTime.UtcNow.AddDays(-5)) || f.LastUpdated >= DateTime.UtcNow.AddDays(-1)))
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
            var minX = fiberPoints.Min(x => x.X - 0.2);
            var maxX = fiberPoints.Max(x => x.X + 0.2);
            var minY = fiberPoints.Min(x => x.Y - 0.2);
            var maxY = fiberPoints.Max(x => x.Y + 0.2);
            var dbFibers = scopedDbContext.FiberPoints.Where(ctx => ctx.X >= minX && ctx.X <= maxX && ctx.Y >= minY && ctx.Y <= maxY).ToList();

            fiberPoints.ForEach(fiber =>
            {
                var dbFiber = dbFibers.FirstOrDefault(f => f.Signature == fiber.Signature);
                if (dbFiber == null)
                {
                    fiber.LastUpdated = DateTime.UtcNow;
                    scopedDbContext.FiberPoints.Add(fiber);
                }
                else
                {
                    AddOrUpdateEligibiliteFtth(scopedDbContext, dbFiber, fiber);
                    if (fiber.X != dbFiber.X || fiber.Y != dbFiber.Y || fiber.LibAdresse != dbFiber.LibAdresse)
                    {
                        fiber.LastUpdated = DateTime.UtcNow;
                    }
                    dbFiber.X = fiber.X;
                    dbFiber.Y = fiber.Y;
                    dbFiber.LibAdresse = fiber.LibAdresse;
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

    private static void AddOrUpdateEligibiliteFtth(ApplicationDbContext dbContext, FiberPointDTO dbFiber, FiberPointDTO fiberPoint)
    {
        fiberPoint.EligibilitesFtth.ForEach(e =>
        {
            var dbE = dbFiber.EligibilitesFtth.FirstOrDefault(dbE => dbE.CodeImb == e.CodeImb && dbE.EtapeFtth == e.EtapeFtth);
            if (dbE != null)
            {
                if (dbE.Batiment != e.Batiment || dbE.DateDebutEligibilite != e.DateDebutEligibilite)
                {
                    dbE.LastUpdated = DateTime.UtcNow;
                }
                dbE.DateDebutEligibilite = e.DateDebutEligibilite;
                dbE.Batiment = e.Batiment;
                dbContext.EligibiliteFtth.Update(dbE);
            }
            else
            {
                e.LastUpdated = DateTime.UtcNow;
                dbFiber.EligibilitesFtth.Add(e);
            }
        });

    }
}
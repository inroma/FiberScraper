using AutoMapper;
using FiberEvolutionScraper.Api.Api;
using FiberEvolutionScraper.Api.Data;
using FiberEvolutionScraper.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace FiberEvolutionScraper.Api.Services;

public class FiberManager
{
    readonly ApplicationDbContext context;
    readonly FiberApi fiberApi;
    readonly IMapper mapper;

    public FiberManager(IServiceProvider serviceProvider)
    {
        context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        fiberApi = serviceProvider.GetRequiredService<FiberApi>();
        mapper = serviceProvider.GetRequiredService<IMapper>();
    }

    internal IList<FiberPointDTO> GetFibersForLoc(double coordX, double coordY, int squareSize = 5, bool canIterate = true)
    {
        var fibers = fiberApi.GetFibersForLoc(coordX, coordY, squareSize, canIterate);
        var mapped = mapper.Map<IList<FiberPointDTO>>(fibers).ToList();
        mapped = mapped.DistinctBy(m => m.Signature).ToList();

        return mapped;
    }

    internal IList<FiberPointDTO> GetDbFibersForLoc(double coordX, double coordY)
    {
        var result = context.FiberPoints.Include(f => f.EligibilitesFtth.OrderByDescending(e => e.LastUpdated))
            .GroupBy(e => e.Signature).Select(g => g.First()).ToList()
            .GroupBy(x => Math.Pow((coordX - x.X), 2) + Math.Pow(coordY - x.Y, 2))
            .OrderBy(x => x.Key).SelectMany(g => g.ToList()).Take(1500);

        return result.ToList();
    }

    internal FiberPointDTO GetSameSignaturePoints(string signature)
    {
        var result = context.FiberPoints.Include(f => f.EligibilitesFtth.OrderByDescending(e => e.LastUpdated)).First(s => s.Signature == signature);
        return result;
    }

    internal IList<FiberPointDTO> GetNewestPoints(string parameters)
    {
        var latlng = parameters.Split(",").Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToList();

        var result = context.FiberPoints.Include(f => f.EligibilitesFtth.OrderByDescending(e => e.LastUpdated))
            .Where(f => f.X >= latlng[0] && f.Y >= latlng[1] && f.X <= latlng[2] && f.Y <= latlng[3])
            .Where(f => f.EligibilitesFtth.Any(e => e.Created >= DateTime.UtcNow.AddDays(-6) || e.LastUpdated >= DateTime.UtcNow.AddDays(-3))
                    || f.EligibilitesFtth.Count == 0 && f.Created >= DateTime.UtcNow.AddDays(-6) || f.LastUpdated >= DateTime.UtcNow.AddDays(-3))
            .ToList();

        return result
            .GroupBy(x => x.Signature).Select(a => a.OrderBy(a => a.LastUpdated).First()).Take(2000).ToList();
    }

    internal async Task<int> UpdateDbFibers(double coordX, double coordY, int squareSize = 5)
    {
        var fibers = GetFibersForLoc(coordX, coordY, squareSize);

        return await SaveToDB([.. fibers]);
    }

    public async Task<int> SaveToDB(List<FiberPointDTO> fiberPoints)
    {
        try
        {
            if (fiberPoints.Count == 0)
            {
                return 0;
            }
            var xOffset = 0.6;
            var yOffset = 0.6;
            var minX = fiberPoints.Min(x => x.X - xOffset);
            var maxX = fiberPoints.Max(x => x.X + xOffset);
            var minY = fiberPoints.Min(x => x.Y - yOffset);
            var maxY = fiberPoints.Max(x => x.Y + yOffset);
            var dbFibers = context.FiberPoints.Where(ctx => ctx.X >= minX && ctx.X <= maxX && ctx.Y >= minY && ctx.Y <= maxY).ToList();

            fiberPoints.ForEach(fiber =>
            {
                var dbFiber = dbFibers.FirstOrDefault(f => f.Signature == fiber.Signature);
                if (dbFiber == null)
                {
                    fiber.Created = DateTime.UtcNow;
                    fiber.LastUpdated = fiber.Created;
                    fiber.EligibilitesFtth.ForEach(e =>
                    {
                        e.Created = fiber.Created;
                        e.LastUpdated = fiber.Created;
                    });
                    context.FiberPoints.Add(fiber);
                }
                else
                {
                    AddOrUpdateEligibiliteFtth(dbFiber, fiber);
                    if (fiber.X != dbFiber.X || fiber.Y != dbFiber.Y || fiber.LibAdresse != dbFiber.LibAdresse)
                    {
                        fiber.LastUpdated = DateTime.UtcNow;
                    }
                    dbFiber.X = fiber.X;
                    dbFiber.Y = fiber.Y;
                    dbFiber.LibAdresse = fiber.LibAdresse;
                    context.FiberPoints.Update(dbFiber);
                }
            });
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
            return -1;
        }
    }

    private static void AddOrUpdateEligibiliteFtth(FiberPointDTO dbFiber, FiberPointDTO fiberPoint)
    {
        foreach (var item in fiberPoint.EligibilitesFtth)
        {
            var dbE = dbFiber.EligibilitesFtth.OrderByDescending(dbe => dbe.LastUpdated).FirstOrDefault(dbE => dbE.CodeImb == item.CodeImb && dbE.EtapeFtth == item.EtapeFtth);
            if (dbE == null)
            {
                item.Created = DateTime.UtcNow;
                item.LastUpdated = DateTime.UtcNow;
                dbFiber.EligibilitesFtth.Add(item);
            }
            // Dans le cas où le statut repasse sur un statut connu en BDD
            else
            {
                var mostRecent = dbFiber.EligibilitesFtth.OrderByDescending(dbe => dbe.LastUpdated).FirstOrDefault(dbE => dbE.CodeImb == item.CodeImb);
                if (mostRecent != null && mostRecent.EtapeFtth != item.EtapeFtth)
                {
                    dbE.LastUpdated = DateTime.UtcNow;
                }
                if (dbE.Batiment != item.Batiment)
                {
                    dbE.LastUpdated = DateTime.UtcNow;
                    dbE.Batiment = item.Batiment;
                }
            }
        }
    }
}
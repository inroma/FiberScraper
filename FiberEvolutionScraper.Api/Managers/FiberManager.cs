using AutoMapper;
using FiberEvolutionScraper.Api.Api;
using FiberEvolutionScraper.Api.Data;
using FiberEvolutionScraper.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FiberEvolutionScraper.Api.Managers;

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

    internal async Task<IList<FiberPoint>> GetFibersForLocAsync(double coordX, double coordY, int squareSize = 3, bool canIterate = true)
    {
        var fibers = await fiberApi.GetFibersForLocAsync(coordX, coordY, squareSize, canIterate);
        var mapped = mapper.Map<IList<FiberPoint>>(fibers).ToList();
        mapped = [.. mapped.DistinctBy(m => m.Signature)];

        return mapped;
    }

    internal IList<FiberPoint> GetDbFibersForLoc(double minX, double minY, double maxX, double maxY)
    {
        var result = context.FiberPoints.Include(f => f.EligibilitesFtth.OrderByDescending(e => e.LastUpdated))
            .Where(f => f.X >= minX && f.Y >= minY && f.X <= maxX && f.Y <= maxY)
            .GroupBy(e => e.Signature).Select(g => g.First())
            .Take(1500);

        return [.. result];
    }

    internal FiberPoint GetSameSignaturePoints(string signature)
    {
        var result = context.FiberPoints.Include(f => f.EligibilitesFtth.OrderByDescending(e => e.LastUpdated)).First(s => s.Signature == signature);
        return result;
    }

    internal IList<FiberPoint> GetNewestPoints(double minX, double minY, double maxX, double maxY)
    {
        var result = context.FiberPoints.Include(f => f.EligibilitesFtth.OrderByDescending(e => e.LastUpdated))
            .Where(f => f.X >= minX && f.Y >= minY && f.X <= maxX && f.Y <= maxY).ToList()
            .Where(f => f.EligibilitesFtth.Any(e => e.Created >= DateTime.UtcNow.AddDays(-6) || e.LastUpdated >= DateTime.UtcNow.AddDays(-3))
                    || f.EligibilitesFtth.Count == 0 && f.Created >= DateTime.UtcNow.AddDays(-6) || f.LastUpdated >= DateTime.UtcNow.AddDays(-3));

        return [.. result.GroupBy(x => x.Signature).Select(a => a.OrderBy(a => a.LastUpdated).First()).Take(2000)];
    }

    internal async Task<int> UpdateDbFibers(double coordX, double coordY, int squareSize = 5)
    {
        var fibers = await GetFibersForLocAsync(coordX, coordY, squareSize);

        return await SaveToDB([.. fibers]);
    }

    public async Task<int> SaveToDB(List<FiberPoint> fiberPoints)
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

    private static void AddOrUpdateEligibiliteFtth(FiberPoint dbFiber, FiberPoint fiberPoint)
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
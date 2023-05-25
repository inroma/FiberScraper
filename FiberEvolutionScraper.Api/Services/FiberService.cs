using AutoMapper;
using FiberEvolutionScraper.Api.Api;
using FiberEvolutionScraper.Api.Data;
using FiberEvolutionScraper.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace FiberEvolutionScraper.Api.Services;

public class FiberService
{
    readonly ApplicationDbContext context;
    readonly FiberApi fiberApi;
    readonly IMapper mapper;

    public FiberService(IServiceProvider serviceProvider)
    {
        context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        fiberApi = serviceProvider.GetRequiredService<FiberApi>();
        mapper = serviceProvider.GetRequiredService<IMapper>();
    }

    internal IList<FiberPointDTO> GetFibersForLoc(double coordX, double coordY, int squareSize = 5, bool canIterate = true)
    {
        var fibers = fiberApi.GetFibersForLoc(coordX, coordY, squareSize, canIterate);
        var mapped = mapper.Map<IList<FiberPointDTO>>(fibers.Results).ToList();
        mapped = mapped.DistinctBy(m => m.Signature).ToList();

        return mapped;
    }

    internal IList<FiberPointDTO> GetDbFibersForLoc(double coordX, double coordY)
    {
        var result = context.FiberPoints.Include(f => f.EligibilitesFtth.OrderByDescending(e => e.LastUpdated))
            .GroupBy(x => x.Signature).Select(a => a.First()).ToList()
            .GroupBy(x => Math.Pow((coordX - x.X), 2) + Math.Pow((coordY - x.Y), 2))
            .OrderBy(x => x.Key).SelectMany(a => a.Select(b => b)).ToList().Take(500).ToList();

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
            .Where(f => (f.EligibilitesFtth.Any(e => e.Created >= DateTime.UtcNow.AddDays(-5) || e.LastUpdated >= DateTime.UtcNow.AddDays(-1))
                    || !f.EligibilitesFtth.Any() && f.Created >= DateTime.UtcNow.AddDays(-5) || f.LastUpdated >= DateTime.UtcNow.AddDays(-1)))
            .ToList();

        return result
            .GroupBy(x => x.Signature).Select(a => a.OrderBy(a => a.LastUpdated).First()).Take(800).ToList();
    }

    internal async Task<int> UpdateDbFibers(double coordX, double coordY)
    {
        var fibers = GetFibersForLoc(coordX, coordY, 5, true);

        return await SaveToDB(fibers.ToList());
    }

    public async Task<int> SaveToDB(List<FiberPointDTO> fiberPoints)
    {
        try
        {
            var xOffset = 0.8;
            var yOffset = 0.8;
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
                    fiber.LastUpdated = DateTime.UtcNow;
                    context.FiberPoints.Add(fiber);
                }
                else
                {
                    AddOrUpdateEligibiliteFtth(context, dbFiber, fiber);
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
                e.Created = DateTime.UtcNow;
                e.LastUpdated = DateTime.UtcNow;
                dbFiber.EligibilitesFtth.Add(e);
            }
        });

    }
}
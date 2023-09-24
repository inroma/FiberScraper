using FiberEvolutionScraper.Api.Data;
using FiberEvolutionScraper.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FiberEvolutionScraper.Api.Services;

public class AutoRefreshManager
{
    private readonly ApplicationDbContext context;
    private readonly ILogger<AutoRefreshManager> logger;

    public AutoRefreshManager(ApplicationDbContext context, ILogger<AutoRefreshManager> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    public async Task<IEnumerable<AutoRefreshInput>> GetAll()
    {
        var list = await context.AutoRefreshInputs.ToListAsync();
        return list;
    }

    public async Task<int> Add(AutoRefreshInput input)
    {
        await context.AutoRefreshInputs.AddAsync(input);
        return await context.SaveChangesAsync();
    }

    public async Task<int> AddRange(IEnumerable<AutoRefreshInput> list)
    {
        await context.AutoRefreshInputs.AddRangeAsync(list);
        return await context.SaveChangesAsync();
    }

    public async Task<int> Delete(int inputId)
    {
        var item = await context.AutoRefreshInputs.FirstOrDefaultAsync(x => x.Id == inputId);
        if (item != null)
        {
            context.AutoRefreshInputs.Remove(item);
            return await context.SaveChangesAsync();
        }
        throw new($"Aucun enregistrement en BDD avec cet Id: {inputId}");
    }

    public async Task<int> Delete(AutoRefreshInput input)
    {
        context.AutoRefreshInputs.Remove(input);
        return await context.SaveChangesAsync();
    }

    public async Task<int> Update(AutoRefreshInput input)
    {
        try
        {
            context.AutoRefreshInputs.Update(input);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex.InnerException.Message ?? ex.Message);
            return -1;
        }
    }
}
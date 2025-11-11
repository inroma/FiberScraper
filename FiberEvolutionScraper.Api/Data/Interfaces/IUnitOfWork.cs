namespace FiberEvolutionScraper.Api.Data.Interfaces;

using FiberEvolutionScraper.Api.Models;

public interface IUnitOfWork<TContext> : IDisposable
    where TContext : ApplicationDbContext
{
    public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseModel;

    /// <summary>
    /// Commit all changes made in this context to the database.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    public int Save();
}

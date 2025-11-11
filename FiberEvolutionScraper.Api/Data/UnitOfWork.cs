namespace FiberEvolutionScraper.Api.Data;

using FiberEvolutionScraper.Api.Data.Interfaces;
using FiberEvolutionScraper.Api.Models;
using FiberEvolutionScraper.Api.Models.User;

public class UnitOfWork<TContext>(TContext context) : IUnitOfWork<TContext>
    where TContext: ApplicationDbContext
{
    private readonly ApplicationDbContext context = context;
    private Dictionary<Type, object> _repositories;

    public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseModel
    {
        _repositories ??= [];
        var type = typeof(TEntity);
        if (!_repositories.TryGetValue(type, out object value))
        {
            value = new GenericRepository<TEntity>(context);
            _repositories[type] = value;
        }
        return (IGenericRepository<TEntity>)value;
    }

    public int Save() => context.SaveChanges();

    private bool disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                context.Dispose();
            }
        }
        disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
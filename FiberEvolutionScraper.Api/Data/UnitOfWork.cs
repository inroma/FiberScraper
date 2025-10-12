namespace FiberEvolutionScraper.Api.Data;

using FiberEvolutionScraper.Api.Models;
using FiberEvolutionScraper.Api.Models.User;

public class UnitOfWork : IDisposable
{
    private readonly ApplicationDbContext context = new();
    private GenericRepository<User> userRepository;
    private GenericRepository<FiberPoint> fiberRepository;

    public GenericRepository<User> UserRepository
    {
        get
        {
            userRepository ??= new GenericRepository<User>(context);
            return userRepository;
        }
    }

    public GenericRepository<FiberPoint> CourseRepository
    {
        get
        {

            fiberRepository ??= new GenericRepository<FiberPoint>(context);
            return fiberRepository;
        }
    }

    public void Save()
    {
        context.SaveChanges();
    }

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
namespace FiberEvolutionScraper.Api.Middlewares;

using FiberEvolutionScraper.Api.Data;
using FiberEvolutionScraper.Api.Data.Interfaces;
using FiberEvolutionScraper.Api.Models.User;
using System.Security.Claims;

public class AuthenticatedUserMiddleware
{
    private readonly RequestDelegate next;

    public AuthenticatedUserMiddleware(RequestDelegate request)
    {
        next = request;
    }

    public async Task InvokeAsync(HttpContext context, IUnitOfWork<ApplicationDbContext> unitOfWork)
    {
        var userId = context.User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            context.Items["AuthenticatedUser"] = unitOfWork.GetRepository<UserModel>().Get(u => u.UId == userId).First();
        }

        // Call the next delegate/middleware in the pipeline.
        await next(context);
    }
}

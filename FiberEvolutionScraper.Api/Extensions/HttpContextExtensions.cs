namespace FiberEvolutionScraper.Api.Extensions;

using FiberEvolutionScraper.Api.Models.User;

public static class HttpContextExtensions
{
    public static UserModel GetUser(this HttpContext context)
    {
        return (UserModel)context.Items["AuthenticatedUser"];
    }
}

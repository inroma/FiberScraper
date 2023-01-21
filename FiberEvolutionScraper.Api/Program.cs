using FiberEvolutionScraper.Api.Services;

namespace FiberEvolutionScraper.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddTransient<FiberApi>();
        builder.Services.AddScoped<HttpClient>();
        builder.Services.AddSingleton<TokenParser>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();
        app.UseCors(builder => builder.AllowAnyOrigin());

        app.MapControllers();

        app.Run();
    }
}
using FiberEvolutionScraper.Api.Api;
using FiberEvolutionScraper.Api.Data;
using FiberEvolutionScraper.Api.Models;
using FiberEvolutionScraper.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace FiberEvolutionScraper.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
        string dbConnectionString = configuration.GetConnectionString("Database");
        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddScoped<FiberApi>();
        builder.Services.AddScoped<FiberService>();
        builder.Services.AddScoped<HttpClient>();
        builder.Services.AddSingleton<TokenParser>();
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(dbConnectionString));
        builder.Services.AddAutoMapper(cfg => {
            cfg.AllowNullCollections = true;
            cfg.CreateMap<FiberPoint, FiberPointDTO>()
                .ForMember(dest => dest.Signature, act => act.MapFrom(src => src.Address.Signature))
                .ForMember(dest => dest.X, act => act.MapFrom(src => src.Address.BestCoords.Coord.X))
                .ForMember(dest => dest.Y, act => act.MapFrom(src => src.Address.BestCoords.Coord.Y))
                .ForMember(dest => dest.LibAdresse, act => act.MapFrom(src => src.Address.LibAdresse))
                .ForMember(dest => dest.EligibilitesFtth, act => act.MapFrom(src => src.EligibilitesFtth))
                .ReverseMap()
            ;
            cfg.CreateMap<EligibilitesFtth, EligibiliteFtthDTO>().ReverseMap();
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();
        app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

        app.MapControllers();

        app.Run();
    }
}
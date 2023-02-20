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
        builder.Services.AddTransient<FiberService>();
        builder.Services.AddScoped<HttpClient>();
        builder.Services.AddSingleton<TokenParser>();
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(dbConnectionString));
        builder.Services.AddAutoMapper(cfg => {
            cfg.AllowNullCollections = true;
            cfg.CreateMap<FiberPoint, FiberPointDTO>()
                .ForMember(dest => dest.Signature, act => act.MapFrom(src => src.Address.Signature))
                .ForMember(dest => dest.X, act => act.MapFrom(src => src.Address.BestCoords.Coord.X))
                .ForMember(dest => dest.Y, act => act.MapFrom(src => src.Address.BestCoords.Coord.Y))
                .ForMember(dest => dest.ExtVoie, act => act.MapFrom(src => src.Address.ExtVoie))
                .ForMember(dest => dest.NumVoie, act => act.MapFrom(src => src.Address.NumVoie))
                .ForMember(dest => dest.CodeVoie, act => act.MapFrom(src => src.Address.CodeVoie))
                .ForMember(dest => dest.CodeCommune, act => act.MapFrom(src => src.Address.CodeCommune))
                .ForMember(dest => dest.CodePostal, act => act.MapFrom(src => src.Address.CodePostal))
                .ForMember(dest => dest.LibAdresse, act => act.MapFrom(src => src.Address.LibAdresse))
                .ForMember(dest => dest.LibCommune, act => act.MapFrom(src => src.Address.LibCommune))
                .ForMember(dest => dest.LibVoie, act => act.MapFrom(src => src.Address.LibVoie))
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

    private static EtapeFtth ResolveEnum(string etapeFtth)
    {
        if (etapeFtth.StartsWith("DEBUG_"))
        {
             return EtapeFtth.DEBUG;
        }
        switch (etapeFtth)
        {
            case "ELLIGIBLE":
                return EtapeFtth.ELLIGIBLE;
            case "EN_COURS_IMMEUBLE":
                return EtapeFtth.EN_COURS_IMMEUBLE;
            case "TERMINE_QUARTIER":
                return EtapeFtth.TERMINE_QUARTIER;
            case "EN_COURS_QUARTIER":
                return EtapeFtth.EN_COURS_QUARTIER;
            case "PREVU_QUARTIER":
                return EtapeFtth.PREVU_QUARTIER;
            default:
                return EtapeFtth._;
        }
    }
}
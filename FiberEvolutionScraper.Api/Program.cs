using FiberEvolutionScraper.Api.Api;
using FiberEvolutionScraper.Api.Data;
using FiberEvolutionScraper.Api.Models;
using FiberEvolutionScraper.Api.Services;
using Microsoft.EntityFrameworkCore;
using Quartz.AspNetCore;
using Quartz;

namespace FiberEvolutionScraper.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("files/appsettings.json")
            .Build();
        string dbConnectionString = configuration.GetConnectionString("Database");
        // Add services to the container.

        builder.Services.AddControllers();

        builder.Services.AddLogging(builder => 
            builder.AddConfiguration(configuration.GetSection("Logging"))
            .AddConsole());
        builder.Services.AddScoped<FiberApi>();
        builder.Services.AddScoped<FiberManager>();
        builder.Services.AddScoped<AutoRefreshManager>();
        builder.Services.AddScoped<AutoRefreshService>();
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

        builder.Services.AddQuartz(q =>
        {
            var jobKey = new JobKey("AutoRefreshJob");
            q.AddJob<AutoRefreshService>(opts => opts.WithIdentity(jobKey));

#if DEBUG
            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("AutoRefreshJob-trigger")
                .WithCronSchedule("0 0/5 * ? * *").StartNow()
            );
#else
            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("AutoRefreshJob-trigger")
                .WithCronSchedule("0 0 7,12,18 ? * *")
            );
#endif
        });

        builder.Services.AddQuartzServer(options =>
        {
            // when shutting down we want jobs to complete gracefully
            options.WaitForJobsToComplete = true;
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
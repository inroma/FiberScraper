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
        GlobalSettings globalConfiguration = configuration.GetSection(nameof(GlobalSettings)).Get<GlobalSettings>();
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
#if DEBUG
            q.ScheduleJob<AutoRefreshService>(trigger => trigger
                .WithIdentity("AutoRefreshJob-trigger")
                .WithCronSchedule(globalConfiguration?.CronSchedule ?? "0 0 0 ? * * 2077", x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time")))
            );
#else
            q.ScheduleJob<AutoRefreshService>(trigger => trigger
                .WithIdentity("AutoRefreshJob-trigger")
                .WithCronSchedule(globalConfiguration?.CronSchedule ?? "0 0 8,16 ? * MON,TUE,WED,THU,FRI,SAT", x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time")))
                .StartNow()
            );
#endif
        });

        builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

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
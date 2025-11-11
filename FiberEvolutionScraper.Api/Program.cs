using FiberEvolutionScraper.Api.Api;
using FiberEvolutionScraper.Api.Data;
using FiberEvolutionScraper.Api.Data.Interfaces;
using FiberEvolutionScraper.Api.Managers;
using FiberEvolutionScraper.Api.Middlewares;
using FiberEvolutionScraper.Api.Models;
using FiberEvolutionScraper.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Text.Json.Serialization;

namespace FiberEvolutionScraper.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("files/appsettings.json")
            .AddJsonFile($"files/appsettings.{builder.Environment.EnvironmentName}.json", true, true)
            .Build();
        GlobalSettings globalConfiguration = configuration.GetSection(nameof(GlobalSettings)).Get<GlobalSettings>();
        string dbConnectionString = configuration.GetConnectionString("Database");
        // Add services to the container.

        builder.Services.AddLogging(builder => 
            builder.AddConfiguration(configuration.GetSection("Logging"))
            .AddConsole());
        builder.Services
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>())
            .AddScoped<FiberApi>()
            .AddScoped<FiberManager>()
            .AddScoped<AutoRefreshManager>()
            .AddScoped<AutoRefreshService>()
            .AddScoped<HttpClient>()
            .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
            .AddSingleton<TokenParser>()
            .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(dbConnectionString))
            .AddScoped<IUnitOfWork<ApplicationDbContext>, UnitOfWork<ApplicationDbContext>>()
            .AddAutoMapper(cfg => {
                cfg.AllowNullCollections = true;
                cfg.CreateMap<FiberPointResponse, FiberPoint>()
                    .ForMember(dest => dest.Signature, act => act.MapFrom(src => src.Address.Signature))
                    .ForMember(dest => dest.X, act => act.MapFrom(src => src.Address.BestCoords.Coord.X))
                    .ForMember(dest => dest.Y, act => act.MapFrom(src => src.Address.BestCoords.Coord.Y))
                    .ForMember(dest => dest.LibAdresse, act => act.MapFrom(src => src.Address.LibAdresse))
                .ReverseMap();
                cfg.CreateMap<EligibilitesFtth, EligibiliteFtth>()
                    .ForMember(d => d.EtapeFtth, src => {
                        EtapeFtth result;
                        src.MapFrom(s => Enum.TryParse(s.EtapeFtth, true, out result) ? result : EtapeFtth.UNKNOWN);
                    })
                .ReverseMap();
            })
            .AddQuartz(q =>
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
            })
            .AddQuartzHostedService(q => q.WaitForJobsToComplete = true)
            .AddAuthorization()
            .AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = globalConfiguration.OAuth.Issuer;
                options.Audience = globalConfiguration.OAuth.Audience;
            });
        builder.Services.AddControllers()
            .AddJsonOptions(opts => {
                opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }

        app.UsePathBase(new("/api/"));
        app.UseHttpsRedirection()
            .UseCors()
            .UseAuthentication()
            .UseAuthorization();
        app.UseRouting();
        app.MapControllers().RequireAuthorization();

        app.UseMiddleware<AuthenticatedUserMiddleware>();

        app.Run();
    }
}
namespace FiberEvolutionScraper.Api.Models;

public class GlobalSettings
{
    public string CronSchedule { get; set; }

    public OAuth OAuth { get; set; }
}

public class OAuth
{
    public string Issuer { get; set; }
}
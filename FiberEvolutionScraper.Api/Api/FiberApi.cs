using FiberEvolutionScraper.Api.Services;
using FiberEvolutionScraper.Api.Models;
using System.Globalization;
using System.Text.Json;

namespace FiberEvolutionScraper.Api.Api;

public class FiberApi
{
    private readonly HttpClient client;
    private readonly TokenParser tokenParser;
    private readonly double offsetX = 0.006929;
    private readonly double offsetY = 0.004457;
    private readonly double offsetCityX = 0.006929 * .25;
    private readonly double offsetCityY = 0.004457 * .25;

    public FiberApi(IServiceProvider serviceProvider)
    {
        tokenParser = serviceProvider.GetRequiredService<TokenParser>();

        client = serviceProvider.GetRequiredService<HttpClient>();
        client.DefaultRequestHeaders.Accept.Add(new("application/json"));
        client.DefaultRequestHeaders.Accept.Add(new("text/plain"));
        client.DefaultRequestHeaders.Accept.Add(new("*/*"));
        client.BaseAddress = new("https://couverture-eligibilite.orange.fr/");
    }

    public async Task<IEnumerable<FiberPoint>> GetFibersForLocAsync(double x, double y, int squareSize = 3, bool canIterate = true)
    {
        SetToken();
        if (!client.DefaultRequestHeaders.Any(h => h.Key == "AppAuthorization"))
        {
            client.DefaultRequestHeaders.Add("AppAuthorization", $"Bearer {tokenParser.Token}");
        }
        var fibers = new List<FiberPoint>();
        HttpResponseMessage httpResponse;

        double currentOffsetX = canIterate ? offsetX : offsetCityX;
        double currentOffsetY = canIterate ? offsetY : offsetCityY;

        var modul = squareSize % 2 == 1 ? squareSize / 2 : squareSize / 2 - 0.5;
        (double startX, double startY) = (x - modul * currentOffsetX, y - modul * currentOffsetY);

        await Parallel.ForAsync(0, squareSize, async (j, m) =>
        {
            await Parallel.ForAsync(0, squareSize, async (k, state) =>
            {
                httpResponse = await client.GetAsync($"api/eligibilite/zoneAdresse?" +
                    $"x={(startX + (k * currentOffsetX)).ToString(CultureInfo.InvariantCulture)}&" +
                    $"y={(startY + (j * currentOffsetY)).ToString(CultureInfo.InvariantCulture)}&extendedZone=false", state);
                if (httpResponse.IsSuccessStatusCode)
                {
                    // On override le Header car Orange l'envoi parfois en double
                    httpResponse.Content.Headers.ContentType = new("application/json");
                    var tempResult = JsonSerializer.Deserialize<FiberResponseModel>(await httpResponse.Content.ReadAsStringAsync());
                    lock (fibers)
                    {
                        fibers.AddRange(tempResult.Result.Results);
                    }
                    if (tempResult.Result.ZoneSize != "GTC" && canIterate)
                    {
                        fibers.AddRange(await GetFibersForLocAsync(startX + (k * currentOffsetX), startY + (j * currentOffsetY), 3, false));
                    }
                    // AddDebugMarker(fibers, (startX, startY), j, k, currentOffsetY, currentOffsetX, squareSize, modul, tempResult.Result.ZoneSize);
                }
                else if (httpResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new Exception($"Erreur lors d'une requête API, {httpResponse.ReasonPhrase ?? httpResponse.StatusCode.ToString()}");
                }
            });
        });

        return fibers;
    }

    private static void AddDebugMarker(List<FiberPoint> fibers, (double X, double Y) startPoint, int j, int k, double currentOffsetY, double currentOffsetX, int squareSize, double modul, string zoneSize)
    {
        lock(fibers)
        {
            fibers.Add(new FiberPoint()
            {
                Address = new()
                {
                    BestCoords = new BestCoords()
                    {
                        Coord = new Coord()
                        {
                            X = startPoint.X + k * currentOffsetX,
                            Y = startPoint.Y + j * currentOffsetY
                        }
                    },
                    Signature = $"{startPoint.X + k * currentOffsetX + startPoint.Y + j * currentOffsetY + modul + squareSize + j + k}",
                    LibAdresse = "DEBUG_" + zoneSize
                },
                EligibilitesFtth = [new() { EtapeFtth = EtapeFtth.DEBUG.ToString() }]
            });
        }
    }

    private void SetToken()
    {
        if (tokenParser.Token == null || tokenParser.TokenAge < DateTime.Now.AddHours(-1))
        {
            tokenParser.GetToken(GetOrangeToken(tokenParser.GenerateAppId()));
            client.DefaultRequestHeaders.Add("AppAuthorization", $"Bearer {tokenParser.Token}");
        }
    }

    public string GetOrangeToken(string appId)
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("appId", appId);
        var result = httpClient.GetAsync("https://couverture-eligibilite.orange.fr/api/appConfig.json").Result;
        return result.Headers.GetValues("Token").FirstOrDefault();
    }
}

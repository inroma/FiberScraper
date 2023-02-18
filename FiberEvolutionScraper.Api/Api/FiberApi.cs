namespace FiberEvolutionScraper.Api.Api;

using FiberEvolutionScraper.Api.Services;
using FiberEvolutionScraper.Api.Models;
using System.Globalization;
using System.Linq;
using System.Text.Json;

public class FiberApi
{
    HttpClient client;
    TokenParser tokenParser;
    double offsetX = 0.006929;
    double offsetY = 0.004457;
    double offsetCityX = 0.006929 * .6;
    double offsetCityY = 0.004457 * .6;

    public FiberApi(IServiceProvider serviceProvider)
    {
        tokenParser = serviceProvider.GetRequiredService<TokenParser>();

        client = serviceProvider.GetRequiredService<HttpClient>();
        client.DefaultRequestHeaders.Accept.Add(new("application/json"));
        client.DefaultRequestHeaders.Accept.Add(new("text/plain"));
        client.DefaultRequestHeaders.Accept.Add(new("*/*"));
        client.BaseAddress = new("https://couverture-eligibilite.orange.fr/");
    }

    public FiberResponseModel GetFibersForLoc(double x, double y, int squareSize = 5, bool canIterate = true)
    {
        setToken();
        if(!client.DefaultRequestHeaders.Any(h => h.Key == "AppAuthorization"))
        {
            client.DefaultRequestHeaders.Add("AppAuthorization", $"Bearer {tokenParser.Token}");
        }
        FiberResponseModel fibers = new();
        HttpResponseMessage httpResponse;

        double currentOffsetX = canIterate ? offsetX : offsetCityX;
        double currentOffsetY = canIterate ? offsetY : offsetCityY;

        var modul = squareSize % 2 == 1 ? squareSize / 2 : squareSize / 2 - 0.5;
        var startPoint = (x - modul * currentOffsetX, y - modul * currentOffsetY);

        var parallelResult = Parallel.For(0, squareSize, (j) =>
        {
            Parallel.For(0, squareSize, (k) =>
            {
                httpResponse = client.GetAsync($"api/eligibilite/zoneAdresse?x={(startPoint.Item1 + k * currentOffsetX).ToString(CultureInfo.InvariantCulture)}&y={(startPoint.Item2 + j * currentOffsetY).ToString(CultureInfo.InvariantCulture)}&extendedZone=false").Result;
                var tempResult = JsonSerializer.Deserialize<FiberResponseModel>(httpResponse.Content.ReadAsStringAsync().Result);
                lock (fibers.Results)
                {
                    if (tempResult.Results.Count > 1)
                    {
                        fibers.Results.AddRange(tempResult.Results);
                    }
                    else if (tempResult.Results.Count == 1)
                    {
                        fibers.Results.Add(tempResult.Results.First());
                    }
                }
                if (tempResult.Results.Any() && tempResult.ZoneSize != "GTC" && canIterate)
                {
                    fibers.Results.AddRange(GetFibersForLoc(startPoint.Item1 + k * currentOffsetX, startPoint.Item2 + j * currentOffsetY, 2, false).Results);
                }
                //AddDebugMarker(fibers, startPoint, j, k, currentOffsetY, currentOffsetX, squareSize, modul, tempResult.ZoneSize);
            });
        });

        return fibers;
    }

    private void AddDebugMarker(FiberResponseModel fibers, (double, double) startPoint, int j, int k, double currentOffsetY, double currentOffsetX, int squareSize, double modul, string zoneSize)
    {
        lock(fibers.Results)
        {
            fibers.Results.Add(new FiberPoint()
            {
                Address = new()
                {
                    BestCoords = new BestCoords()
                    {
                        Coord = new Coord()
                        {
                            X = startPoint.Item1 + k * currentOffsetX,
                            Y = startPoint.Item2 + j * currentOffsetY
                        }
                    },
                    Signature = $"{startPoint.Item1 + k * currentOffsetX + startPoint.Item2 + j * currentOffsetY + modul + squareSize + j + k}",
                    LibAdresse = $"{startPoint.Item1}, {startPoint.Item2}, {modul}, {j} + {k}"
                },
                EligibilitesFtth = new() { new() { EtapeFtth = "DEBUG_" + zoneSize } }
            });
        }
    }

    private void setToken()
    {
        if (tokenParser.Token == null || tokenParser.TokenAge < DateTime.Now.AddHours(-1.5))
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

using FiberEvolutionScraper.Api.Models;
using FiberEvolutionScraper.Api.Services;
using System.Globalization;
using System.Text.Json;
using FiberEvolutionScraper.Api.Managers;
using AutoMapper;

namespace FiberEvolutionScraper.Api.Api;

public class FiberApi
{
    private readonly HttpClient client;
    private readonly TokenParser tokenParser;
    private readonly IMapper mapper;
    private readonly double offsetX = 0.006929;
    private readonly double offsetY = 0.004457;
    private readonly double offsetCityX = 0.006929 * .25;
    private readonly double offsetCityY = 0.004457 * .25;

    public FiberApi(TokenParser tokenParser, HttpClient httpClient, IMapper mapper)
    {
        this.tokenParser = tokenParser;
        this.mapper = mapper;
        client = httpClient;
        client.DefaultRequestHeaders.Accept.Add(new("application/json"));
        client.DefaultRequestHeaders.Accept.Add(new("text/plain"));
        client.DefaultRequestHeaders.Accept.Add(new("*/*"));
        client.BaseAddress = new("https://couverture-eligibilite.orange.fr/");
    }

    public async Task<IEnumerable<FiberPoint>> GetFibersForLocAsync(double x, double y, int squareSize = 3, bool canIterate = true)
    {
        SetToken();
        var fibers = new List<FiberPoint>();

        var currentOffset = (X: canIterate ? offsetX : offsetCityX, Y: canIterate ? offsetY : offsetCityY);

        var modul = squareSize % 2 == 1 ? squareSize / 2 : squareSize / 2 - 0.5;
        var start = (X: x - modul * currentOffset.X, Y: y - modul * currentOffset.Y);

        await Parallel.ForAsync(0, squareSize, async (j, m) =>
        {
            await Parallel.ForAsync(0, squareSize, async (k, state) =>
            {
                await ProcessCurrentArea(fibers, start, currentOffset, j, k, canIterate, state);
            });
        });

        return fibers;
    }

    private async Task ProcessCurrentArea(List<FiberPoint> fibers, (double X, double Y) start,
        (double X, double Y) currentOffset, int j, int k, bool canIterate, CancellationToken cancellationToken, bool tokenAlreadyRenewed = false)
    {
        var httpResponse = await client.GetAsync($"api/eligibilite/zoneAdresse?" +
                    $"x={(start.X + (k * currentOffset.X)).ToString(CultureInfo.InvariantCulture)}&" +
                    $"y={(start.Y + (j * currentOffset.Y)).ToString(CultureInfo.InvariantCulture)}&extendedZone=false", cancellationToken);
        if (httpResponse.IsSuccessStatusCode)
        {
            // On override le Header car Orange l'envoi parfois en double
            httpResponse.Content.Headers.ContentType = new("application/json");
            var tempResult = JsonSerializer.Deserialize<FiberResponseModel>(await httpResponse.Content.ReadAsStringAsync(cancellationToken));
            lock (fibers)
            {
                fibers.AddRange(tempResult.Result.Results);
            }
            if (tempResult.Result.ZoneSize != "GTC" && canIterate)
            {
                fibers.AddRange(await GetFibersForLocAsync(start.X + (k * currentOffset.X), start.Y + (j * currentOffset.Y), 3, false));
            }
            // AddDebugMarker(fibers, (startX, startY), j, k, currentOffsetY, currentOffsetX, squareSize, modul, tempResult.Result.ZoneSize);
        }
        // Cas du token qui expire pendant le process, on le refresh et on relance l'iteration actuelle
        else if (httpResponse.StatusCode == System.Net.HttpStatusCode.Forbidden && !tokenAlreadyRenewed)
        {
            SetToken();
            await ProcessCurrentArea(fibers, start, currentOffset, j, k, false, cancellationToken, true);
        }
    }

    private static void AddDebugMarker(List<FiberPoint> fibers, (double X, double Y) startPoint, int j, int k, double currentOffsetY, double currentOffsetX, int squareSize, double modul, string zoneSize)
    {
        lock(fibers)
        {
            fibers.Add(new FiberPoint()
            {
                Address = new()
                {
                    BestCoords = new()
                    {
                        Coord = new()
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
        lock (tokenParser)
        {
            if (tokenParser.Token == null || tokenParser.TokenAge < DateTime.Now.AddHours(-1))
            {
                tokenParser.GetToken(GetOrangeToken(tokenParser.GenerateAppId()));
            }
            if (client.DefaultRequestHeaders.Contains("AppAuthorization"))
            {
                client.DefaultRequestHeaders.Remove("AppAuthorization");
            }
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

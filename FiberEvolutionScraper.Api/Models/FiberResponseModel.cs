using System.Text.Json.Serialization;

namespace FiberEvolutionScraper.Api.Models;

public class FiberResponseModel
{
    [JsonPropertyName("result")]
    public Result Result { get; set; }

    [JsonPropertyName("controller")]
    public string Controller { get; set; }

    [JsonPropertyName("date")]
    public string Date { get; set; }

    [JsonPropertyName("serviceCalls")]
    public List<object> ServiceCalls { get; set; }
}

public class Result
{
    [JsonPropertyName("partialResult")]
    public bool PartialResult { get; set; }

    [JsonPropertyName("zoneSize")]
    public string ZoneSize { get; set; }

    [JsonPropertyName("results")]
    public List<FiberPoint> Results { get; set; } = [];
}

public class FiberPoint
{
    [JsonPropertyName("address")]
    public Address Address { get; set; }

    [JsonPropertyName("ftthLoaded")]
    public bool FtthLoaded { get; set; }

    [JsonPropertyName("eligibilitesFtth")]
    public List<EligibilitesFtth> EligibilitesFtth { get; set; } = [];
}

public class Address
{
    [JsonPropertyName("codeCommune")]
    public string CodeCommune { get; set; }

    [JsonPropertyName("codeVoie")]
    public string CodeVoie { get; set; }

    [JsonPropertyName("numVoie")]
    public string NumVoie { get; set; }

    [JsonPropertyName("extVoie")]
    public string ExtVoie { get; set; }

    [JsonPropertyName("signature")]
    public string Signature { get; set; }

    [JsonPropertyName("codePostal")]
    public string CodePostal { get; set; }

    [JsonPropertyName("libCommune")]
    public string LibCommune { get; set; }

    [JsonPropertyName("libVoie")]
    public string LibVoie { get; set; }

    [JsonPropertyName("libAdresse")]
    public string LibAdresse { get; set; }

    [JsonPropertyName("bestCoords")]
    public BestCoords BestCoords { get; set; }
}

public class BestCoords
{
    [JsonPropertyName("coord")]
    public Coord Coord { get; set; }
}

public class Coord
{
    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }
}

public class EligibilitesFtth
{
    [JsonPropertyName("batiment")]
    public string Batiment { get; set; }

    [JsonPropertyName("codeImb")]
    public string CodeImb { get; set; }

    [JsonPropertyName("coord")]
    public BestCoords Coord { get; set; }

    [JsonPropertyName("etapeFtth")]
    public string EtapeFtth { get; set; }

    [JsonPropertyName("dateDebutEligibilite")]
    public DateTime? DateDebutEligibilite { get; set; }
}
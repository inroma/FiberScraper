using System.Text.Json.Serialization;
using static FiberEvolutionScraper.Api.Models.FiberResponseModel;

namespace FiberEvolutionScraper.Api.Models;

public class FiberResponseModel
{
    [JsonPropertyName("partialResult")]
    public bool PartialResult { get; set; }

    [JsonPropertyName("zoneSize")]
    public string ZoneSize { get; set; }

    [JsonPropertyName("results")]
    public List<FiberPoint> Results { get; set; } = new();


    public class Address
    {
        [JsonPropertyName("codeCommune")]
        public string codeCommune { get; set; }
        [JsonPropertyName("codeVoie")]
        public string codeVoie { get; set; }
        [JsonPropertyName("numVoie")]
        public string numVoie { get; set; }
        [JsonPropertyName("extVoie")]
        public string extVoie { get; set; }
        [JsonPropertyName("signature")]
        public string signature { get; set; }
        [JsonPropertyName("codePostal")]
        public string codePostal { get; set; }
        [JsonPropertyName("libCommune")]
        public string libCommune { get; set; }
        [JsonPropertyName("libVoie")]
        public string libVoie { get; set; }
        [JsonPropertyName("libAdresse")]
        public string libAdresse { get; set; }
        [JsonPropertyName("coords")]
        public IList<BestCoords> coords { get; set; }
        [JsonPropertyName("streetHasNumber")]
        public bool streetHasNumber { get; set; }
        [JsonPropertyName("bestCoords")]
        public BestCoords bestCoords { get; set; }
    }

    public class BestCoords
    {
        [JsonPropertyName("coord")]
        public Coord Coord { get; set; }

        [JsonPropertyName("origin")]
        public string Origin { get; set; }

        [JsonPropertyName("accuracy")]
        public object Accuracy { get; set; }
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
        public string batiment { get; set; }
        public Coord coord { get; set; }
        public string etapeFtth { get; set; }
        public object dateDebutEligibilite { get; set; }
        public object syndicStatusCode { get; set; }
        public string codeImb { get; set; }
        public object dateMescFT { get; set; }
        public object etatImmeuble { get; set; }
        public object etatImmeuble360 { get; set; }
    }

}

public class FiberPoint
{
    [JsonPropertyName("address")]
    public Address address { get; set; }
    [JsonPropertyName("ftthLoaded")]
    public bool ftthLoaded { get; set; }
    [JsonPropertyName("eligibilitesFtth")]
    public List<EligibilitesFtth> eligibilitesFtth { get; set; } = new();
    [JsonPropertyName("xdslLoaded")]
    public bool xdslLoaded { get; set; }
    [JsonPropertyName("xdslFound")]
    public bool xdslFound { get; set; }
    [JsonPropertyName("xdslOffresLoaded")]
    public bool xdslOffresLoaded { get; set; }
    [JsonPropertyName("offres")]
    public IList<object> offres { get; set; }
    [JsonPropertyName("elig360")]
    public object elig360 { get; set; }
    [JsonPropertyName("mobileCoverage")]
    public object mobileCoverage { get; set; }
}
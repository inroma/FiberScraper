using FiberEvolutionScraper.Api.Models.User;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FiberEvolutionScraper.Api.Models;

[Table("AutoRefreshInput")]
public class AutoRefreshInput : BaseModel
{
    public bool Enabled { get; set; }

    public double CoordX { get; set; }

    public double CoordY { get; set; }

    public string Label { get; set; }

    public int AreaSize { get; set; } = 5;

    public DateTime? LastRun { get; set; }

    public int UserId { get; set; }

    [JsonIgnore]
    public virtual UserModel User { get; set; }
}
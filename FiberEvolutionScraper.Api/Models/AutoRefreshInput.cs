using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace FiberEvolutionScraper.Api.Models;

[Table("AutoRefreshInput")]
[PrimaryKey(nameof(Id))]
public class AutoRefreshInput
{
    public int Id { get; set; }

    public bool Enabled { get; set; }

    public double CoordX { get; set; }

    public double CoordY { get; set; }

    public string Label { get; set; }

    public int AreaSize { get; set; } = 5;

    public DateTime? LastRun { get; set; }
}
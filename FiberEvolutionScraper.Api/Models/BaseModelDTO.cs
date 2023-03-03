using System.ComponentModel.DataAnnotations;

namespace FiberEvolutionScraper.Api.Models;

public abstract class BaseModelDTO
{
    [Required]
    public DateTime Created { get; set; }

    [Required]
    public DateTime LastUpdated { get; set; }
}

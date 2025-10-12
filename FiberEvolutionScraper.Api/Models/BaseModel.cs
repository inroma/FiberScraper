using FiberEvolutionScraper.Api.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FiberEvolutionScraper.Api.Models;

[PrimaryKey(nameof(Id))]
public abstract class BaseModel : IBaseModel<int>
{
    public virtual int Id { get; set; }

    [Required]
    public DateTime Created { get; set; }

    [Required]
    public DateTime LastUpdated { get; set; }
}

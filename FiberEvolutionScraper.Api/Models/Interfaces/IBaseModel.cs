namespace FiberEvolutionScraper.Api.Models.Interfaces;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public interface IBaseModel<T>
{
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public T Id { get; set; }

    public DateTime Created { get; set; }

    public DateTime LastUpdated { get; set; }
}

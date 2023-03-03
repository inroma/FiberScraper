using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace FiberEvolutionScraper.Api.Models;

[Table("FiberPoint")]
[PrimaryKey(nameof(Signature))]
public class FiberPointDTO : BaseModelDTO
{
    public string Signature { get; set; }

    public string LibAdresse { get; set; }

    public double X { get; set; }

    public double Y { get; set; }

    [InverseProperty("FiberPoint")]
    public List<EligibiliteFtthDTO> EligibilitesFtth { get; set; } = new List<EligibiliteFtthDTO>();
}
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FiberEvolutionScraper.Api.Models;

[Table("EligibiliteFtth")]
[PrimaryKey(nameof(CodeImb), nameof(EtapeFtth), nameof(FiberPointDTOSignature))]
public class EligibiliteFtth : BaseModel
{
    public string Batiment { get; set; }

    public string CodeImb { get; set; }

    public EtapeFtth EtapeFtth { get; set; } = EtapeFtth._;

    public DateTime? DateDebutEligibilite { get; set; }

    [ForeignKey(nameof(FiberPointDTOSignature))]
    [JsonIgnore]
    public FiberPoint FiberPoint { get; set; }

    public string FiberPointDTOSignature { get; set; }
}
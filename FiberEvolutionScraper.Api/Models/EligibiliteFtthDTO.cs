using System.ComponentModel.DataAnnotations.Schema;

namespace FiberEvolutionScraper.Api.Models;

[Table("EligibiliteFtth")]
public class EligibiliteFtthDTO : BaseModelDTO
{
    public string Batiment { get; set; }

    public string CodeImb { get; set; }

    public EtapeFtth EtapeFtth { get; set; } = EtapeFtth._;

    public DateTime? DateDebutEligibilite { get; set; }
}
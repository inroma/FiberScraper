﻿using System.ComponentModel.DataAnnotations.Schema;

namespace FiberEvolutionScraper.Api.Models;

[Table("FiberPoint")]
public class FiberPointDTO : BaseModelDTO
{
    public string CodeCommune { get; set; }

    public string CodeVoie { get; set; }

    public string NumVoie { get; set; }

    public string ExtVoie { get; set; }

    public string Signature { get; set; }

    public string CodePostal { get; set; }

    public string LibCommune { get; set; }

    public string LibVoie { get; set; }

    public string LibAdresse { get; set; }

    public double X { get; set; }

    public double Y { get; set; }

    public bool FtthLoaded { get; set; }

    public EtapeFtth EtapeFtth { get; set; }

    public string Batiment { get; set; }

    public string CodeImb { get; set; }
}

public enum EtapeFtth
{
    ELLIGIBLE,
    EN_COURS_IMMEUBLE,
    TERMINE_QUARTIER,
    EN_COURS_QUARTIER,
    PREVU_QUARTIER,
    _,
    DEBUG,
    UNKNOWN = 999
}
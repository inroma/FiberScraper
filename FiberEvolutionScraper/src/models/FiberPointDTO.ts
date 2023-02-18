export default class FiberPointDTO
{
    codeCommune = "";

    codeVoie = "";

    numVoie = "";

    extVoie = "";

    signature = "";

    codePostal = "";

    libCommune = "";

    libVoie = "";

    libAdresse = "";

    batiment = "";

    codeImb = "";

    x = 0.0;

    y = 0.0;

    ftthLoaded = false;

    etapeFtth = EtapeFtth._;

    created = new Date();

    lastUpdated = new Date();
}

export enum EtapeFtth
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
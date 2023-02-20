import BaseModelDTO from "./BaseModelDTO";
import EligibiliteFtthDTO from "./EligibiliteFtthDTO";

export default class FiberPointDTO implements BaseModelDTO
{
    created = new Date();

    lastUpdated = new Date();

    eligibilitesFtth: EligibiliteFtthDTO[] = [];

    codeCommune = "";

    codeVoie = "";

    numVoie = "";

    extVoie = "";

    signature = "";

    codePostal = "";

    libCommune = "";

    libVoie = "";

    libAdresse = "";

    x = 0.0;

    y = 0.0;

    iconUrl = "";

    iconClassName = "";
}
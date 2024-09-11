import BaseModelDTO from "./BaseModelDTO";
import { EtapeFtth } from "./Enums";

export default class EligibiliteFtth implements BaseModelDTO
{
    created = new Date();

    lastUpdated = new Date();

    batiment = "";
    
    codeImb = "";

    etapeFtth = EtapeFtth._;

    dateDebutEligibilite = new Date();
}
import type BaseModelDTO from "./BaseModelDTO";
import EligibiliteFtthDTO from "./EligibiliteFtthDTO";

export default class FiberPointDTO implements BaseModelDTO
{
    created = new Date();

    lastUpdated = new Date();

    eligibilitesFtth: EligibiliteFtthDTO[] = [];

    signature = "";

    libAdresse = "";

    x = 0.0;

    y = 0.0;

    iconUrl = "";

    opacity = 1;
}
export class FiberResponseModel {
    partialResult: boolean = false;
    zoneSize: string = "";
    results: FiberPoint[] = [];
}

interface Address
{
    codeCommune: string;
    codeVoie: string;
    numVoie: string;
    extVoie: string;
    signature: string;
    codePostal: string;
    libCommune: string;
    libVoie: string;
    libAdresse: string;
    coords: Coord[];
    streetHasNumber: boolean;
    bestCoords: BestCoords;
}

interface BestCoords
{
    coord: Coord;
    origin: string;
    accuracy: object;
}

interface Coord
{
    x: number;
    y: number;
}

interface EligibilitesFtth
{
    batiment: string;
    coord: Coord;
    etapeFtth: string;
    dateDebutEligibilite: object;
    syndicStatusCode: object;
    codeImb: string;
    dateMescFT: object;
    etatImmeuble: object;
    etatImmeuble360: object;
}

export interface FiberPoint
{
    address: Address;
    ftthLoaded: boolean;
    eligibilitesFtth: EligibilitesFtth[];
    xdslLoaded: boolean;
    xdslFound: boolean;
    xdslOffresLoaded: boolean;
    offres: object[];
    elig360: object;
    mobileCoverage: object;
}
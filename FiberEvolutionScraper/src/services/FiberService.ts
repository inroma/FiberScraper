import FiberPointDTO from '@/models/FiberPointDTO';
import { type AxiosPromise } from 'axios';
import { AbstractApiService } from './AbstractApiService';
import type MinMaxBounds from '@/models/MinMaxBounds';

class FiberService extends AbstractApiService {

    constructor() {
        super();
        this.url = '/api/v1/fiber/';
    }

    getCloseAreaFibers(coord: Array<number>): AxiosPromise<FiberPointDTO[]> {
        return this.httpClient.get<FiberPointDTO[]>(`${this.url}GetCloseArea`, { params: { coordX: coord.at(0), coordY: coord.at(1) }});
    }
    
    updateWideArea(coord: Array<number>): AxiosPromise<number> {
        return this.httpClient.get<number>(`${this.url}UpdateWideArea`, { params: { coordX: coord.at(0), coordY: coord.at(1) }});
    }

    getWideArea(coord: Array<number>): AxiosPromise<FiberPointDTO[]> {
        return this.httpClient.get<FiberPointDTO[]>(`${this.url}GetWideArea`, { params: { coordX: coord.at(0), coordY: coord.at(1) }});
    }

    getDbFibers(bounds: MinMaxBounds): AxiosPromise<FiberPointDTO[]> {
        return this.httpClient.get<FiberPointDTO[]>(`${this.url}GetFibers`, { params: bounds });
    }

    getNewestPoints(bounds: MinMaxBounds): AxiosPromise<FiberPointDTO[]> {
        return this.httpClient.get<FiberPointDTO[]>(`${this.url}GetNewestPoints`, { params: bounds });
    }
  
    getHistorique(signature: string): AxiosPromise<FiberPointDTO> {
        return this.httpClient.get<FiberPointDTO>(`${this.url}GetSameSignaturePoints`, { params: { signature: signature }});
    }
}

export const fiberService = new FiberService();
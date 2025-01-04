import FiberPointDTO from '@/models/FiberPointDTO';
import axios, { type AxiosPromise } from 'axios';

export default class FiberService {

    private static readonly endpoint = '/api/v1/fiber/';
    private static readonly headers = { 'Content-Type': 'application/json' };

    static getCloseAreaFibers(coord: Array<number>): AxiosPromise<FiberPointDTO[]> {
        return axios.get<FiberPointDTO[]>(`${this.endpoint}GetCloseArea`, { headers: this.headers,
            params: { coordX: coord.at(0), coordY: coord.at(1) }});
    }
    
    static updateWideArea(coord: Array<number>): AxiosPromise<number> {
        return axios.get<number>(`${this.endpoint}UpdateWideArea`, { headers: this.headers,
        params: { coordX: coord.at(0), coordY: coord.at(1) }});
    }

    static getWideArea(coord: Array<number>): AxiosPromise<FiberPointDTO[]> {
        return axios.get<FiberPointDTO[]>(`${this.endpoint}GetWideArea`, { headers: this.headers,
        params: { coordX: coord.at(0), coordY: coord.at(1) }});
    }

    static getDbFibers(coord: Array<number>): AxiosPromise<FiberPointDTO[]> {
        return axios.get<FiberPointDTO[]>(`${this.endpoint}GetFibers`, { headers: this.headers,
        params: { coordX: coord.at(0), coordY: coord.at(1) }});
    }

    static getNewestPoints(bounds: string): AxiosPromise<FiberPointDTO[]> {
        return axios.get<FiberPointDTO[]>(`${this.endpoint}GetNewestPoints`, { headers: this.headers,
            params: { data: bounds }});
    }
  
    static getHistorique(signature: string): AxiosPromise<FiberPointDTO> {
        return axios.get<FiberPointDTO>(`${this.endpoint}GetSameSignaturePoints`, 
            { headers: this.headers, params: { signature: signature }});
    }
}
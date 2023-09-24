import FiberPointDTO from '@/models/FiberPointDTO';
import axios, { AxiosPromise } from 'axios';

export default class FiberService {

    private static readonly endpoint = 'https://localhost:5001/api/fiber/';
    private static readonly headers = { 'Content-Type': 'application/json' };

    static getCloseAreaFibers(lat: number, lng: number): AxiosPromise<FiberPointDTO[]> {
        return axios.get<FiberPointDTO[]>(`${this.endpoint}GetCloseArea`, { headers: this.headers,
            params: { coordX: lat, coordY: lng }});
    }
    
    static updateWideArea(lat: number, lng: number): AxiosPromise<number> {
        return axios.get<number>(`${this.endpoint}UpdateWideArea`, { headers: this.headers,
        params: { coordX: lat, coordY: lng }});
    }

    static getWideArea(lat: number, lng: number): AxiosPromise<FiberPointDTO[]> {
        return axios.get<FiberPointDTO[]>(`${this.endpoint}GetWideArea`, { headers: this.headers,
        params: { coordX: lat, coordY: lng }});
    }

    static getDbFibers(lat: number, lng: number): AxiosPromise<FiberPointDTO[]> {
        return axios.get<FiberPointDTO[]>(`${this.endpoint}GetFibers`, { headers: this.headers,
        params: { coordX: lat, coordY: lng }});
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
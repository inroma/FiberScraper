import AutoRefreshInput from '@/models/AutoRefreshInput';
import axios, { AxiosPromise, AxiosResponse } from 'axios';

export default class AutoRefreshService {

    private static readonly endpoint = 'https://localhost:5001/api/autorefresh/';

    static getAll(): AxiosPromise<AutoRefreshInput[]> {
        return axios.get<AutoRefreshInput[]>(`${this.endpoint}GetAll`);
    }
    
    static add(refreshInput: AutoRefreshInput): AxiosPromise<number> {
        return axios.post<AutoRefreshInput, AxiosResponse<number>>(`${this.endpoint}Add`, refreshInput);
    }
    
    static update(refreshInput: AutoRefreshInput): AxiosPromise<number> {
        return axios.patch<AutoRefreshInput, AxiosResponse<number>>(`${this.endpoint}Update`, refreshInput);
    }

    static delete(inputId: number): AxiosPromise<number> {
        return axios.delete<number>(`${this.endpoint}Delete`, { params: { inputId: inputId }});
    }
}
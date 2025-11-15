import AutoRefreshInput from '@/models/AutoRefreshInput';
import  { type AxiosPromise, type AxiosResponse } from 'axios';
import { AbstractApiService } from './AbstractApiService';

class AutoRefreshService extends AbstractApiService {

    constructor() {
        super();
        this.url = 'autorefresh/';
    }

    getAll(): AxiosPromise<AutoRefreshInput[]> {
        return this.httpClient.get<AutoRefreshInput[]>(`${this.url}GetAll`);
    }
    
    add(refreshInput: AutoRefreshInput): AxiosPromise<number> {
        return this.httpClient.post<AutoRefreshInput, AxiosResponse<number>>(`${this.url}Add`, refreshInput);
    }
    
    update(refreshInput: AutoRefreshInput): AxiosPromise<number> {
        return this.httpClient.patch<AutoRefreshInput, AxiosResponse<number>>(`${this.url}Update`, refreshInput);
    }

    delete(inputId: number): AxiosPromise<number> {
        return this.httpClient.delete<number>(`${this.url}Delete`, { params: { inputId: inputId }});
    }

    runAll(): AxiosPromise {
        return this.httpClient.post(`${this.url}RunAll`);
    }
}

export const autoRefreshService = new AutoRefreshService();
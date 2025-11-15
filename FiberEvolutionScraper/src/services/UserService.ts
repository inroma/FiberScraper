import { type AxiosPromise } from 'axios';
import { AbstractApiService } from './AbstractApiService';
import type UserModel from '@/models/auth/userModel';

class UserService extends AbstractApiService {

    constructor() {
        super();
        this.url = 'user/';
    }
    
    syncUser(): AxiosPromise<UserModel> {
        return this.httpClient.post<UserModel>(`${this.url}syncUser`);
    }

    deleteAccount(): AxiosPromise<boolean> {
        return this.httpClient.delete<boolean>(`${this.url}`);
    }
}

export const userService = new UserService();
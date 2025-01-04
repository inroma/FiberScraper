import type { AddressResponse } from '@/models/address/addressResponse';
import { AbstractApiService } from './AbstractApiService';

class AddressService extends AbstractApiService {

	constructor() {
		super();
		this.url = `https://api-adresse.data.gouv.fr/`;
	}
	
	public search(search: string): Promise<AddressResponse> {
		if (!search) {
			return Promise.reject("no query");
		}
		return this.http.get<AddressResponse>(this.url + 'search/',
			{ params: { 'q': search, 'limit': '10', 'autocomplete': '1' } }
		)
		.then(this.handleResponse.bind(this))
		.catch(this.handleError.bind(this));
	}
}

export const addressService = new AddressService();
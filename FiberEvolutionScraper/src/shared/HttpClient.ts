import type { Axios } from "axios";
import axios from "axios";
import { auth } from "./services/auth/OAuthService";
import router from "@/router";

let httpClient: Axios = axios.create({
  headers: {
    "Content-Type": "application/json"
  }
});

httpClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response) {
      if (error.response.status === 401) {
        // Redirect to login page
        router.push('/auth/login')
      }
    }
    return Promise.reject(error)
  },
);

export class HttpClient {

  public static setTokenHeader = (token: string) => {
    httpClient.defaults.headers.common.Authorization = 'Bearer ' + token;
  }
}

export default httpClient;
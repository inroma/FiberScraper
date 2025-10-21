import type { Axios } from "axios";
import axios from "axios";
import router from "@/router";
import { useAuth } from "./composables/OAuthComposable";

let httpClient: Axios = axios.create({
  headers: {
    "Content-Type": "application/json"
  }
});

httpClient.interceptors.request.use(
  async (request) => {
    const auth = useAuth();
    request.headers.Authorization = 'Bearer ' + (await auth.getUser()).access_token
    return request;
  }
);

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

export default httpClient;
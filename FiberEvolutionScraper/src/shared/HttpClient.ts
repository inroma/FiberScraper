import type { Axios } from "axios";
import axios from "axios";
import router from "@/router";
import { useUserStore } from "@/store/userStore";

let httpClient: Axios = axios.create({
  headers: {
    "Content-Type": "application/json"
  }
});

httpClient.interceptors.request.use(
  async (request) => {
    const userStore = useUserStore();
    request.headers.Authorization = 'Bearer ' + (await userStore.getUser())?.access_token
    return request;
  }
);

httpClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response) {
      if (error.response.status === 401) {
        const userStore = useUserStore();
        await userStore.renewToken();
        // Redirect to login page
        router.push('/auth/login');
      }
    }
    return Promise.reject(error)
  },
);

export default httpClient;
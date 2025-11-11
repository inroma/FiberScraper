import type UserModel from '@/models/auth/userModel'
import router from '@/router';
import { userService } from '@/services/UserService';
import { WebStorageStateStore, type UserManagerSettings, UserManager } from 'oidc-client-ts';
import { defineStore } from 'pinia'

const settings = {
  authority: import.meta.env.VITE_AUTHORITY_URL,
  client_id: import.meta.env.VITE_CLIENT_ID,
  redirect_uri: `${window.location.origin}/auth/callback`,
  silent_redirect_uri: `${window.location.origin}/silent-refresh`,
  post_logout_redirect_uri: `${window.location.origin}`,
  response_type: 'code',
  scope: import.meta.env.VITE_CLIENT_SCOPE,
  userStore: new WebStorageStateStore(),
  loadUserInfo: true,
  monitorSession: true,
  automaticSilentRenew: true
} as UserManagerSettings;
const userManager = new UserManager(settings);
userManager.events.addAccessTokenExpired(async () => {
  await userManager.signinSilent();
});

export const useUserStore = defineStore('user', {
  state: () => ({
    user: null as UserModel | null,
    loginLoading: false
  }),
  getters: {
    isConnected(state) {
      return state.user !== null && state.user !== undefined
    }
  },
  actions: {
    signInRedirect() {
      this.loginLoading = true
      return userManager.signinRedirect()
      .finally(() => this.loginLoading = false);
    },
    async signInCallback() {
      this.loginLoading = true;
      if (await userManager.getUser()) {
        this.user = (await userService.syncUser())?.data;
        this.loginLoading = false;
        return;
      }
      userManager.signinCallback()
        .then(async () => {
			    this.user = (await userService.syncUser())?.data;
        })
        .catch(() => { this.user = null })
        .finally(() => this.loginLoading = false);
    },
    async renewToken() {
      await userManager.removeUser();
      userManager.startSilentRenew();
      
      this.loginLoading = true;
      if (await userManager.getUser()) {
        this.user = (await userService.syncUser())?.data;
        this.loginLoading = false;
      }
    },
    logout() {
      this.user = null;
      userManager.signoutSilent();
      return router.push({ path: '/', force: true });
    },
    logoutSilent() {
      this.user = null;
      return userManager.signoutSilent()
    },
    logoutWithRedirect() {
      this.user = null;
      return userManager.signoutRedirect()
    },
    getUser() {
      return userManager.getUser()
    }
  },
  persist: true
});
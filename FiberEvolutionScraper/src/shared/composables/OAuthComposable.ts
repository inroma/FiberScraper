import { UserManager, WebStorageStateStore, type UserManagerSettings } from 'oidc-client-ts';

const isConnected = ref(false);
const settings = {
  authority: import.meta.env.VITE_AUTHORITY_URL,
  client_id: import.meta.env.VITE_CLIENT_ID,
  client_secret: import.meta.env.VITE_CLIENT_SECRET,
  redirect_uri: `${window.location.origin}/auth/callback`,
  silent_redirect_uri: `${window.location.origin}/silent-refresh`,
  post_logout_redirect_uri: `${window.location.origin}`,
  response_type: 'code',
  scope: import.meta.env.VITE_CLIENT_SCOPE,
  userStore: new WebStorageStateStore(),
  loadUserInfo: true,
  monitorSession: true
} as UserManagerSettings;
const userManager = new UserManager(settings);
watchEffect(async () => isConnected.value = (await userManager?.getUser())?.access_token !== undefined);

export const useAuth = () => {

  const signInRedirect = () => {
    return userManager.signinRedirect()
  }

  const signInCallback = () => {
    return userManager.signinCallback();
  }

  const renewToken = () => {
    return userManager.signinSilentCallback()
  }

  const logout = () => {
    return userManager.signoutRedirect()
  }

  const getUser = () => userManager.getUser();

  return { userManager, isConnected, signInRedirect, signInCallback, renewToken, logout, getUser }
}
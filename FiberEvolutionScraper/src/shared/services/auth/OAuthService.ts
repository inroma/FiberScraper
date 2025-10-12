import { User, UserManager, WebStorageStateStore, type UserManagerSettings } from 'oidc-client-ts';

let instance: OAuthService;

export const auth = () => {
  if (!instance) {
    instance = new OAuthService();
  }
  return instance;
};

export default class OAuthService {
  public userManager: UserManager;
  public isConnected: boolean;

  constructor() {
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
    this.userManager = new UserManager(settings);
    this.userManager.events.addUserLoaded(() => { this.isConnected = true });
    this.userManager.events.addUserSignedIn(() => { this.isConnected = true });
    this.userManager.events.addUserUnloaded(() => { this.isConnected = false });
    this.userManager.events.addUserSignedOut(() => { this.isConnected = false });
  }

  public signInRedirect() {
    return this.userManager.signinRedirect()
  }

  public signInCallback() {
    return this.userManager.signinCallback()
  }

  public renewToken(): Promise<void> {
    return this.userManager.signinSilentCallback()
  }

  public logout(): Promise<void> {
    return this.userManager.signoutRedirect()
  }

  public getUser(): Promise<User | null> {
    return this.userManager.getUser()
  }
}
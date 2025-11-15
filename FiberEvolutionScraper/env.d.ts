/// <reference types="vite/client" />

interface ImportMetaEnv {
    readonly VITE_APP_TITLE: string
    readonly VITE_BACKEND_URL: string
    readonly VITE_AUTHORITY_URL: string
    readonly VITE_CLIENT_ID: string
    readonly VITE_CLIENT_SCOPE: string
    // more env variables...
}
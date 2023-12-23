import { fileURLToPath, URL } from 'node:url';
import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'
import vuetify from 'vite-plugin-vuetify'

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
    const env = loadEnv(mode, process.cwd(), '');
    return { 
        plugins: [vue(), vuetify()],
        resolve: {
            alias: {
                '@': fileURLToPath(new URL('./src', import.meta.url))
            }
        },
        server: {
            cors: true,
            proxy: {
                '/api/v1/': {
                    target: env.VITE_BACKEND_URL,
                    secure: false,
                    changeOrigin: true,
                    rewrite: (path) => path.replace(/^\/api\/v1\//, 'api/'),
                }
            },
            port: 8888
        }
    }
})
import { fileURLToPath, URL } from 'node:url';
import { defineConfig, loadEnv } from 'vite';
import AutoImport from 'unplugin-auto-import/vite';
import Components from 'unplugin-vue-components/vite';
import Vuetify from 'vite-plugin-vuetify';
import Vue from '@vitejs/plugin-vue';

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
    const env = loadEnv(mode, process.cwd(), '');
    return { 
        plugins: [
            AutoImport({
                imports: [
                    'vue',
                    {
                        'vue-router/auto': ['useRoute', 'useRouter'],
                    }
                    ],
                    dts: 'src/auto-imports.d.ts',
                    eslintrc: {
                        enabled: true,
                    },
                    vueTemplate: true,
                }),
                Components({
                  dts: 'src/components.d.ts',
                }),
                Vue(),
                Vuetify({
                    autoImport: true,
                    styles: {
                        configFile: 'src/styles/settings.scss',
                    },
                }),
        ],
        resolve: {
            alias: {
                '@': fileURLToPath(new URL('./src', import.meta.url))
            },
            extensions: [
              '.js',
              '.json',
              '.jsx',
              '.mjs',
              '.ts',
              '.tsx',
              '.vue',
            ],
        },
        server: {
            host: true,
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
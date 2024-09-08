import { createPinia } from 'pinia'
import { createApp } from 'vue'
import App from "./App.vue";
import router from "./router";
import { createVuetify } from 'vuetify'
import { fr, en } from 'vuetify/locale'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import 'vuetify/dist/vuetify.min.css';
import '@mdi/font/css/materialdesignicons.css';
import 'vue3-openlayers/dist/vue3-openlayers.css';
import OpenLayersMap from "vue3-openlayers";
import { useGeographic } from 'ol/proj';

const pinia = createPinia();
const app = createApp(App).use(pinia);

const vuetify = createVuetify({
  components,
  directives,
  icons: {
    defaultSet: 'mdi',
  },
  theme: {
    defaultTheme: 'dark'
  },
  locale: {
    locale: 'fr',
    fallback: 'en',
    messages: { fr, en },
  },
  defaults: {
    global: {
      density: "comfortable"
    },
    VBtn: {
      density: "default"
    }
  }
});
useGeographic();

app
  .use(router)
  .use(OpenLayersMap)
  .use(vuetify).mount("#app");

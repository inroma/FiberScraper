import { createPinia } from 'pinia'
import App from "./App.vue";
import router from "./router";
import { createVuetify } from 'vuetify'
import { fr, en } from 'vuetify/locale'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import 'vuetify/dist/vuetify.min.css';
import '@mdi/font/css/materialdesignicons.css';
import 'vue3-openlayers/vue3-openlayers.css';
import 'ol/ol.css';
import OpenLayersMap from "vue3-openlayers";
import { useGeographic } from 'ol/proj';
import minMax from 'dayjs/plugin/minMax';
import piniaPluginPersistedState from "pinia-plugin-persistedstate"
import dayjs from 'dayjs'
import isLeapYear from 'dayjs/plugin/isLeapYear'
import 'dayjs/locale/fr'

dayjs.extend(minMax) // use plugin
dayjs.extend(isLeapYear) // use plugin
dayjs.locale('fr') // use locale

const pinia = createPinia();
pinia.use(piniaPluginPersistedState);
const app = createApp(App);

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

app
  .use(vuetify)
  .use(router)
  .use(pinia)
  .use(OpenLayersMap, useGeographic())
  .mount("#app");

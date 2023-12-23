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

app.use(router).use(vuetify).mount("#app");

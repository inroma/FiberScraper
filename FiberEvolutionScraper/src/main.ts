import Vue from "vue";
import App from "./App.vue";
import router from "./router";
import store from "./store";
import Vuetify from "vuetify";
import 'vuetify/dist/vuetify.min.css';
import '@mdi/font/css/materialdesignicons.css';

Vue.config.productionTip = false;

const vuetify = new Vuetify({
  theme: { dark: true },
  icons: {
    iconfont: 'mdi',
  },
});

Vue.use(Vuetify, vuetify);

new Vue({
  router,
  store,
  vuetify,
  render: (h) => h(App),
}).$mount("#app");

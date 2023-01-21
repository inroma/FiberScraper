import Vue from "vue";
import Vuex from "vuex";
import { toastStore } from "./ToastStore";

Vue.use(Vuex);

export default new Vuex.Store({
  state: {},
  getters: {},
  mutations: {},
  actions: {},
  modules: {
    ToastStore: toastStore
  },
});

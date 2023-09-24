import FiberMapVue from "@/views/fibers/FiberMap.vue";
import Vue from "vue";
import VueRouter, { RouteConfig } from "vue-router";
import HomeView from "../views/HomeView.vue";
import AutoRefreshView from "@/views/autorefresh/AutoRefreshView.vue";

Vue.use(VueRouter);

const routes: Array<RouteConfig> = [
  {
    path: "/",
    name: "fibermap",
    component: FiberMapVue,
  },
  {
    path: "/auto-refresh",
    name: "autorefresh",
    component: AutoRefreshView,
  },
  {
    path: "/home",
    name: "home",
    component: HomeView,
  }
];

const router = new VueRouter({
  mode: "history",
  base: process.env.BASE_URL,
  routes,
});

export default router;

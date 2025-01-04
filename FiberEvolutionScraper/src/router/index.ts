import FiberMapVue from "@/views/fibers/FiberMap.vue";
import { type RouteRecordRaw, createRouter, createWebHistory } from "vue-router";
import HomeView from "@/views/HomeView.vue";
import AutoRefreshView from "@/views/autorefresh/AutoRefreshView.vue";

const routes: Array<RouteRecordRaw> = [
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

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;

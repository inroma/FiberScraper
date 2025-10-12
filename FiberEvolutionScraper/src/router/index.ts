import FiberMapVue from "@/views/fibers/FiberMap.vue";
import { type RouteRecordRaw, createRouter, createWebHistory } from "vue-router";
import AutoRefreshView from "@/views/autorefresh/AutoRefreshView.vue";
import Callback from "@/views/auth/Callback.vue";
import Login from "@/views/auth/Login.vue";

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
    meta: { requireAuth: true },
  },
  {
    path: "/auth/callback",
    name: "authCallback",
    component: Callback,
  },
  {
    path: "/auth/login",
    name: "login",
    component: Login,
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
